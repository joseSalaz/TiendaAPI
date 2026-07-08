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


    }
}
