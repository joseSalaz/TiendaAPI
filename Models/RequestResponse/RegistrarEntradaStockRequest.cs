using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.RequestResponse
{
    public class RegistrarEntradaStockRequest
    {
        public int ProductoId { get; set; }
        public int PresentacionId { get; set; }
        public int SucursalId { get; set; }
        public int UsuarioId { get; set; }

        public int CantidadPresentacion { get; set; }

        // Opcional: solo si quieres sobrescribir el precio de compra de esa entrada
        public decimal? PrecioCompra { get; set; }

        public string? CodigoLote { get; set; }
        public DateOnly? FechaVencimiento { get; set; }
        public string? Observacion { get; set; }
    }
}
