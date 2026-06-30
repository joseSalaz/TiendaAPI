using DBModel.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilInterface;

namespace IRepository
{
    public interface IProductoLoteRepository : ICRUDRepositorio<ProductoLote>
    {
        Task<ProductoStock?> GetByProductoSucursalAsync(int productoId, int sucursalId);
        Task<List<ProductoLote>> GetLotesDisponiblesFefoAsync(int productoId, int sucursalId);

    }
}
