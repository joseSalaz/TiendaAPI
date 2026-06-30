using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.RequestResponse
{
    public class DocumentoElectronicoResponse
    {
        public int Id { get; set; }
        public int VentaId { get; set; }

        public string TipoDocumento { get; set; } = string.Empty;
        public string TipoDocumentoNombre { get; set; } = string.Empty;

        public string Serie { get; set; } = string.Empty;
        public int Correlativo { get; set; }
        public string Comprobante { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;
        public string? CodigoRespuesta { get; set; }
        public string? DescripcionRespuesta { get; set; }
        public string? CodigoHash { get; set; }

        public DateTime? FechaEnvio { get; set; }
        public DateTime? FechaRespuesta { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
    public class DocumentoElectronicoFiltroRequest
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public string? TipoDocumento { get; set; }
        public string? Estado { get; set; }
        public string? Serie { get; set; }
        public int? Correlativo { get; set; }

        public int Pagina { get; set; } = 1;
        public int Cantidad { get; set; } = 20;
    }
}
