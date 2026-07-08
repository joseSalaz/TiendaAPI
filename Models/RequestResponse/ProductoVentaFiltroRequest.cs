using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.RequestResponse
{
    public class ProductoVentaFiltroRequest
    {
        public string? Texto { get; set; }
        public int SucursalId { get; set; }
        public int Pagina { get; set; } = 1;
        public int Cantidad { get; set; } = 10;
    }
    public class ProductoVentaResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public string? CodigoInterno { get; set; }
        public string? CodigoBarras { get; set; }

        public string UnidadMedida { get; set; } = string.Empty;
        public decimal PrecioVenta { get; set; }

        public bool ManejaVencimiento { get; set; }

        public List<ProductoVentaPresentacionResponse> Presentaciones { get; set; } = new();
    }
    public class ProductoVentaPresentacionResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public string? CodigoBarras { get; set; }

        public int CantidadUnidades { get; set; }

        public decimal PrecioVenta { get; set; }
        public decimal? PrecioMayoreo { get; set; }

        public bool EsUnidadBase { get; set; }
    }
}
