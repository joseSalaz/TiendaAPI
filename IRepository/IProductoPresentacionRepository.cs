using DBModel.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilInterface;

namespace IRepository
{
    public interface IProductoPresentacionRepository : ICRUDRepositorio<ProductoPresentacione>
    {
        Task<List<ProductoPresentacione>> GetAutoCompleteAsync(string query);
        IQueryable<ProductoPresentacione> GetQueryable();
        Task CreateRangeAsync(List<ProductoPresentacione> presentaciones);

        Task<bool> ExistsCodigoBarrasAsync(string codigoBarras);

        Task<List<ProductoPresentacione>> GetByProductoIdAsync(int productoId);
        Task<ProductoPresentacione?> GetByIdAndProductoAsync(int id, int productoId);
    }
}
