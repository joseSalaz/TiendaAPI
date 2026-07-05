using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ApisPeru
{
    public class DetalleSunatTemporal
    {
        public int ProductoId { get; set; }

        public int PresentacionId { get; set; }

        public string CodProducto { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public string UnidadSunat { get; set; } = "NIU";

        public string TipoAfectacionIgv { get; set; } = "10";

        public decimal Cantidad { get; set; }

        public decimal PrecioUnitarioConIgv { get; set; }

        public decimal DescuentoConIgv { get; set; }

        public decimal TotalConIgv { get; set; }
    }
}
