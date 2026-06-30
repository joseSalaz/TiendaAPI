using DBModel.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilInterface;

namespace IRepository
{
    public interface IVentaPagoRepository : ICRUDRepositorio<VentaPago>
    {
        Task<List<VentaPago>> ObtenerPorVentaIdAsync(int ventaId);
    }
}
