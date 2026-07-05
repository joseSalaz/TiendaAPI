using DBModel.DBModels;
using IBussines;
using IRepository;
using IService.FacturacionElectronica;
using Models.ApisPeru;
using Models.RequestResponse;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UtilInterface;
using UtilPaginados.RequestResponse;
using UtilPDF.ComprobantesPdf;
using UtilPDF.ComprobantesPdf.Cache;

namespace Bussines
{
    public partial class VentaBussines : IVentaBussines
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly IVentaDetalleRepository _ventaDetalleRepository;
        private readonly IVentaPagoRepository _ventaPagoRepository;
        private readonly IComprobanteSerieRepository _comprobanteSerieRepository;

        private readonly IProductoRepository _productoRepository;
        private readonly IProductoPresentacionRepository _productoPresentacionRepository;
        private readonly IProductoStockRepository _productoStockRepository;
        private readonly IProductoLoteRepository _productoLoteRepository;
        private readonly IVentaDetalleLoteRepository _ventaDetalleLoteRepository;
        private readonly IStockMovimientoRepository _stockMovimientoRepository;
        private readonly IMedioPagoRepository _medioPagoRepository;
        private readonly ICajaMovimientoRepository _cajaMovimientoRepository;
        private readonly IEmpresaRepository _empresaRepository;
        private readonly IDocumentoElectronicoRepository _documentoElectronicoRepository;
        private readonly IApisPeruFacturacionService _apisPeruFacturacionService;
        private readonly INotaCreditoRepository _notaCreditoRepository;
        private readonly IApisPeruPayloadBuilder _apisPeruPayloadBuilder;
        private readonly IClienteRepository _clienteRepository;
        private readonly IComprobantePdfService _comprobantePdfService;
        private readonly IUnitOfWork _unitOfWork;

        public VentaBussines(
            IVentaRepository ventaRepository,
            IVentaDetalleRepository ventaDetalleRepository,
            IVentaPagoRepository ventaPagoRepository,
            IComprobanteSerieRepository comprobanteSerieRepository,
            IProductoRepository productoRepository,
            IProductoPresentacionRepository productoPresentacionRepository,
            IProductoStockRepository productoStockRepository,
            IProductoLoteRepository productoLoteRepository,
            IVentaDetalleLoteRepository ventaDetalleLoteRepository,
            IStockMovimientoRepository stockMovimientoRepository,
            IMedioPagoRepository medioPagoRepository,
            ICajaMovimientoRepository cajaMovimientoRepository,
            IEmpresaRepository empresaRepository,
            IApisPeruFacturacionService apisPeruFacturacionService,
            IDocumentoElectronicoRepository documentoElectronicoRepository,
            INotaCreditoRepository notaCreditoRepository,
            IApisPeruPayloadBuilder apisPeruPayloadBuilder,
            IClienteRepository clienteRepository,
            IComprobantePdfService comprobantePdfService,

            IUnitOfWork unitOfWork)
        {
            _ventaRepository = ventaRepository;
            _ventaDetalleRepository = ventaDetalleRepository;
            _ventaPagoRepository = ventaPagoRepository;
            _comprobanteSerieRepository = comprobanteSerieRepository;

            _productoRepository = productoRepository;
            _productoPresentacionRepository = productoPresentacionRepository;
            _productoStockRepository = productoStockRepository;
            _productoLoteRepository = productoLoteRepository;
            _ventaDetalleLoteRepository = ventaDetalleLoteRepository;
            _stockMovimientoRepository = stockMovimientoRepository;
            _medioPagoRepository = medioPagoRepository;
            _cajaMovimientoRepository = cajaMovimientoRepository;
            _empresaRepository = empresaRepository;
            _apisPeruFacturacionService = apisPeruFacturacionService;
            _documentoElectronicoRepository = documentoElectronicoRepository;
            _notaCreditoRepository = notaCreditoRepository;
            _apisPeruPayloadBuilder = apisPeruPayloadBuilder;
            _clienteRepository = clienteRepository;
            _comprobantePdfService = comprobantePdfService;
            _unitOfWork = unitOfWork;
        }

        public async Task<RegistrarVentaResponse> RegistrarVentaAsync(RegistrarVentaRequest request)
        {
            ValidarRequest(request);
            ValidarReglasLocalesEmision(request);

            Venta? venta = null;
            object? payloadEmision = null;

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var serie = await _comprobanteSerieRepository.GetSerieActivaAsync(
                    request.SucursalId,
                    request.TipoDocumento,
                    request.Serie
                );

                if (serie == null)
                    throw new Exception("No existe una serie activa para este tipo de comprobante.");

                serie.CorrelativoActual += 1;
                var correlativo = serie.CorrelativoActual;

                decimal totalVentaConIgv = 0;
                decimal descuentoVenta = request.Descuento;

                var clienteIdVenta = await ResolverClienteVentaAsync(request);
                var detallesSunat = new List<DetalleSunatTemporal>();

                venta = new Venta
                {
                    SucursalId = request.SucursalId,
                    CajaId = request.CajaId,
                    CajaSesionId = request.CajaSesionId,
                    UsuarioId = request.UsuarioId,
                    ClienteId = clienteIdVenta,

                    TipoDocumento = request.TipoDocumento,
                    Serie = request.Serie,
                    Correlativo = correlativo,

                    Subtotal = 0,
                    Impuesto = 0,
                    Descuento = descuentoVenta,
                    Total = 0,

                    EstadoSunat = "PENDIENTE_ENVIO",

                    Observaciones = request.Observaciones,
                    FechaCreacion = DateTime.UtcNow
                };

                await _ventaRepository.CreateAsync(venta);
                await _unitOfWork.SaveChangesAsync();

                foreach (var detalleRequest in request.Detalles)
                {
                    var producto = await _productoRepository.GetByIdAsync(detalleRequest.ProductoId);

                    if (producto == null)
                        throw new Exception("Uno de los productos no existe.");

                    var presentacion = await _productoPresentacionRepository.GetByIdAndProductoAsync(
                        detalleRequest.PresentacionId,
                        detalleRequest.ProductoId
                    );

                    if (presentacion == null)
                        throw new Exception("Una presentación no existe o no pertenece al producto.");

                    if (!presentacion.PermiteVenta)
                        throw new Exception($"La presentación {presentacion.Nombre} no permite venta.");

                    var unidadesPorPresentacion = Convert.ToInt32(presentacion.CantidadUnidades);

                    if (unidadesPorPresentacion <= 0)
                        throw new Exception("La presentación tiene cantidad de unidades inválida.");

                    var cantidadUnidades = detalleRequest.Cantidad * unidadesPorPresentacion;

                    var precioUnitarioConIgv = Convert.ToDecimal(presentacion.PrecioVenta);

                    if (precioUnitarioConIgv <= 0)
                        throw new Exception($"La presentación {presentacion.Nombre} no tiene precio de venta válido.");

                    var totalDetalleConIgv = Math.Round(
                        (detalleRequest.Cantidad * precioUnitarioConIgv) - detalleRequest.Descuento,
                        2
                    );

                    if (totalDetalleConIgv < 0)
                        throw new Exception("El total del detalle no puede ser negativo.");


                    var ventaDetalle = new VentaDetalle
                    {
                        VentaId = venta.Id,

                        ProductoId = detalleRequest.ProductoId,
                        PresentacionId = detalleRequest.PresentacionId,

                        CodigoBarras = producto.CodigoBarras,
                        NombreProducto = producto.Nombre,
                        ProductoCodigoInterno = producto.CodigoInterno,
                        ProductoImagenUrl = producto.ImagenUrl,

                        PresentacionNombre = presentacion.Nombre,
                        Cantidad = detalleRequest.Cantidad,
                        CantidadUnidades = cantidadUnidades,

                        UnidadMedida = producto.UnidadMedida,
                        UnidadSunat = "NIU",
                        TipoAfectacionIgv = "10",

                        PrecioUnitario = precioUnitarioConIgv,
                        Descuento = detalleRequest.Descuento,
                        Subtotal = totalDetalleConIgv
                    };

                    await _ventaDetalleRepository.CreateAsync(ventaDetalle);
                    await _unitOfWork.SaveChangesAsync();

                    detallesSunat.Add(new DetalleSunatTemporal
                    {
                        ProductoId = detalleRequest.ProductoId,
                        PresentacionId = detalleRequest.PresentacionId,
                        CodProducto = producto.CodigoInterno ?? producto.CodigoBarras ?? producto.Id.ToString(),
                        Descripcion = $"{producto.Nombre} - {presentacion.Nombre}",
                        UnidadSunat = "NIU",
                        TipoAfectacionIgv = "10",
                        Cantidad = detalleRequest.Cantidad,
                        PrecioUnitarioConIgv = precioUnitarioConIgv,
                        DescuentoConIgv = detalleRequest.Descuento,
                        TotalConIgv = totalDetalleConIgv
                    });

                    await DescontarStockFefoAsync(
                        ventaDetalle,
                        detalleRequest.ProductoId,
                        request.SucursalId,
                        request.UsuarioId,
                        cantidadUnidades
                    );

                    totalVentaConIgv += totalDetalleConIgv;
                }

                if (descuentoVenta > totalVentaConIgv)
                    throw new Exception("El descuento global no puede ser mayor al total de productos.");

                var total = Math.Round(totalVentaConIgv - descuentoVenta, 2);

                if (total < 0)
                    throw new Exception("El total de venta no puede ser negativo.");

                var subtotalSinIgv = Math.Round(total / 1.18m, 2);
                var impuestoIncluido = Math.Round(total - subtotalSinIgv, 2);

                var totalPagado = Math.Round(request.Pagos.Sum(x => x.Monto), 2);

                if (Math.Abs(totalPagado - total) > 0.01m)
                    throw new Exception($"El total pagado ({totalPagado}) no coincide con el total de venta ({total}).");

                venta.Subtotal = subtotalSinIgv;
                venta.Impuesto = impuestoIncluido;
                venta.Descuento = descuentoVenta;
                venta.Total = total;

                /*
                 * CLAVE:
                 * Esto se ejecuta ANTES DEL COMMIT.
                 * Si el error es local, cae al catch, hace rollback
                 * y NO se registra venta, stock, caja ni documento.
                 */
                payloadEmision = await ConstruirPayloadFacturaBoletaLocalAsync(
                    venta,
                    detallesSunat
                );

                foreach (var pagoRequest in request.Pagos)
                {
                    var pago = new VentaPago
                    {
                        VentaId = venta.Id,
                        MedioPagoId = pagoRequest.MedioPagoId,
                        Monto = pagoRequest.Monto,
                        Referencia = pagoRequest.Referencia,
                        FechaCreacion = DateTime.UtcNow
                    };

                    await _ventaPagoRepository.CreateAsync(pago);
                    await _unitOfWork.SaveChangesAsync();

                    await CrearMovimientoCajaPorPagoAsync(
                        venta,
                        pago,
                        pagoRequest.MedioPagoId,
                        request.UsuarioId
                    );
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            /*
             * Desde aquí la venta YA existe.
             * Aquí solo se debe manejar error remoto:
             * APISPERU caído, SUNAT no responde, timeout, rechazo, etc.
             */
            await IntentarEmitirComprobanteElectronicoAsync(
                venta,
                payloadEmision!
            );

            return new RegistrarVentaResponse
            {
                VentaId = venta.Id,
                TipoDocumento = venta.TipoDocumento,
                Serie = venta.Serie,
                Correlativo = venta.Correlativo,
                Subtotal = venta.Subtotal,
                Impuesto = venta.Impuesto,
                Descuento = venta.Descuento ?? 0,
                Total = venta.Total,
                EstadoSunat = venta.EstadoSunat,
                SunatCodigo = venta.SunatCodigo,
                SunatMensaje = venta.SunatMensaje,
                Mensaje = venta.EstadoSunat == "ACEPTADO"
                    ? "Venta registrada y comprobante electrónico aceptado."
                    : venta.EstadoSunat == "ENVIADO"
                        ? "Venta registrada y comprobante electrónico enviado."
                        : "Venta registrada, pero la emisión electrónica quedó pendiente o con error."
            };
        }

    }
}
