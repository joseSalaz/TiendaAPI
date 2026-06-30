using DBModel.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilInterface;

namespace IRepository
{
    public interface IProductoStockRepository : ICRUDRepositorio<ProductoStock>
    {
        Task CreateRangeAsync(List<ProductoStock> stocks);

        Task<bool> ExistsAsync(int productoId, int sucursalId);
        Task<ProductoStock?> GetByProductoSucursalAsync(int productoId, int sucursalId);
    }
}
