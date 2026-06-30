using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.RequestResponse
{
    public class AnularVentaRequest
    {
        public int VentaId { get; set; }

        public int UsuarioId { get; set; }

        // Catálogo 09 SUNAT
        public string CodigoMotivo { get; set; } = "01";

        public string DescripcionMotivo { get; set; } = "ANULACIÓN DE LA OPERACIÓN";

        public bool DevolverDineroCaja { get; set; } = true;

        public string? Observacion { get; set; }
    }
    public class AnularVentaResponse
    {
        public int VentaId { get; set; }

        public int NotaCreditoId { get; set; }

        public string VentaOriginal { get; set; } = string.Empty;

        public string NotaCredito { get; set; } = string.Empty;

        public string EstadoVenta { get; set; } = string.Empty;

        public string EstadoNotaCredito { get; set; } = string.Empty;

        public decimal TotalDevuelto { get; set; }

        public string? SunatCodigo { get; set; }

        public string? SunatMensaje { get; set; }

        public string Mensaje { get; set; } = string.Empty;
    }
}
