using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.RequestResponse
{
    public class CajaSesioneRequest
    {
        public int Id { get; set; }
        public int CajaId { get; set; }
        public int UsuarioAperturaId { get; set; }
        public DateTime FechaApertura { get; set; }
        public decimal MontoInicial { get; set; }
        public DateTime? FechaCierre { get; set; }
        public int? UsuarioCierreId { get; set; }
        public decimal? MontoCierre { get; set; }
        public decimal? Diferencia { get; set; }
        public string? Observaciones { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
