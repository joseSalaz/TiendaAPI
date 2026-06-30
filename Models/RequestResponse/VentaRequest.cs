using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.RequestResponse
{
    public class VentaRequest
    {

    }
    public class VentaFiltroRequest
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public int? SucursalId { get; set; }
        public int? CajaId { get; set; }
        public int? ClienteId { get; set; }

        public string? TipoDocumento { get; set; }
        public string? EstadoSunat { get; set; }
        public string? Serie { get; set; }
        public int? Correlativo { get; set; }

        public int Pagina { get; set; } = 1;
        public int Cantidad { get; set; } = 20;
    }


    public class VentaListadoResponse
    {
        public int Id { get; set; }

        public string TipoDocumento { get; set; } = string.Empty;
        public string TipoDocumentoNombre { get; set; } = string.Empty;

        public string Serie { get; set; } = string.Empty;
        public int Correlativo { get; set; }
        public string Comprobante { get; set; } = string.Empty;

        public int SucursalId { get; set; }
        public int? CajaId { get; set; }
        public int? CajaSesionId { get; set; }

        public int? ClienteId { get; set; }
        public string? ClienteNombre { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }

        public string EstadoSunat { get; set; } = string.Empty;
        public string? SunatCodigo { get; set; }
        public string? SunatMensaje { get; set; }

        public DateTime FechaCreacion { get; set; }

        public bool TieneDocumentos { get; set; }
        public bool PuedeAnular { get; set; }
        public bool PuedeReintentarEmision { get; set; } 
        public int? DocumentoActualId { get; set; }
        public string? DocumentoActualTipo { get; set; }
        public string? DocumentoActualTipoNombre { get; set; }
        public string? DocumentoActualComprobante { get; set; }
        public string? DocumentoActualEstado { get; set; }
    }


    public class VentaDetalleCompletoResponse
    {
        public int VentaId { get; set; }

        public string TipoDocumento { get; set; } = string.Empty;
        public string TipoDocumentoNombre { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public int Correlativo { get; set; }
        public string Comprobante { get; set; } = string.Empty;

        public int SucursalId { get; set; }
        public int? CajaId { get; set; }
        public int? CajaSesionId { get; set; }
        public int UsuarioId { get; set; }

        public int? ClienteId { get; set; }
        public string? ClienteNombre { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }

        public string EstadoSunat { get; set; } = string.Empty;
        public string? SunatCodigo { get; set; }
        public string? SunatMensaje { get; set; }

        public DateTime FechaCreacion { get; set; }

        public bool PuedeAnular { get; set; }

        public NotaCreditoResumenResponse? NotaCredito { get; set; }

        public List<VentaDetalleItemResponse> Detalles { get; set; } = new();

        public List<VentaPagoItemResponse> Pagos { get; set; } = new();

        public List<DocumentoElectronicoResponse> Documentos { get; set; } = new();
    }

    public class VentaDetalleItemResponse
    {
        public int Id { get; set; }

        public int ProductoId { get; set; }

        public int? PresentacionId { get; set; }

        public string Producto { get; set; } = string.Empty;

        public string? Presentacion { get; set; }

        public decimal Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Descuento { get; set; }

        public decimal Subtotal { get; set; }
    }

    public class VentaPagoItemResponse
    {
        public int Id { get; set; }

        public int MedioPagoId { get; set; }

        public string? MedioPagoNombre { get; set; }

        public decimal Monto { get; set; }

        public string? Referencia { get; set; }
    }

    public class NotaCreditoResumenResponse
    {
        public int Id { get; set; }

        public string Serie { get; set; } = string.Empty;

        public int Correlativo { get; set; }

        public string Comprobante { get; set; } = string.Empty;

        public string EstadoSunat { get; set; } = string.Empty;

        public string? SunatCodigo { get; set; }

        public string? SunatMensaje { get; set; }
    }
}
