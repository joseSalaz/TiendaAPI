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
    public partial class VentaBussines
    {
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
    }
}
