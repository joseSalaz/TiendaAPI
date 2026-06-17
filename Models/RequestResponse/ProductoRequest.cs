using DBModel.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.RequestResponse
{
    public class ProductoRequest
    {

        public int Id { get; set; }

        public int? CategoriaId { get; set; }

        public string? CodigoBarras { get; set; }

        public string? CodigoInterno { get; set; }

        public string Nombre { get; set; } = null!;

        public string? Descripcion { get; set; }

        public string UnidadMedida { get; set; } = null!;

        public decimal? PrecioCompra { get; set; }

        public decimal PrecioVenta { get; set; }

        public decimal? PrecioMayoreo { get; set; }

        public bool? PermiteStockNegativo { get; set; }

        public string Estado { get; set; } = null!;

        public string? ImagenUrl { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime FechaActualizacion { get; set; }
    }
}
