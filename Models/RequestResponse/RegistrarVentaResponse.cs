using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.RequestResponse
{
    public class RegistrarVentaResponse
    {
        public int VentaId { get; set; }

        public string TipoDocumento { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public int Correlativo { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }

        public string EstadoSunat { get; set; } = "PENDIENTE_ENVIO";
        public string Mensaje { get; set; } = "Venta registrada correctamente.";
        public string? SunatCodigo { get; set; }

        public string? SunatMensaje { get; set; }
    }
}
