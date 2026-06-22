using Models.Comon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.RequestResponse
{
    public class VueltoResponse
    {
        public decimal Total { get; set; }
        public decimal MontoRecibido { get; set; }
        public decimal Vuelto { get; set; }
        public List<DesgloseBillete> Desglose { get; set; } = new();
    }
}
