using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Comon
{
    public class ProductoScannedDto
    {
        public string? CodigoBarras { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string? ImagenUrl { get; set; }
        public string? CategoriaTexto { get; set; } 
        public bool Encontrado { get; set; }
        public string? SourceApi { get; set; }
    }
}
