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

    }
}
