using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBModel.DBModels;

namespace UtilPDF.ComprobantesPdf.RequestResponse
{
    public class ComprobantePdfData
    {
        public string RucEmpresa { get; set; } = string.Empty;
        public string RazonSocialEmpresa { get; set; } = string.Empty;
        public string NombreComercialEmpresa { get; set; } = string.Empty;
        public string DireccionEmpresa { get; set; } = string.Empty;
        public string ProvinciaEmpresa { get; set; } = string.Empty;
        public string DepartamentoEmpresa { get; set; } = string.Empty;
        public string DistritoEmpresa { get; set; } = string.Empty;
        public string? LogoUrl { get; set; } = "https://img.magnific.com/vector-gratis/plantilla-diseno-logotipo-verificacion-humana_474888-1825.jpg?semt=ais_hybrid&w=740&q=80";
        public byte[] LogoBytes { get; set; } = Array.Empty<byte>();
        public string TipoDocumento { get; set; } = string.Empty;
        public string TipoDocumentoNombre { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public int Correlativo { get; set; }
        public string? Telefono { get; set; }
        public string ? Email { get; set; }

        public DateTime FechaEmision { get; set; }

        public string ClienteNombre { get; set; } = "Cliente general";
        public string ClienteTipoDocumento { get; set; } = "0";
        public string ClienteNumeroDocumento { get; set; } = "-";
        public string ClienteDireccion { get; set; } = "-";

        public decimal Subtotal { get; set; }
        public decimal Igv { get; set; }
        public decimal DescuentoGlobal { get; set; }
        public decimal Total { get; set; }

        public string? Hash { get; set; }
        public string TextoQr { get; set; } = string.Empty;
        public byte[] QrPng { get; set; } = Array.Empty<byte>();

        public List<ComprobantePdfDetalle> Detalles { get; set; } = new();
        public List<ComprobantePdfPago> Pagos { get; set; } = new();
    }

    public class ComprobantePdfDetalle
    {
        public int Item { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Unidad { get; set; } = "NIU";

        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class ComprobantePdfPago
    {
        public string MedioPago { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? Referencia { get; set; }
    }
}
