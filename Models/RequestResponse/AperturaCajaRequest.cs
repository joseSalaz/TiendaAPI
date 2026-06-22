using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.RequestResponse
{
    public class AperturaCajaRequest
    {
        public int CajaId { get; set; }
        public int UsuarioAperturaId { get; set; }
        public decimal MontoInicial { get; set; }
        public string? Observaciones { get; set; }
    }
}
