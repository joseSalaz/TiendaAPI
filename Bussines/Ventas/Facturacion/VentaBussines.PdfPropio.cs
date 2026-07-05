using DBModel.DBModels;
using Models.ApisPeru;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilPDF.ComprobantesPdf.RequestResponse;

namespace Bussines
{
    public partial class VentaBussines
    {
        public async Task<ArchivoVentaResponse> DescargarPdfPropioA4Async(int ventaId)
        {
            var data = await ConstruirComprobantePdfDataAsync(ventaId);

            var bytes = await _comprobantePdfService.GenerarA4Async(data);

            return new ArchivoVentaResponse
            {
                Bytes = bytes,
                ContentType = "application/pdf",
                NombreArchivo = $"{data.RucEmpresa}-{data.TipoDocumento}-{data.Serie}-{data.Correlativo}-A4.pdf"
            };
        }

        public async Task<ArchivoVentaResponse> DescargarPdfPropioTicketAsync(int ventaId)
        {
            var data = await ConstruirComprobantePdfDataAsync(ventaId);

            var bytes = await _comprobantePdfService.GenerarTicket80mmAsync(data);

            return new ArchivoVentaResponse
            {
                Bytes = bytes,
                ContentType = "application/pdf",
                NombreArchivo = $"{data.RucEmpresa}-{data.TipoDocumento}-{data.Serie}-{data.Correlativo}-TICKET.pdf"
            };
        }

        private async Task<ComprobantePdfData> ConstruirComprobantePdfDataAsync(int ventaId)
        {
            if (ventaId <= 0)
                throw new Exception("El id de la venta es obligatorio.");

            var venta = await _ventaRepository.GetByIdAsync(ventaId);

            if (venta == null)
                throw new Exception($"No se encontró la venta con id {ventaId}.");

            if (venta.EstadoSunat != "ACEPTADO" && venta.SunatCodigo != "0")
                throw new Exception("Solo se puede generar el PDF oficial propio de una venta aceptada por SUNAT.");

            var empresa = await _empresaRepository.ObtenerEmpresaPorSucursalAsync(venta.SucursalId);

            if (empresa == null)
                throw new Exception("No se encontró la empresa emisora para la sucursal de la venta.");

            var detalles = await _ventaDetalleRepository.ObtenerPorVentaIdAsync(ventaId);

            if (detalles == null || !detalles.Any())
                throw new Exception("La venta no tiene detalles.");

            var pagos = await _ventaPagoRepository.ObtenerPorVentaIdAsync(ventaId);

            var textoQr = ConstruirTextoQrPdfPropio(empresa, venta);

            var data = new ComprobantePdfData
            {
                RucEmpresa = empresa.Ruc,
                RazonSocialEmpresa = empresa.RazonSocial,
                NombreComercialEmpresa = empresa.NombreComercial ?? empresa.RazonSocial,
                DireccionEmpresa = empresa.DireccionFiscal,
                ProvinciaEmpresa = empresa.Provincia,
                DepartamentoEmpresa = empresa.Departamento,
                DistritoEmpresa = empresa.Distrito,

                TipoDocumento = venta.TipoDocumento,
                TipoDocumentoNombre = ObtenerNombreTipoDocumentoPdf(venta.TipoDocumento),
                Serie = venta.Serie,
                Correlativo = venta.Correlativo,

                FechaEmision = venta.FechaCreacion,

                ClienteNombre = string.IsNullOrWhiteSpace(venta.ClienteNombre)
                    ? "Cliente general"
                    : venta.ClienteNombre,

                ClienteTipoDocumento = string.IsNullOrWhiteSpace(venta.ClienteTipoDoc)
                    ? "0"
                    : venta.ClienteTipoDoc,

                ClienteNumeroDocumento = string.IsNullOrWhiteSpace(venta.ClienteNumeroDoc)
                    ? "-"
                    : venta.ClienteNumeroDoc,

                ClienteDireccion = string.IsNullOrWhiteSpace(venta.ClienteDireccion)
                    ? "-"
                    : venta.ClienteDireccion,

                Subtotal = venta.Subtotal,
                Igv = venta.Impuesto,
                DescuentoGlobal = venta.Descuento ?? 0,
                Total = venta.Total,

                Hash = venta.SunatHash,
                TextoQr = textoQr,

                Detalles = detalles.Select((detalle, index) => new ComprobantePdfDetalle
                {
                    Item = index + 1,

                    Codigo = detalle.ProductoCodigoInterno
                        ?? detalle.CodigoBarras
                        ?? detalle.ProductoId.ToString(),

                    Descripcion = string.IsNullOrWhiteSpace(detalle.PresentacionNombre)
                        ? detalle.NombreProducto
                        : $"{detalle.NombreProducto} - {detalle.PresentacionNombre}",

                    Unidad = string.IsNullOrWhiteSpace(detalle.UnidadSunat)
                        ? "NIU"
                        : detalle.UnidadSunat,

                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    Descuento = detalle.Descuento ?? 0,
                    Subtotal = detalle.Subtotal
                }).ToList(),

                Pagos = pagos.Select(pago => new ComprobantePdfPago
                {
                    MedioPago = pago.MedioPago?.Nombre ?? $"Medio pago {pago.MedioPagoId}",
                    Monto = pago.Monto,
                    Referencia = pago.Referencia
                }).ToList()
            };

            return data;
        }

        private static string ConstruirTextoQrPdfPropio(Empresa empresa, Venta venta)
        {
            var fechaPeru = venta.FechaCreacion.Kind == DateTimeKind.Utc
                ? venta.FechaCreacion.AddHours(-5)
                : venta.FechaCreacion;

            var fecha = fechaPeru.ToString("yyyy-MM-dd");

            var tipoDocCliente = NormalizarTipoDocClienteQr(venta.ClienteTipoDoc);

            var numeroDocCliente = string.IsNullOrWhiteSpace(venta.ClienteNumeroDoc)
                ? "-"
                : venta.ClienteNumeroDoc.Trim();

            var igv = FormatearDecimalQr(venta.Impuesto);
            var total = FormatearDecimalQr(venta.Total);

            var hash = venta.SunatHash ?? "";

            return string.Join("|", new[]
            {
                empresa.Ruc,
                venta.TipoDocumento,
                venta.Serie,
                venta.Correlativo.ToString(),
                igv,
                total,
                fecha,
                tipoDocCliente,
                numeroDocCliente,
                hash,
                ""
            }) + "|";
        }

        private static string NormalizarTipoDocClienteQr(string? tipoDocumento)
        {
            var tipo = tipoDocumento?.Trim().ToUpper();

            return tipo switch
            {
                "DNI" => "1",
                "1" => "1",
                "RUC" => "6",
                "6" => "6",
                "CE" => "4",
                "4" => "4",
                "PASAPORTE" => "7",
                "7" => "7",
                null or "" => "0",
                _ => "0"
            };
        }

        private static string FormatearDecimalQr(decimal value)
        {
            return Math.Round(value, 2)
                .ToString("0.00", CultureInfo.InvariantCulture);
        }

        private static string ObtenerNombreTipoDocumentoPdf(string tipoDocumento)
        {
            return tipoDocumento switch
            {
                "01" => "FACTURA",
                "03" => "BOLETA DE VENTA",
                "07" => "NOTA DE CRÉDITO",
                "08" => "NOTA DE DÉBITO",
                _ => "DOCUMENTO"
            };
        }
    }
}
