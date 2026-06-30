using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.RequestResponse
{
    public class ClienteResponse
    {

        public int Id { get; set; }
        public string TipoDocumento { get; set; } = null!;
        public string? NumeroDocumento { get; set; }
        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public string? RazonSocial { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Ubigeo { get; set; }
        public string Estado { get; set; } = null!;
        public DateTime FechaCreacion { get; set; }
    }
}
