using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.RequestResponse
{
    public class RegistrarEntradaStockResponse
    {
        public int ProductoId { get; set; }
        public int PresentacionId { get; set; }
        public int SucursalId { get; set; }

        public int CantidadPresentacion { get; set; }
        public int CantidadUnidades { get; set; }

        public int LoteId { get; set; }

        public int StockAnterior { get; set; }
        public int StockNuevo { get; set; }

        public string Mensaje { get; set; } = "Entrada de stock registrada correctamente.";
    }
}
