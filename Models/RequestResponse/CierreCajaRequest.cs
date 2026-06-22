using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.RequestResponse
{
    public class CierreCajaRequest
    {
        public int CajaSesionId { get; set; }
        public int UsuarioCierreId { get; set; }
        public decimal MontoCierre { get; set; }
        public string? Observaciones { get; set; }
    }
}
