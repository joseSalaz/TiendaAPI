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

    }
}
