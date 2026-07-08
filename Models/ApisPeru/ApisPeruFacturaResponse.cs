using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models.ApisPeru
{
    public class ApisPeruFacturaResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("xml")]
        public string? Xml { get; set; }

        [JsonPropertyName("hash")]
        public string? Hash { get; set; }

        [JsonPropertyName("sunatResponse")]
        public ApisPeruSunatResponse? SunatResponse { get; set; }

        [JsonPropertyName("error")]
        public ApisPeruErrorResponse? Error { get; set; }

        [JsonIgnore]
        public ApisPeruCdrResponse? CdrResponse => SunatResponse?.CdrResponse;

        [JsonIgnore]
        public string? RawJson { get; set; }
    }

    public class ApisPeruSunatResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("cdrZip")]
        public string? CdrZip { get; set; }

        [JsonPropertyName("cdrResponse")]
        public ApisPeruCdrResponse? CdrResponse { get; set; }

        [JsonPropertyName("error")]
        public ApisPeruErrorResponse? Error { get; set; }
    }

    public class ApisPeruCdrResponse
    {
        [JsonPropertyName("accepted")]
        public bool? Accepted { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("notes")]
        public List<string>? Notes { get; set; }
    }

    public class ApisPeruErrorResponse
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    public class ApisPeruArchivoResponse
    {
        public bool Success { get; set; }
        public byte[]? Bytes { get; set; }
        public string ContentType { get; set; } = "application/pdf";
        public string? RawError { get; set; }
    }
    public class ArchivoVentaResponse
    {
        public byte[] Bytes { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "application/pdf";
        public string NombreArchivo { get; set; } = "comprobante.pdf";
    }

    public class ApisPeruEstadoResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("cdrZip")]
        public string? CdrZip { get; set; }

        [JsonPropertyName("cdrResponse")]
        public ApisPeruCdrResponse? CdrResponse { get; set; }

        [JsonPropertyName("error")]
        public ApisPeruErrorResponse? Error { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonIgnore]
        public string? RawJson { get; set; }
    }

    public class EstadoSunatConsultaResponse
    {
        public int VentaId { get; set; }

        public string TipoDocumento { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public int Correlativo { get; set; }

        public string EstadoLocal { get; set; } = string.Empty;
        public string? CodigoLocal { get; set; }
        public string? MensajeLocal { get; set; }

        public bool ConsultaExitosa { get; set; }

        public string? CodigoConsulta { get; set; }
        public string? MensajeConsulta { get; set; }

        public string? EstadoConsulta { get; set; }

        public string? RawJson { get; set; }
    }
    #region NOTAS DE CREDITO
    public class EmitirNotaCreditoRequest
    {
        public int VentaId { get; set; }

        // 01 = Anulación de la operación
        public string CodigoMotivo { get; set; } = "01";

        public string DescripcionMotivo { get; set; } = "ANULACIÓN DE LA OPERACIÓN";
    }
    public class EmitirNotaCreditoResponse
    {
        public int NotaCreditoId { get; set; }

        public int VentaId { get; set; }

        public string TipoDocumento { get; set; } = "07";

        public string Serie { get; set; } = string.Empty;

        public int Correlativo { get; set; }

        public string Comprobante { get; set; } = string.Empty;

        public string EstadoSunat { get; set; } = string.Empty;

        public string? SunatCodigo { get; set; }

        public string? SunatMensaje { get; set; }

        public string Mensaje { get; set; } = string.Empty;
    }
    #endregion
}
