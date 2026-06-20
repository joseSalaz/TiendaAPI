using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilPaginados.RequestResponse
{
    public class PaginacionResponse<T>
    {
        public List<T> Items { get; set; } = [];

        public int Total { get; set; }

        public int PaginaActual { get; set; }

        public int TotalPaginas { get; set; }

        public int CantidadPorPagina { get; set; }
    }
}
