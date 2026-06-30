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

namespace Bussines
{
    public class VentaBussines : IVentaBussines
    {
        #region DEPENDENCIAS
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
        private readonly IUnitOfWork _unitOfWork;
        #endregion

        #region CONSTRUCTOR
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
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region VENTA Y DESCUENTO DE STOCK
        public async Task<RegistrarVentaResponse> RegistrarVentaAsync(RegistrarVentaRequest request)
        {
            ValidarRequest(request);

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

                var venta = new Venta
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

                await IntentarEmitirComprobanteElectronicoAsync(venta, detallesSunat);

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
                    Mensaje = venta.EstadoSunat == "ACEPTADO"
                        ? "Venta registrada y comprobante electrónico aceptado."
                        : venta.EstadoSunat == "ENVIADO"
                            ? "Venta registrada y comprobante electrónico enviado."
                            : "Venta registrada, pero la emisión electrónica quedó pendiente o con error."
                };
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        private async Task DescontarStockFefoAsync(VentaDetalle ventaDetalle, int productoId, int sucursalId, int usuarioId, int cantidadUnidades)
        {
            var stock = await _productoStockRepository.GetByProductoSucursalAsync(productoId, sucursalId);

            if (stock == null)
                throw new Exception("No existe stock para este producto y sucursal.");

            var stockActual = Convert.ToInt32(stock.StockActual);

            if (stockActual < cantidadUnidades)
                throw new Exception($"Stock insuficiente. Stock actual: {stockActual}, requerido: {cantidadUnidades}.");

            var lotes = await _productoLoteRepository.GetLotesDisponiblesFefoAsync(productoId, sucursalId);

            var stockLotes = lotes.Sum(x => x.StockActual);

            if (stockLotes < cantidadUnidades)
                throw new Exception($"Stock insuficiente por lotes. Disponible en lotes: {stockLotes}, requerido: {cantidadUnidades}.");

            var restante = cantidadUnidades;
            var stockTemporal = stockActual;

            foreach (var lote in lotes)
            {
                if (restante <= 0)
                    break;

                var tomar = Math.Min(restante, lote.StockActual);

                lote.StockActual -= tomar;
                restante -= tomar;

                var detalleLote = new VentaDetalleLote
                {
                    VentaDetalleId = ventaDetalle.Id,
                    LoteId = lote.Id,
                    Cantidad = tomar,
                    FechaCreacion = DateTime.UtcNow
                };

                await _ventaDetalleLoteRepository.CreateAsync(detalleLote);

                var movimiento = new StockMovimiento
                {
                    ProductoId = productoId,
                    SucursalId = sucursalId,
                    Tipo = "VENTA",
                    Cantidad = tomar,
                    StockAnterior = stockTemporal,
                    StockNuevo = stockTemporal - tomar,
                    Motivo = $"Venta {ventaDetalle.VentaId}",
                    ReferenciaTabla = "venta_detalles",
                    ReferenciaId = ventaDetalle.Id,
                    UsuarioId = usuarioId,
                    FechaCreacion = DateTime.UtcNow,
                    LoteId = lote.Id
                };

                await _stockMovimientoRepository.CreateAsync(movimiento);

                stockTemporal -= tomar;
            }

            stock.StockActual = stockActual - cantidadUnidades;
        }

        private async Task CrearMovimientoCajaPorPagoAsync(Venta venta, VentaPago pago, int medioPagoId, int usuarioId)
        {
            if (venta.CajaSesionId == null)
                throw new Exception("La venta no tiene una sesión de caja asociada.");

            var medioPago = await _medioPagoRepository.GetByIdAsync(medioPagoId);

            if (medioPago == null)
                throw new Exception("El medio de pago no existe.");

            var codigo = medioPago.Codigo?.Trim().ToUpper();
            var tipo = medioPago.Tipo?.Trim().ToUpper();

            var esEfectivo = codigo == "EFECTIVO" || tipo == "EFECTIVO";

            if (!esEfectivo)
                return;

            var movimiento = new CajaMovimiento
            {
                CajaSesionId = venta.CajaSesionId.Value,
                CajaId = venta.CajaId,
                SucursalId = venta.SucursalId,
                UsuarioId = usuarioId,

                TipoMovimiento = "INGRESO",
                Concepto = "VENTA_EFECTIVO",

                MedioPagoId = medioPagoId,
                Monto = pago.Monto,

                AfectaEfectivo = true,

                Descripcion = $"Ingreso por venta {venta.Serie}-{venta.Correlativo}",
                ReferenciaTabla = "venta_pagos",
                ReferenciaId = pago.Id,

                Estado = "ACTIVO",
                FechaCreacion = DateTime.UtcNow
            };

            await _cajaMovimientoRepository.CreateAsync(movimiento);
        }

        private void ValidarRequest(RegistrarVentaRequest request)
        {
            if (request.SucursalId <= 0)
                throw new Exception("La sucursal es obligatoria.");

            if (request.CajaId <= 0)
                throw new Exception("La caja es obligatoria.");

            if (request.CajaSesionId == null || request.CajaSesionId <= 0)
                throw new Exception("La sesión de caja es obligatoria.");

            if (request.UsuarioId <= 0)
                throw new Exception("El usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.TipoDocumento))
                throw new Exception("El tipo de documento es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Serie))
                throw new Exception("La serie es obligatoria.");

            if (request.Detalles == null || !request.Detalles.Any())
                throw new Exception("La venta debe tener al menos un detalle.");

            foreach (var detalle in request.Detalles)
            {
                if (detalle.ProductoId <= 0)
                    throw new Exception("El producto es obligatorio.");

                if (detalle.PresentacionId <= 0)
                    throw new Exception("La presentación es obligatoria.");

                if (detalle.Cantidad <= 0)
                    throw new Exception("La cantidad debe ser mayor a 0.");

                if (detalle.Descuento < 0)
                    throw new Exception("El descuento no puede ser negativo.");
            }

            if (request.Pagos == null || !request.Pagos.Any())
                throw new Exception("Debe registrar al menos un pago.");
        }
        private async Task<int?> ResolverClienteVentaAsync(RegistrarVentaRequest request)
        {
            var clienteIdRequest = request.ClienteId.HasValue && request.ClienteId.Value > 0
                ? request.ClienteId.Value
                : request.Cliente?.Id;

            if (clienteIdRequest.HasValue && clienteIdRequest.Value > 0)
            {
                var existeCliente = await _clienteRepository.ExisteAsync(clienteIdRequest.Value);

                if (!existeCliente)
                    throw new Exception("El cliente seleccionado no existe en la base de datos.");

                return clienteIdRequest.Value;
            }

            var clienteRequest = request.Cliente;

            if (clienteRequest == null)
            {
                if (request.TipoDocumento == "01")
                    throw new Exception("Para emitir factura debes seleccionar un cliente con RUC.");

                return null;
            }

            var tipoDocumento = NormalizarTipoDocumento(clienteRequest.TipoDocumento);
            var numeroDocumento = clienteRequest.NumeroDocumento?.Trim();

            if (string.IsNullOrWhiteSpace(numeroDocumento))
            {
                if (request.TipoDocumento == "01")
                    throw new Exception("Para emitir factura debes ingresar el RUC del cliente.");

                return null;
            }

            if (request.TipoDocumento == "01" && tipoDocumento != "RUC")
                throw new Exception("Para emitir factura el cliente debe tener RUC.");

            if (request.TipoDocumento == "01" && numeroDocumento.Length != 11)
                throw new Exception("El RUC debe tener 11 dígitos.");

            var clienteExistente = await _clienteRepository.ObtenerPorDocumentoAsync(
                tipoDocumento,
                numeroDocumento
            );

            if (clienteExistente != null)
                return clienteExistente.Id;

            var nuevoCliente = new Cliente
            {
                TipoDocumento = tipoDocumento,
                NumeroDocumento = numeroDocumento,

                Nombres = clienteRequest.Nombres,
                Apellidos = clienteRequest.Apellidos,
                RazonSocial = clienteRequest.RazonSocial,

                Direccion = clienteRequest.Direccion,
                Telefono = clienteRequest.Telefono,
                Email = clienteRequest.Email,

                Estado = "ACTIVO",
                FechaCreacion = DateTime.UtcNow,
            };

            if (string.IsNullOrWhiteSpace(nuevoCliente.Nombres) &&
                string.IsNullOrWhiteSpace(nuevoCliente.RazonSocial) &&
                !string.IsNullOrWhiteSpace(clienteRequest.NombreCompleto))
            {
                nuevoCliente.Nombres = clienteRequest.NombreCompleto.Trim();
            }

            await _clienteRepository.CreateAsync(nuevoCliente);
            await _unitOfWork.SaveChangesAsync();

            return nuevoCliente.Id;
        }

        private static string NormalizarTipoDocumento(string? tipoDocumento)
        {
            var tipo = tipoDocumento?.Trim().ToUpper() ?? "";

            return tipo switch
            {
                "DNI" => "DNI",
                "1" => "DNI",
                "RUC" => "RUC",
                "6" => "RUC",
                _ => tipo
            };
        }
        #endregion 

        #region Emision-Electronica

        private async Task IntentarEmitirComprobanteElectronicoAsync(
            Venta venta,
            List<DetalleSunatTemporal> detallesSunat)
        {
            DocumentosElectronico? documento = null;

            try
            {
                documento = await ObtenerOCrearDocumentoElectronicoAsync(venta);

                var empresa = await _empresaRepository.ObtenerEmpresaPorSucursalAsync(venta.SucursalId);

                if (empresa == null)
                    throw new Exception("No se encontró la empresa emisora para la sucursal de la venta.");

                var payload = _apisPeruPayloadBuilder.ConstruirPayloadApisPeru(venta, detallesSunat, empresa);

                documento.FechaEnvio = DateTime.UtcNow;
                documento.Estado = "PENDIENTE";

                await _unitOfWork.SaveChangesAsync();

                var respuesta = await _apisPeruFacturacionService
                    .EnviarFacturaBoletaAsync(payload);

                var codigoCdr = respuesta.CdrResponse?.Code;
                var codigoAceptado = codigoCdr == "0";

                var aceptado = respuesta.Success &&
                               ((respuesta.CdrResponse?.Accepted ?? false) || codigoAceptado);

                var estadoSunat = aceptado
                    ? "ACEPTADO"
                    : respuesta.Success && respuesta.CdrResponse != null
                        ? "RECHAZADO"
                        : respuesta.Success
                            ? "ENVIADO"
                            : "ERROR_ENVIO";

                var codigoRespuesta = respuesta.CdrResponse?.Code
                    ?? respuesta.Error?.Code;

                var descripcionRespuesta = respuesta.CdrResponse?.Description
                    ?? respuesta.Error?.Message
                    ?? "Respuesta recibida de APISPERU.";

                // Resumen en ventas
                venta.EstadoSunat = estadoSunat;
                venta.SunatCodigo = codigoRespuesta;
                venta.SunatMensaje = descripcionRespuesta;
                venta.SunatHash = respuesta.Hash;
                venta.RespuestaSunatJson = respuesta.RawJson ?? "{}";
                venta.FechaEnvioSunat = DateTime.UtcNow;

                // Auditoría en documentos_electronicos
                documento.FechaRespuesta = DateTime.UtcNow;
                documento.CodigoRespuesta = Limitar(codigoRespuesta, 10);
                documento.DescripcionRespuesta = descripcionRespuesta;
                documento.CdrNumero = Limitar(respuesta.CdrResponse?.Id, 20);
                documento.CdrDescripcion = respuesta.CdrResponse?.Description;
                documento.XmlEnviado = respuesta.Xml;
                documento.XmlRespuesta = respuesta.SunatResponse?.CdrZip ?? respuesta.RawJson;
                documento.CodigoHash = respuesta.Hash;
                documento.SunatXmlId = Limitar(
                    $"{empresa.Ruc}-{venta.TipoDocumento}-{venta.Serie}-{venta.Correlativo}",
                    100
                );
                documento.Estado = estadoSunat;

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                documento ??= await ObtenerOCrearDocumentoElectronicoAsync(venta);

                venta.EstadoSunat = "ERROR_ENVIO";
                venta.SunatCodigo = "LOCAL_ERROR";
                venta.SunatMensaje = ex.Message;
                venta.RespuestaSunatJson = JsonSerializer.Serialize(new
                {
                    error = ex.Message,
                    fecha = DateTime.UtcNow
                });
                venta.FechaEnvioSunat = DateTime.UtcNow;

                documento.FechaRespuesta = DateTime.UtcNow;
                documento.CodigoRespuesta = "LOCAL_ERR";
                documento.DescripcionRespuesta = ex.Message;
                documento.CdrDescripcion = ex.Message;
                documento.XmlRespuesta = venta.RespuestaSunatJson?.ToString();
                documento.Estado = "ERROR_ENVIO";

                await _unitOfWork.SaveChangesAsync();
            }
        }
        private async Task<DocumentosElectronico> ObtenerOCrearDocumentoElectronicoAsync(Venta venta)
        {
            var documento = await _documentoElectronicoRepository
                .ObtenerPorVentaIdAsync(venta.Id);

            if (documento != null)
                return documento;

            documento = await _documentoElectronicoRepository.ObtenerPorComprobanteAsync(
                venta.TipoDocumento,
                venta.Serie,
                venta.Correlativo
            );

            if (documento != null)
                return documento;

            documento = new DocumentosElectronico
            {
                VentaId = venta.Id,
                TipoDocumento = venta.TipoDocumento,
                Serie = venta.Serie,
                Correlativo = venta.Correlativo,
                FechaEnvio = DateTime.UtcNow,
                Estado = "PENDIENTE",
                FechaCreacion = DateTime.UtcNow
            };

            await _documentoElectronicoRepository.CreateAsync(documento);
            await _unitOfWork.SaveChangesAsync();

            return documento;
        }

        private static string? Limitar(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            return value.Length <= maxLength
                ? value
                : value.Substring(0, maxLength);
        }


        public async Task<RegistrarVentaResponse> ReintentarEmisionAsync(int ventaId)
        {

            if (ventaId <= 0)
                throw new Exception("El id de la venta es obligatorio.");

            var venta = await _ventaRepository.GetByIdAsync(ventaId);

            if (venta == null)
                throw new Exception($"No se encontró la venta con id {ventaId}.");

            if (venta.EstadoSunat == "ACEPTADO")
                throw new Exception("La venta ya fue aceptada por SUNAT.");

            var detallesSunat = await ConstruirDetallesSunatDesdeVentaAsync(ventaId);
            var estadosPermitidos = new[]
{
                "PENDIENTE_ENVIO",
                "ERROR_ENVIO",
                "ERROR_CONSULTA"
            };
            if (!estadosPermitidos.Contains(venta.EstadoSunat))
                throw new Exception("Esta venta no permite reintento de emisión.");
            await IntentarEmitirComprobanteElectronicoAsync(
                venta,
                detallesSunat
            );

            return MapToRegistrarVentaResponse(
                venta,
                venta.EstadoSunat == "ACEPTADO"
                    ? "Comprobante electrónico aceptado."
                    : "Se reintentó la emisión electrónica."
            );
        }
        #endregion

        #region GENERARPDF
        public async Task<ArchivoVentaResponse> DescargarPdfAsync(int ventaId)
        {
            if (ventaId <= 0)
                throw new Exception("El id de la venta es obligatorio.");

            var venta = await _ventaRepository.GetByIdAsync(ventaId);

            if (venta == null)
                throw new Exception($"No se encontró la venta con id {ventaId}.");

            if (venta.EstadoSunat != "ACEPTADO" && venta.EstadoSunat != "ENVIADO")
                throw new Exception("La venta aún no fue enviada electrónicamente.");

            var detalles = await _ventaDetalleRepository.ObtenerPorVentaIdAsync(ventaId);

            if (detalles == null || !detalles.Any())
                throw new Exception("La venta no tiene detalles.");

            var empresa = await _empresaRepository.ObtenerEmpresaPorSucursalAsync(venta.SucursalId);

            if (empresa == null)
                throw new Exception("No se encontró la empresa emisora para la sucursal de la venta.");

            var detallesSunat = detalles.Select(detalle => new DetalleSunatTemporal
            {
                ProductoId = detalle.ProductoId,
                PresentacionId = detalle.PresentacionId ?? 0,
                CodProducto = detalle.ProductoCodigoInterno
                    ?? detalle.CodigoBarras
                    ?? detalle.ProductoId.ToString(),

                Descripcion = string.IsNullOrWhiteSpace(detalle.PresentacionNombre)
                    ? detalle.NombreProducto
                    : $"{detalle.NombreProducto} - {detalle.PresentacionNombre}",

                UnidadSunat = string.IsNullOrWhiteSpace(detalle.UnidadSunat)
                    ? "NIU"
                    : detalle.UnidadSunat,

                TipoAfectacionIgv = string.IsNullOrWhiteSpace(detalle.TipoAfectacionIgv)
                    ? "10"
                    : detalle.TipoAfectacionIgv,

                Cantidad = detalle.Cantidad,
                PrecioUnitarioConIgv = detalle.PrecioUnitario,
                TotalConIgv = detalle.Subtotal
            }).ToList();

            var payload = _apisPeruPayloadBuilder.ConstruirFacturaBoletaPayload(
                venta,
                detallesSunat,
                empresa
            );

            var archivo = await _apisPeruFacturacionService
                .GenerarPdfFacturaBoletaAsync(payload);

            if (!archivo.Success || archivo.Bytes == null || archivo.Bytes.Length == 0)
            {
                throw new Exception(
                    $"No se pudo generar el PDF del comprobante. Error APISPERU: {archivo.RawError}"
                );
            }

            var nombreArchivo = $"{empresa.Ruc}-{venta.TipoDocumento}-{venta.Serie}-{venta.Correlativo}.pdf";

            return new ArchivoVentaResponse
            {
                Bytes = archivo.Bytes,
                ContentType = "application/pdf",
                NombreArchivo = nombreArchivo
            };
        }
        #endregion

        #region Construccion Detalles SUNAT
        private async Task<List<DetalleSunatTemporal>> ConstruirDetallesSunatDesdeVentaAsync(int ventaId)
        {
            var detalles = await _ventaDetalleRepository.ObtenerPorVentaIdAsync(ventaId);

            if (detalles == null || !detalles.Any())
                throw new Exception("La venta no tiene detalles.");

            return detalles.Select(detalle => new DetalleSunatTemporal
            {
                ProductoId = detalle.ProductoId,
                PresentacionId = detalle.PresentacionId ?? 0,
                CodProducto = detalle.ProductoCodigoInterno
                    ?? detalle.CodigoBarras
                    ?? detalle.ProductoId.ToString(),

                Descripcion = string.IsNullOrWhiteSpace(detalle.PresentacionNombre)
                    ? detalle.NombreProducto
                    : $"{detalle.NombreProducto} - {detalle.PresentacionNombre}",

                UnidadSunat = string.IsNullOrWhiteSpace(detalle.UnidadSunat)
                    ? "NIU"
                    : detalle.UnidadSunat,

                TipoAfectacionIgv = string.IsNullOrWhiteSpace(detalle.TipoAfectacionIgv)
                    ? "10"
                    : detalle.TipoAfectacionIgv,

                Cantidad = detalle.Cantidad,
                PrecioUnitarioConIgv = detalle.PrecioUnitario,
                TotalConIgv = detalle.Subtotal
            }).ToList();
        }
        #endregion

        #region DESCARGA XML
        public async Task<ArchivoVentaResponse> DescargarXmlAsync(int ventaId)
        {
            if (ventaId <= 0)
                throw new Exception("El id de la venta es obligatorio.");

            var venta = await _ventaRepository.GetByIdAsync(ventaId);

            if (venta == null)
                throw new Exception($"No se encontró la venta con id {ventaId}.");

            var empresa = await _empresaRepository.ObtenerEmpresaPorSucursalAsync(venta.SucursalId);

            if (empresa == null)
                throw new Exception("No se encontró la empresa emisora para la sucursal de la venta.");

            var documento = await _documentoElectronicoRepository.ObtenerPorVentaIdAsync(ventaId);

            if (!string.IsNullOrWhiteSpace(documento?.XmlEnviado))
            {
                var bytesGuardados = Encoding.UTF8.GetBytes(documento.XmlEnviado);

                return new ArchivoVentaResponse
                {
                    Bytes = bytesGuardados,
                    ContentType = "application/xml",
                    NombreArchivo = $"{empresa.Ruc}-{venta.TipoDocumento}-{venta.Serie}-{venta.Correlativo}.xml"
                };
            }

            var detallesSunat = await ConstruirDetallesSunatDesdeVentaAsync(ventaId);

            var payload = _apisPeruPayloadBuilder.ConstruirFacturaBoletaPayload(
                venta,
                detallesSunat,
                empresa
            );

            var archivo = await _apisPeruFacturacionService
                .GenerarXmlFacturaBoletaAsync(payload);

            if (!archivo.Success || archivo.Bytes == null || archivo.Bytes.Length == 0)
            {
                throw new Exception(
                    $"No se pudo generar el XML del comprobante. Error APISPERU: {archivo.RawError}"
                );
            }

            return new ArchivoVentaResponse
            {
                Bytes = archivo.Bytes,
                ContentType = "application/xml",
                NombreArchivo = $"{empresa.Ruc}-{venta.TipoDocumento}-{venta.Serie}-{venta.Correlativo}.xml"
            };
        }
        #endregion

        #region CONSULTA ESTADO Y SINCRONIZAR ESTADO SUNAT
        public async Task<EstadoSunatConsultaResponse> ConsultarEstadoSunatAsync(int ventaId)
        {
            if (ventaId <= 0)
                throw new Exception("El id de la venta es obligatorio.");

            var venta = await _ventaRepository.GetByIdAsync(ventaId);

            if (venta == null)
                throw new Exception($"No se encontró la venta con id {ventaId}.");

            var empresa = await _empresaRepository.ObtenerEmpresaPorSucursalAsync(venta.SucursalId);

            if (empresa == null)
                throw new Exception("No se encontró la empresa emisora para la sucursal de la venta.");

            var respuesta = await _apisPeruFacturacionService.ConsultarEstadoFacturaBoletaAsync(
                venta.TipoDocumento,
                venta.Serie,
                venta.Correlativo,
                empresa.Ruc
            );

            var codigoRespuesta = respuesta.CdrResponse?.Code
                ?? respuesta.Error?.Code
                ?? respuesta.Code;

            var descripcionRespuesta = respuesta.CdrResponse?.Description
                ?? respuesta.Error?.Message
                ?? "Respuesta recibida de APISPERU.";

            var aceptado = respuesta.Success &&
                           ((respuesta.CdrResponse?.Accepted ?? false) || respuesta.CdrResponse?.Code == "0");

            var estadoConsulta = aceptado
                ? "ACEPTADO"
                : respuesta.Success && respuesta.CdrResponse != null
                    ? "RECHAZADO"
                    : "ERROR_CONSULTA";

            return new EstadoSunatConsultaResponse
            {
                VentaId = venta.Id,
                TipoDocumento = venta.TipoDocumento,
                Serie = venta.Serie,
                Correlativo = venta.Correlativo,

                EstadoLocal = venta.EstadoSunat,
                CodigoLocal = venta.SunatCodigo,
                MensajeLocal = venta.SunatMensaje,

                ConsultaExitosa = respuesta.Success,
                CodigoConsulta = codigoRespuesta,
                MensajeConsulta = descripcionRespuesta,
                EstadoConsulta = estadoConsulta,
                RawJson = respuesta.RawJson
            };
        }
        public async Task<RegistrarVentaResponse> SincronizarEstadoSunatAsync(int ventaId)
        {
            if (ventaId <= 0)
                throw new Exception("El id de la venta es obligatorio.");

            var venta = await _ventaRepository.GetByIdAsync(ventaId);

            if (venta == null)
                throw new Exception($"No se encontró la venta con id {ventaId}.");

            var empresa = await _empresaRepository.ObtenerEmpresaPorSucursalAsync(venta.SucursalId);

            if (empresa == null)
                throw new Exception("No se encontró la empresa emisora para la sucursal de la venta.");

            var respuesta = await _apisPeruFacturacionService.ConsultarEstadoFacturaBoletaAsync(
                venta.TipoDocumento,
                venta.Serie,
                venta.Correlativo,
                empresa.Ruc
            );

            var documento = await ObtenerOCrearDocumentoElectronicoAsync(venta);

            var codigoRespuesta = respuesta.CdrResponse?.Code
                ?? respuesta.Error?.Code
                ?? respuesta.Code;

            var descripcionRespuesta = respuesta.CdrResponse?.Description
                ?? respuesta.Error?.Message
                ?? "Respuesta recibida de APISPERU.";

            var aceptado = respuesta.Success &&
                           ((respuesta.CdrResponse?.Accepted ?? false) || respuesta.CdrResponse?.Code == "0");

            if (aceptado)
            {
                venta.EstadoSunat = "ACEPTADO";
                venta.SunatCodigo = codigoRespuesta;
                venta.SunatMensaje = descripcionRespuesta;
                venta.RespuestaSunatJson = respuesta.RawJson ?? "{}";
                venta.FechaEnvioSunat = DateTime.UtcNow;

                documento.FechaRespuesta = DateTime.UtcNow;
                documento.CodigoRespuesta = Limitar(codigoRespuesta, 10);
                documento.DescripcionRespuesta = descripcionRespuesta;
                documento.CdrNumero = Limitar(respuesta.CdrResponse?.Id, 20);
                documento.CdrDescripcion = respuesta.CdrResponse?.Description;
                documento.XmlRespuesta = respuesta.CdrZip ?? respuesta.RawJson;
                documento.Estado = "ACEPTADO";

                await _unitOfWork.SaveChangesAsync();

                return MapToRegistrarVentaResponse(
                    venta,
                    "Comprobante aceptado por SUNAT."
                );
            }

            // Si SUNAT/APISPERU responde error, solo actualizamos si aún NO estaba aceptado.
            if (venta.EstadoSunat != "ACEPTADO" && venta.SunatCodigo != "0")
            {
                venta.EstadoSunat = "ERROR_CONSULTA";
                venta.SunatCodigo = codigoRespuesta;
                venta.SunatMensaje = descripcionRespuesta;
                venta.RespuestaSunatJson = respuesta.RawJson ?? "{}";
                venta.FechaEnvioSunat = DateTime.UtcNow;

                documento.FechaRespuesta = DateTime.UtcNow;
                documento.CodigoRespuesta = Limitar(codigoRespuesta, 10);
                documento.DescripcionRespuesta = descripcionRespuesta;
                documento.XmlRespuesta = respuesta.RawJson;
                documento.Estado = "ERROR_CONSULTA";

                await _unitOfWork.SaveChangesAsync();
            }

            return MapToRegistrarVentaResponse(
                venta,
                $"Consulta realizada. APISPERU respondió: {codigoRespuesta} - {descripcionRespuesta}"
            );
        }

        private static RegistrarVentaResponse MapToRegistrarVentaResponse(Venta venta, string mensaje)
        {
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
                Mensaje = mensaje
            };
        }
        #endregion

        #region EMITIR NOTAS DE CREDITO
        public async Task<EmitirNotaCreditoResponse> EmitirNotaCreditoAsync(EmitirNotaCreditoRequest request)
        {
            if (request.VentaId <= 0)
                throw new Exception("El id de venta es obligatorio.");

            var venta = await _ventaRepository.GetByIdAsync(request.VentaId);

            if (venta == null)
                throw new Exception($"No se encontró la venta con id {request.VentaId}.");

            if (venta.EstadoSunat != "ACEPTADO" && venta.SunatCodigo != "0")
                throw new Exception("Solo se puede emitir nota de crédito de una venta aceptada por SUNAT.");

            var notaExistente = await _notaCreditoRepository.ObtenerPorVentaIdAsync(venta.Id);

            if (notaExistente != null)
                throw new Exception($"La venta ya tiene nota de crédito: {notaExistente.Serie}-{notaExistente.Correlativo}");

            var empresa = await _empresaRepository.ObtenerEmpresaPorSucursalAsync(venta.SucursalId);

            if (empresa == null)
                throw new Exception("No se encontró la empresa emisora para la sucursal de la venta.");

            var detallesSunat = await ConstruirDetallesSunatDesdeVentaAsync(venta.Id);

            var serieNota = venta.TipoDocumento == "03" ? "BC01" : "FC01";

            var serie = await _comprobanteSerieRepository.GetSerieActivaAsync(
                venta.SucursalId,
                "07",
                serieNota
            );

            if (serie == null)
                throw new Exception($"No existe serie activa para nota de crédito {serieNota}.");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                serie.CorrelativoActual += 1;
                var correlativoNota = serie.CorrelativoActual;

                var notaCredito = new NotasCredito
                {
                    VentaId = venta.Id,

                    TipoDocumento = "07",
                    Serie = serieNota,
                    Correlativo = correlativoNota,

                    TipoDocumentoAfectado = venta.TipoDocumento,
                    SerieAfectada = venta.Serie,
                    CorrelativoAfectado = venta.Correlativo,

                    CodigoMotivo = request.CodigoMotivo,
                    DescripcionMotivo = request.DescripcionMotivo,

                    Subtotal = venta.Subtotal,
                    Impuesto = venta.Impuesto,
                    Total = venta.Total,

                    EstadoSunat = "PENDIENTE_ENVIO",
                    FechaEmision = DateTime.UtcNow,
                    Estado = "ACTIVO"
                };
                var documentoNota = await ObtenerOCrearDocumentoElectronicoNotaCreditoAsync(notaCredito);
                await _notaCreditoRepository.CreateAsync(notaCredito);
                await _unitOfWork.SaveChangesAsync();

                var payload = _apisPeruPayloadBuilder.ConstruirNotasCreditoPayload(
                    notaCredito,
                    venta,
                    detallesSunat,
                    empresa
                );

                var respuesta = await _apisPeruFacturacionService.EnviarNotaCreditoAsync(payload);

                var codigoCdr = respuesta.CdrResponse?.Code;
                var aceptado = respuesta.Success &&
                               ((respuesta.CdrResponse?.Accepted ?? false) || codigoCdr == "0");

                var estadoSunat = aceptado
                    ? "ACEPTADO"
                    : respuesta.Success && respuesta.CdrResponse != null
                        ? "RECHAZADO"
                        : "ERROR_ENVIO";

                var codigoRespuesta = respuesta.CdrResponse?.Code
                    ?? respuesta.Error?.Code;

                var descripcionRespuesta = respuesta.CdrResponse?.Description
                    ?? respuesta.Error?.Message
                    ?? "Respuesta recibida de APISPERU.";

                notaCredito.EstadoSunat = estadoSunat;
                notaCredito.SunatCodigo = codigoRespuesta;
                notaCredito.SunatMensaje = descripcionRespuesta;
                notaCredito.SunatHash = respuesta.Hash;
                notaCredito.RespuestaSunatJson = respuesta.RawJson ?? "{}";
                notaCredito.FechaEnvioSunat = DateTime.UtcNow;

                // Documento electrónico de la nota
                documentoNota.FechaRespuesta = DateTime.UtcNow;
                documentoNota.CodigoRespuesta = Limitar(codigoRespuesta, 10);
                documentoNota.DescripcionRespuesta = descripcionRespuesta;
                documentoNota.CdrNumero = Limitar(respuesta.CdrResponse?.Id, 20);
                documentoNota.CdrDescripcion = respuesta.CdrResponse?.Description;
                documentoNota.XmlEnviado = respuesta.Xml;
                documentoNota.XmlRespuesta = respuesta.SunatResponse?.CdrZip ?? respuesta.RawJson;
                documentoNota.CodigoHash = respuesta.Hash;
                documentoNota.SunatXmlId = Limitar(
                    $"{notaCredito.TipoDocumento}-{notaCredito.Serie}-{notaCredito.Correlativo}",
                    100
                );
                documentoNota.Estado = estadoSunat;

                if (aceptado)
                {
                    venta.EstadoSunat = "ANULADO";
                    venta.SunatMensaje = $"Venta anulada con nota de crédito {notaCredito.Serie}-{notaCredito.Correlativo}";
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return new EmitirNotaCreditoResponse
                {
                    NotaCreditoId = notaCredito.Id,
                    VentaId = venta.Id,
                    TipoDocumento = notaCredito.TipoDocumento,
                    Serie = notaCredito.Serie,
                    Correlativo = notaCredito.Correlativo,
                    Comprobante = $"{notaCredito.Serie}-{notaCredito.Correlativo}",
                    EstadoSunat = notaCredito.EstadoSunat,
                    SunatCodigo = notaCredito.SunatCodigo,
                    SunatMensaje = notaCredito.SunatMensaje,
                    Mensaje = aceptado
                        ? "Nota de crédito aceptada. Venta anulada."
                        : "Nota de crédito registrada, pero no fue aceptada."
                };
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        private async Task<DocumentosElectronico> ObtenerOCrearDocumentoElectronicoNotaCreditoAsync(NotasCredito notaCredito)
        {
            var documento = await _documentoElectronicoRepository.ObtenerPorComprobanteAsync(
                notaCredito.TipoDocumento,
                notaCredito.Serie,
                notaCredito.Correlativo
            );

            if (documento != null)
                return documento;

            documento = new DocumentosElectronico
            {
                VentaId = notaCredito.VentaId,

                TipoDocumento = notaCredito.TipoDocumento, // 07
                Serie = notaCredito.Serie,                 // BC01
                Correlativo = notaCredito.Correlativo,

                FechaEnvio = DateTime.UtcNow,
                Estado = "PENDIENTE",
                FechaCreacion = DateTime.UtcNow,

                SunatXmlId = $"{notaCredito.TipoDocumento}-{notaCredito.Serie}-{notaCredito.Correlativo}"
            };

            await _documentoElectronicoRepository.CreateAsync(documento);
            await _unitOfWork.SaveChangesAsync();

            return documento;
        }
        #endregion

        #region ANULAR VENTA
        public async Task<AnularVentaResponse> AnularVentaAsync(AnularVentaRequest request)
        {
            if (request.VentaId <= 0)
                throw new Exception("El id de venta es obligatorio.");

            if (request.UsuarioId <= 0)
                throw new Exception("El usuario es obligatorio.");

            var venta = await _ventaRepository.GetByIdAsync(request.VentaId);

            if (venta == null)
                throw new Exception($"No se encontró la venta con id {request.VentaId}.");

            if (venta.EstadoSunat == "ANULADO")
                throw new Exception("La venta ya está anulada.");

            if (venta.EstadoSunat != "ACEPTADO" && venta.SunatCodigo != "0")
                throw new Exception("Solo se puede anular una venta aceptada por SUNAT.");

            var notaExistente = await _notaCreditoRepository.ObtenerPorVentaIdAsync(venta.Id);

            if (notaExistente != null && notaExistente.EstadoSunat == "ACEPTADO")
                throw new Exception($"La venta ya tiene nota de crédito aceptada: {notaExistente.Serie}-{notaExistente.Correlativo}");

            var empresa = await _empresaRepository.ObtenerEmpresaPorSucursalAsync(venta.SucursalId);

            if (empresa == null)
                throw new Exception("No se encontró la empresa emisora para la sucursal de la venta.");

            var detallesSunat = await ConstruirDetallesSunatDesdeVentaAsync(venta.Id);

            var serieNota = venta.TipoDocumento == "03" ? "BC01" : "FC01";

            var serie = await _comprobanteSerieRepository.GetSerieActivaAsync(
                venta.SucursalId,
                "07",
                serieNota
            );

            if (serie == null)
                throw new Exception($"No existe serie activa para nota de crédito {serieNota}.");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                serie.CorrelativoActual += 1;
                var correlativoNota = serie.CorrelativoActual;

                var notaCredito = new NotasCredito
                {
                    VentaId = venta.Id,

                    TipoDocumento = "07",
                    Serie = serieNota,
                    Correlativo = correlativoNota,

                    TipoDocumentoAfectado = venta.TipoDocumento,
                    SerieAfectada = venta.Serie,
                    CorrelativoAfectado = venta.Correlativo,

                    CodigoMotivo = request.CodigoMotivo,
                    DescripcionMotivo = request.DescripcionMotivo,

                    Subtotal = venta.Subtotal,
                    Impuesto = venta.Impuesto,
                    Total = venta.Total,

                    EstadoSunat = "PENDIENTE_ENVIO",
                    FechaEmision = DateTime.UtcNow,
                    Estado = "ACTIVO"
                };

                await _notaCreditoRepository.CreateAsync(notaCredito);
                await _unitOfWork.SaveChangesAsync();

                var documentoNota = await ObtenerOCrearDocumentoElectronicoNotaCreditoAsync(notaCredito);

                var payload = _apisPeruPayloadBuilder.ConstruirNotasCreditoPayload(
                    notaCredito,
                    venta,
                    detallesSunat,
                    empresa
                );

                var respuesta = await _apisPeruFacturacionService.EnviarNotaCreditoAsync(payload);

                var codigoCdr = respuesta.CdrResponse?.Code;
                var aceptado = respuesta.Success &&
                               ((respuesta.CdrResponse?.Accepted ?? false) || codigoCdr == "0");

                var estadoSunat = aceptado
                    ? "ACEPTADO"
                    : respuesta.Success && respuesta.CdrResponse != null
                        ? "RECHAZADO"
                        : "ERROR_ENVIO";

                var codigoRespuesta = respuesta.CdrResponse?.Code
                    ?? respuesta.Error?.Code;

                var descripcionRespuesta = respuesta.CdrResponse?.Description
                    ?? respuesta.Error?.Message
                    ?? "Respuesta recibida de APISPERU.";

                notaCredito.EstadoSunat = estadoSunat;
                notaCredito.SunatCodigo = codigoRespuesta;
                notaCredito.SunatMensaje = descripcionRespuesta;
                notaCredito.SunatHash = respuesta.Hash;
                notaCredito.RespuestaSunatJson = respuesta.RawJson ?? "{}";
                notaCredito.FechaEnvioSunat = DateTime.UtcNow;

                documentoNota.FechaRespuesta = DateTime.UtcNow;
                documentoNota.CodigoRespuesta = Limitar(codigoRespuesta, 10);
                documentoNota.DescripcionRespuesta = descripcionRespuesta;
                documentoNota.CdrNumero = Limitar(respuesta.CdrResponse?.Id, 20);
                documentoNota.CdrDescripcion = respuesta.CdrResponse?.Description;
                documentoNota.XmlEnviado = respuesta.Xml;
                documentoNota.XmlRespuesta = respuesta.SunatResponse?.CdrZip ?? respuesta.RawJson;
                documentoNota.CodigoHash = respuesta.Hash;
                documentoNota.SunatXmlId = Limitar(
                    $"{empresa.Ruc}-{notaCredito.TipoDocumento}-{notaCredito.Serie}-{notaCredito.Correlativo}",
                    100
                );
                documentoNota.Estado = estadoSunat;

                if (!aceptado)
                {
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();

                    return new AnularVentaResponse
                    {
                        VentaId = venta.Id,
                        NotaCreditoId = notaCredito.Id,
                        VentaOriginal = $"{venta.Serie}-{venta.Correlativo}",
                        NotaCredito = $"{notaCredito.Serie}-{notaCredito.Correlativo}",
                        EstadoVenta = venta.EstadoSunat,
                        EstadoNotaCredito = notaCredito.EstadoSunat,
                        TotalDevuelto = 0,
                        SunatCodigo = notaCredito.SunatCodigo,
                        SunatMensaje = notaCredito.SunatMensaje,
                        Mensaje = "La nota de crédito no fue aceptada. No se devolvió stock ni caja."
                    };
                }

                await DevolverStockVentaAsync(
                    venta,
                    request.UsuarioId,
                    notaCredito.Id
                );

                if (request.DevolverDineroCaja)
                {
                    await RegistrarDevolucionCajaAsync(
                        venta,
                        request.UsuarioId,
                        notaCredito.Id
                    );
                }

                venta.EstadoSunat = "ANULADO";
                venta.SunatMensaje = $"Venta anulada con nota de crédito {notaCredito.Serie}-{notaCredito.Correlativo}";

                if (!string.IsNullOrWhiteSpace(request.Observacion))
                {
                    venta.Observaciones = $"{venta.Observaciones} | ANULACIÓN: {request.Observacion}";
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return new AnularVentaResponse
                {
                    VentaId = venta.Id,
                    NotaCreditoId = notaCredito.Id,
                    VentaOriginal = $"{venta.Serie}-{venta.Correlativo}",
                    NotaCredito = $"{notaCredito.Serie}-{notaCredito.Correlativo}",
                    EstadoVenta = venta.EstadoSunat,
                    EstadoNotaCredito = notaCredito.EstadoSunat,
                    TotalDevuelto = request.DevolverDineroCaja ? venta.Total : 0,
                    SunatCodigo = notaCredito.SunatCodigo,
                    SunatMensaje = notaCredito.SunatMensaje,
                    Mensaje = "Venta anulada correctamente. Stock y caja actualizados."
                };
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        private async Task DevolverStockVentaAsync(Venta venta, int usuarioId, int notaCreditoId)
        {
            var detalles = await _ventaDetalleRepository.ObtenerPorVentaIdAsync(venta.Id);

            if (detalles == null || !detalles.Any())
                throw new Exception("La venta no tiene detalles para devolver stock.");

            foreach (var detalle in detalles)
            {
                var lotesVendidos = await _ventaDetalleLoteRepository
                    .ObtenerPorVentaDetalleIdAsync(detalle.Id);

                if (lotesVendidos == null || !lotesVendidos.Any())
                    throw new Exception($"No se encontró detalle de lotes para el producto {detalle.ProductoId}.");

                var totalUnidadesDevueltas = lotesVendidos.Sum(x => x.Cantidad);

                var stock = await _productoStockRepository.GetByProductoSucursalAsync(
                    detalle.ProductoId,
                    venta.SucursalId
                );

                if (stock == null)
                    throw new Exception($"No existe stock del producto {detalle.ProductoId} en la sucursal {venta.SucursalId}.");

                var stockAnterior = stock.StockActual;
                stock.StockActual += totalUnidadesDevueltas;

                foreach (var loteVendido in lotesVendidos)
                {
                    var lote = await _productoLoteRepository.GetByIdAsync(loteVendido.LoteId);

                    if (lote == null)
                        throw new Exception($"No se encontró el lote {loteVendido.LoteId}.");

                    lote.StockActual += loteVendido.Cantidad;

                    var movimiento = new StockMovimiento
                    {
                        ProductoId = detalle.ProductoId,
                        SucursalId = venta.SucursalId,
                        LoteId = lote.Id,

                        Tipo = "DEVOLUCION",
                        Cantidad = loteVendido.Cantidad,

                        StockAnterior = stockAnterior,
                        StockNuevo = stock.StockActual,

                        Motivo = $"Anulación de venta {venta.Serie}-{venta.Correlativo}",
                        ReferenciaTabla = "notas_credito",
                        ReferenciaId = notaCreditoId,

                        UsuarioId = usuarioId,
                        FechaCreacion = DateTime.UtcNow
                    };

                    await _stockMovimientoRepository.CreateAsync(movimiento);
                }
            }
        }

        private async Task RegistrarDevolucionCajaAsync(Venta venta, int usuarioId, int notaCreditoId)
        {
            if (venta.CajaId == null || venta.CajaSesionId == null)
                return;

            var pagos = await _ventaPagoRepository.ObtenerPorVentaIdAsync(venta.Id);

            if (pagos == null || !pagos.Any())
                return;

            // Por ahora asumimos que medio_pago_id = 1 es EFECTIVO.
            // Si tu tabla tiene otro id para efectivo, cambia esto.
            var montoEfectivo = pagos
                .Where(x => x.MedioPagoId == 1)
                .Sum(x => x.Monto);

            if (montoEfectivo <= 0)
                return;

            var movimientoCaja = new CajaMovimiento
            {
                CajaSesionId = venta.CajaSesionId.Value,
                CajaId = venta.CajaId,
                SucursalId = venta.SucursalId,
                UsuarioId = usuarioId,

                TipoMovimiento = "EGRESO",
                Concepto = "DEVOLUCION_VENTA",
                MedioPagoId = 1,

                Monto = montoEfectivo,
                AfectaEfectivo = true,

                Descripcion = $"Devolución por anulación de venta {venta.Serie}-{venta.Correlativo}",
                ReferenciaTabla = "notas_credito",
                ReferenciaId = notaCreditoId,

                Estado = "ACTIVO",
                FechaCreacion = DateTime.UtcNow
            };

            await _cajaMovimientoRepository.CreateAsync(movimientoCaja);
        }
        #endregion

        #region LISTAR VENTAS
        public async Task<PaginacionResponse<VentaListadoResponse>> ListarAsync(VentaFiltroRequest filtro)
        {
            var paginado = await _ventaRepository.ListarFiltradoAsync(filtro);

            var ventas = paginado.Items;

            var ventaIds = ventas
                .Select(x => x.Id)
                .ToList();

            var documentos = await _documentoElectronicoRepository
                .ObtenerPorVentasAsync(ventaIds);

            var documentosPorVenta = documentos
                .GroupBy(x => x.VentaId)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .OrderByDescending(x => x.FechaCreacion)
                        .ThenByDescending(x => x.Id)
                        .ToList()
                );

            var items = ventas.Select(venta =>
            {
                documentosPorVenta.TryGetValue(venta.Id, out var docsVenta);

                var documentoActual = docsVenta?.FirstOrDefault();

                var puedeAnular =
                    venta.EstadoSunat != "ANULADO" &&
                    (
                        venta.EstadoSunat == "ACEPTADO" ||
                        venta.SunatCodigo == "0"
                    );

                return new VentaListadoResponse
                {
                    Id = venta.Id,

                    TipoDocumento = venta.TipoDocumento,
                    TipoDocumentoNombre = ObtenerNombreTipoDocumento(venta.TipoDocumento),

                    Serie = venta.Serie,
                    Correlativo = venta.Correlativo,
                    Comprobante = $"{venta.Serie}-{venta.Correlativo}",

                    SucursalId = venta.SucursalId,
                    CajaId = venta.CajaId,
                    CajaSesionId = venta.CajaSesionId,

                    ClienteId = venta.ClienteId,
                    ClienteNombre = venta.ClienteNombre,

                    Subtotal = venta.Subtotal,
                    Impuesto = venta.Impuesto,
                    Descuento = venta.Descuento ?? 0,
                    Total = venta.Total,

                    EstadoSunat = venta.EstadoSunat,
                    SunatCodigo = venta.SunatCodigo,
                    SunatMensaje = venta.SunatMensaje,

                    FechaCreacion = venta.FechaCreacion,

                    TieneDocumentos = docsVenta != null && docsVenta.Any(),
                    PuedeAnular = puedeAnular,
                    PuedeReintentarEmision = PuedeReintentarEmision(venta.EstadoSunat),
                    DocumentoActualId = documentoActual?.Id,
                    DocumentoActualTipo = documentoActual?.TipoDocumento,
                    DocumentoActualTipoNombre = documentoActual == null
                        ? null
                        : ObtenerNombreTipoDocumento(documentoActual.TipoDocumento),
                    DocumentoActualComprobante = documentoActual == null
                        ? null
                        : $"{documentoActual.Serie}-{documentoActual.Correlativo}",
                    DocumentoActualEstado = documentoActual?.Estado
                };
            }).ToList();

            return new PaginacionResponse<VentaListadoResponse>
            {
                Items = items,
                Total = paginado.Total,
                PaginaActual = paginado.PaginaActual,
                TotalPaginas = paginado.TotalPaginas,
                CantidadPorPagina = paginado.CantidadPorPagina
            };
        }
        private static bool PuedeReintentarEmision(string? estadoSunat)
        {
            return estadoSunat == "PENDIENTE_ENVIO" ||
                   estadoSunat == "ERROR_ENVIO" ||
                   estadoSunat == "ERROR_CONSULTA";
        }
        private static string ObtenerNombreTipoDocumento(string tipo)
        {
            return tipo switch
            {
                "01" => "FACTURA",
                "03" => "BOLETA",
                "07" => "NOTA DE CRÉDITO",
                "08" => "NOTA DE DÉBITO",
                _ => "DOCUMENTO"
            };
        }

        public async Task<VentaDetalleCompletoResponse> ObtenerDetalleAsync(int ventaId)
        {
            if (ventaId <= 0)
                throw new Exception("El id de venta es obligatorio.");

            var venta = await _ventaRepository.GetByIdAsync(ventaId);

            if (venta == null)
                throw new Exception($"No se encontró la venta con id {ventaId}.");

            var detalles = await _ventaDetalleRepository.ObtenerPorVentaIdAsync(ventaId);

            var pagos = await _ventaPagoRepository.ObtenerPorVentaIdAsync(ventaId);

            var documentos = await _documentoElectronicoRepository.ObtenerPorVentaAsync(ventaId);

            var notaCredito = await _notaCreditoRepository.ObtenerPorVentaIdAsync(ventaId);

            var puedeAnular =
                venta.EstadoSunat != "ANULADO" &&
                (
                    venta.EstadoSunat == "ACEPTADO" ||
                    venta.SunatCodigo == "0"
                );

            return new VentaDetalleCompletoResponse
            {
                VentaId = venta.Id,

                TipoDocumento = venta.TipoDocumento,
                TipoDocumentoNombre = ObtenerNombreTipoDocumento(venta.TipoDocumento),
                Serie = venta.Serie,
                Correlativo = venta.Correlativo,
                Comprobante = $"{venta.Serie}-{venta.Correlativo}",

                SucursalId = venta.SucursalId,
                CajaId = venta.CajaId,
                CajaSesionId = venta.CajaSesionId,
                UsuarioId = venta.UsuarioId,

                ClienteId = venta.ClienteId,
                ClienteNombre = venta.ClienteNombre,

                Subtotal = venta.Subtotal,
                Impuesto = venta.Impuesto,
                Descuento = venta.Descuento ?? 0,
                Total = venta.Total,

                EstadoSunat = venta.EstadoSunat,
                SunatCodigo = venta.SunatCodigo,
                SunatMensaje = venta.SunatMensaje,

                FechaCreacion = venta.FechaCreacion,

                PuedeAnular = puedeAnular,

                NotaCredito = notaCredito == null
                    ? null
                    : new NotaCreditoResumenResponse
                    {
                        Id = notaCredito.Id,
                        Serie = notaCredito.Serie,
                        Correlativo = notaCredito.Correlativo,
                        Comprobante = $"{notaCredito.Serie}-{notaCredito.Correlativo}",
                        EstadoSunat = notaCredito.EstadoSunat,
                        SunatCodigo = notaCredito.SunatCodigo,
                        SunatMensaje = notaCredito.SunatMensaje
                    },

                Detalles = detalles.Select(x => new VentaDetalleItemResponse
                {
                    Id = x.Id,
                    ProductoId = x.ProductoId,
                    PresentacionId = x.PresentacionId,
                    Producto = x.NombreProducto,
                    Presentacion = x.PresentacionNombre,
                    Cantidad = x.Cantidad,
                    PrecioUnitario = x.PrecioUnitario,
                    Descuento = x.Descuento ?? 0,
                    Subtotal = x.Subtotal
                }).ToList(),

                Pagos = pagos.Select(x => new VentaPagoItemResponse
                {
                    Id = x.Id,
                    MedioPagoId = x.MedioPagoId,
                    MedioPagoNombre = x.MedioPago.Nombre,
                    Monto = x.Monto,
                    Referencia = x.Referencia
                }).ToList(),

                Documentos = documentos.Select(x => new DocumentoElectronicoResponse
                {
                    Id = x.Id,
                    VentaId = x.VentaId,
                    TipoDocumento = x.TipoDocumento,
                    TipoDocumentoNombre = ObtenerNombreTipoDocumento(x.TipoDocumento),
                    Serie = x.Serie,
                    Correlativo = x.Correlativo,
                    Comprobante = $"{x.Serie}-{x.Correlativo}",
                    Estado = x.Estado,
                    CodigoRespuesta = x.CodigoRespuesta,
                    DescripcionRespuesta = x.DescripcionRespuesta,
                    CodigoHash = x.CodigoHash,
                    FechaEnvio = x.FechaEnvio,
                    FechaRespuesta = x.FechaRespuesta,
                    FechaCreacion = x.FechaCreacion
                }).ToList()
            };
        }
        #endregion
    }
}
