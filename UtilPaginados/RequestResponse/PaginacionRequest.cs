using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilPaginados.RequestResponse
{
    public class PaginacionRequest
    {
        public int Pagina { get; set; } = 1;
        public int Cantidad { get; set; } = 10;
    }
}
