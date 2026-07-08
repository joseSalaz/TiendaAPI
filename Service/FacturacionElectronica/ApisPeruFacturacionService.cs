using IService.FacturacionElectronica;
using Microsoft.Extensions.Options;
using Models.ApisPeru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service.FacturacionElectronica
{
    public class ApisPeruFacturacionService : IApisPeruFacturacionService
    {
        private readonly HttpClient _httpClient;
        private readonly ApisPeruOptions _options;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        public ApisPeruFacturacionService(
            HttpClient httpClient,
            IOptions<ApisPeruOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }
        #region Emision Electronica
        public async Task<ApisPeruFacturaResponse> EnviarFacturaBoletaAsync(object payload)
        {
            if (string.IsNullOrWhiteSpace(_options.CompanyToken))
                throw new Exception("No se configuró el token de empresa de APISPERU.");

            var json = JsonSerializer.Serialize(payload, _jsonOptions);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                _options.InvoiceSendPath
            );

            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _options.CompanyToken
            );

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            using var response = await _httpClient.SendAsync(request);

            var rawJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ApisPeruFacturaResponse
                {
                    Success = false,
                    RawJson = rawJson,
                    Error = new ApisPeruErrorResponse
                    {
                        Code = response.StatusCode.ToString(),
                        Message = rawJson
                    }
                };
            }

            try
            {
                var result = JsonSerializer.Deserialize<ApisPeruFacturaResponse>(
                    rawJson,
                    _jsonOptions
                );

                if (result == null)
                {
                    return new ApisPeruFacturaResponse
                    {
                        Success = false,
                        RawJson = rawJson,
                        Error = new ApisPeruErrorResponse
                        {
                            Code = "DESERIALIZE_ERROR",
                            Message = "No se pudo leer la respuesta de APISPERU."
                        }
                    };
                }

                result.RawJson = rawJson;

                if (!result.Success && result.SunatResponse != null)
                {
                    result.Success = result.SunatResponse.Success;
                }

                if (result.Error == null && result.SunatResponse?.Error != null)
                {
                    result.Error = result.SunatResponse.Error;
                }

                return result;
            }
            catch (Exception ex)
            {
                return new ApisPeruFacturaResponse
                {
                    Success = false,
                    RawJson = rawJson,
                    Error = new ApisPeruErrorResponse
                    {
                        Code = "DESERIALIZE_ERROR",
                        Message = ex.Message
                    }
                };
            }
        }
        #endregion

        #region GENERAR ARCHIVOS
        public Task<ApisPeruArchivoResponse> GenerarPdfFacturaBoletaAsync(object payload)
        {
            return DescargarArchivoAsync(
                payload,
                _options.InvoicePdfPath,
                "application/pdf"
            );
        }
        public Task<ApisPeruArchivoResponse> GenerarXmlFacturaBoletaAsync(object payload)
        {
            return DescargarArchivoAsync(
                payload,
                _options.InvoiceXmlPath,
                "application/xml"
            );
        }

        public Task<ApisPeruArchivoResponse> GenerarPdfNotaCreditoAsync(object payload)
        {
            return DescargarArchivoAsync(
                payload,
                _options.NotePdfPath,
                "application/pdf"
            );
        }

        public Task<ApisPeruArchivoResponse> GenerarXmlNotaCreditoAsync(object payload)
        {
            return DescargarArchivoAsync(
                payload,
                _options.NoteXmlPath,
                "application/xml"
            );
        }
        #endregion

        #region Consultar estado CDR
        public async Task<ApisPeruEstadoResponse> ConsultarEstadoFacturaBoletaAsync(string tipoDocumento, string serie, int correlativo, string ruc)
        {
            if (string.IsNullOrWhiteSpace(_options.CompanyToken))
                throw new Exception("No se configuró el token de empresa de APISPERU.");

            var url = $"{_options.InvoiceStatusPath}" +
                      $"?tipo={Uri.EscapeDataString(tipoDocumento)}" +
                      $"&serie={Uri.EscapeDataString(serie)}" +
                      $"&numero={correlativo}" +
                      $"&ruc={Uri.EscapeDataString(ruc)}";

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                url
            );

            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _options.CompanyToken
            );

            using var response = await _httpClient.SendAsync(request);

            var rawJson = await response.Content.ReadAsStringAsync();

            try
            {
                var result = JsonSerializer.Deserialize<ApisPeruEstadoResponse>(
                    rawJson,
                    _jsonOptions
                );

                if (result == null)
                {
                    return new ApisPeruEstadoResponse
                    {
                        Success = false,
                        RawJson = rawJson,
                        Error = new ApisPeruErrorResponse
                        {
                            Code = "DESERIALIZE_ERROR",
                            Message = "No se pudo leer la respuesta de APISPERU."
                        }
                    };
                }

                result.RawJson = rawJson;
                return result;
            }
            catch (Exception ex)
            {
                return new ApisPeruEstadoResponse
                {
                    Success = false,
                    RawJson = rawJson,
                    Error = new ApisPeruErrorResponse
                    {
                        Code = "DESERIALIZE_ERROR",
                        Message = ex.Message
                    }
                };
            }
        }
        #endregion

        #region NOTAS DE CREDITO
        public async Task<ApisPeruFacturaResponse> EnviarNotaCreditoAsync(object payload)
        {
            if (string.IsNullOrWhiteSpace(_options.CompanyToken))
                throw new Exception("No se configuró el token de empresa de APISPERU.");

            var json = JsonSerializer.Serialize(payload, _jsonOptions);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                _options.NoteSendPath
            );

            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _options.CompanyToken
            );

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            using var response = await _httpClient.SendAsync(request);

            var rawJson = await response.Content.ReadAsStringAsync();

            try
            {
                var result = JsonSerializer.Deserialize<ApisPeruFacturaResponse>(
                    rawJson,
                    _jsonOptions
                );

                if (result == null)
                {
                    return new ApisPeruFacturaResponse
                    {
                        Success = false,
                        RawJson = rawJson,
                        Error = new ApisPeruErrorResponse
                        {
                            Code = "DESERIALIZE_ERROR",
                            Message = "No se pudo leer la respuesta de APISPERU."
                        }
                    };
                }

                result.RawJson = rawJson;

                if (!result.Success && result.SunatResponse != null)
                    result.Success = result.SunatResponse.Success;

                if (result.Error == null && result.SunatResponse?.Error != null)
                    result.Error = result.SunatResponse.Error;

                return result;
            }
            catch (Exception ex)
            {
                return new ApisPeruFacturaResponse
                {
                    Success = false,
                    RawJson = rawJson,
                    Error = new ApisPeruErrorResponse
                    {
                        Code = "DESERIALIZE_ERROR",
                        Message = ex.Message
                    }
                };
            }
        }
        #endregion

        #region HELPER DESCARGAR ARCHIVOS
        private async Task<ApisPeruArchivoResponse> DescargarArchivoAsync(object payload, string path, string contentTypeDefault)
        {
            if (string.IsNullOrWhiteSpace(_options.CompanyToken))
                throw new Exception("No se configuró el token de empresa de APISPERU.");

            var json = JsonSerializer.Serialize(payload, _jsonOptions);

            using var request = new HttpRequestMessage(HttpMethod.Post, path);

            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _options.CompanyToken
            );

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            using var response = await _httpClient.SendAsync(request);

            var contentType = response.Content.Headers.ContentType?.MediaType
                ?? contentTypeDefault;

            var bytes = await response.Content.ReadAsByteArrayAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ApisPeruArchivoResponse
                {
                    Success = false,
                    Bytes = null,
                    ContentType = contentType,
                    RawError = Encoding.UTF8.GetString(bytes)
                };
            }

            if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                var raw = Encoding.UTF8.GetString(bytes);

                if (raw.Contains("\"success\":false", StringComparison.OrdinalIgnoreCase))
                {
                    return new ApisPeruArchivoResponse
                    {
                        Success = false,
                        Bytes = null,
                        ContentType = contentType,
                        RawError = raw
                    };
                }
            }

            return new ApisPeruArchivoResponse
            {
                Success = true,
                Bytes = bytes,
                ContentType = contentType
            };
        }
        #endregion

    }
}
