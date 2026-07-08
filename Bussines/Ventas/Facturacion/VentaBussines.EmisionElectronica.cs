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

        private async Task IntentarEmitirComprobanteElectronicoAsync(
            Venta venta,
            object payloadEmision
        )
        {
            DocumentosElectronico? documento = null;

            try
            {
                documento = await ObtenerOCrearDocumentoElectronicoAsync(venta);

                documento.FechaEnvio = DateTime.UtcNow;
                documento.Estado = "PENDIENTE";

                await _unitOfWork.SaveChangesAsync();

                var respuesta = await _apisPeruFacturacionService
                    .EnviarFacturaBoletaAsync(payloadEmision);

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

                venta.EstadoSunat = estadoSunat;
                venta.SunatCodigo = codigoRespuesta;
                venta.SunatMensaje = descripcionRespuesta;
                venta.SunatHash = respuesta.Hash;
                venta.RespuestaSunatJson = respuesta.RawJson ?? "{}";
                venta.FechaEnvioSunat = DateTime.UtcNow;

                documento.FechaRespuesta = DateTime.UtcNow;
                documento.CodigoRespuesta = Limitar(codigoRespuesta, 10);
                documento.DescripcionRespuesta = descripcionRespuesta;
                documento.CdrNumero = Limitar(respuesta.CdrResponse?.Id, 20);
                documento.CdrDescripcion = respuesta.CdrResponse?.Description;
                documento.XmlEnviado = respuesta.Xml;
                documento.XmlRespuesta = respuesta.SunatResponse?.CdrZip ?? respuesta.RawJson;
                documento.CodigoHash = respuesta.Hash;

                documento.SunatXmlId = Limitar(
                    $"{venta.TipoDocumento}-{venta.Serie}-{venta.Correlativo}",
                    100
                );

                documento.Estado = estadoSunat;

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                documento ??= await ObtenerOCrearDocumentoElectronicoAsync(venta);

                venta.EstadoSunat = "ERROR_ENVIO";
                venta.SunatCodigo = "REMOTE_ERR";
                venta.SunatMensaje = ex.Message;
                venta.RespuestaSunatJson = JsonSerializer.Serialize(new
                {
                    error = ex.Message,
                    fecha = DateTime.UtcNow
                });
                venta.FechaEnvioSunat = DateTime.UtcNow;

                documento.FechaRespuesta = DateTime.UtcNow;
                documento.CodigoRespuesta = "REMOTE_ERR";
                documento.DescripcionRespuesta = ex.Message;
                documento.CdrDescripcion = ex.Message;
                documento.XmlRespuesta = venta.RespuestaSunatJson;
                documento.Estado = "ERROR_ENVIO";

                await _unitOfWork.SaveChangesAsync();
            }
        }
        private async Task<object> ConstruirPayloadFacturaBoletaLocalAsync(Venta venta, List<DetalleSunatTemporal> detallesSunat)
        {
            if (detallesSunat == null || detallesSunat.Count == 0)
                throw new Exception("La venta no tiene detalles para emitir comprobante electrónico.");

            var empresa = await _empresaRepository.ObtenerEmpresaPorSucursalAsync(venta.SucursalId);

            if (empresa == null)
                throw new Exception("No se encontró la empresa emisora para la sucursal de la venta.");

            try
            {
                var payload = _apisPeruPayloadBuilder.ConstruirPayloadApisPeru(
                    venta,
                    detallesSunat,
                    empresa
                );

                // Fuerza validaciones de serialización antes del commit.
                _ = JsonSerializer.Serialize(payload);

                return payload;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error local al preparar el comprobante electrónico: {ex.Message}",
                    ex
                );
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
            var payloadEmision = await ConstruirPayloadFacturaBoletaLocalAsync(venta,detallesSunat);
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
                payloadEmision
            );

            return MapToRegistrarVentaResponse(
                venta,
                venta.EstadoSunat == "ACEPTADO"
                    ? "Comprobante electrónico aceptado."
                    : "Se reintentó la emisión electrónica."
            );
        }
    }
}
