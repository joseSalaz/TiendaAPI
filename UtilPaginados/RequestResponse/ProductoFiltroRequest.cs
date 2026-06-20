using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilPaginados.RequestResponse;

namespace UtilPaginados
{
    public class ProductoFiltroRequest : PaginacionRequest
    {
        public string? Nombre { get; set; }

        public string? Estado { get; set; }

        public int? CategoriaId { get; set; }
    }
}
