using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.RequestResponse
{
    public class ProductoPresentacionRequest
    {
        public int? Id { get; set; }

        public string Nombre { get; set; } = null!;

        public string? CodigoBarras { get; set; }

        public int CantidadUnidades { get; set; }

        public decimal? PrecioCompra { get; set; }

        public decimal PrecioVenta { get; set; }

        public decimal? PrecioMayoreo { get; set; }

        public bool EsUnidadBase { get; set; }

        public bool PermiteVenta { get; set; } = true;

        public bool PermiteCompra { get; set; } = true;

        public string Estado { get; set; } = "ACTIVO";
    }
}
