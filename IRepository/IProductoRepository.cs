using DBModel.Models;
using UtilInterface;

namespace IRepository
{
    public interface IProductoRepository : ICRUDRepositorio<Producto>
    {
        Task<List<Producto>>
            GetAutoCompleteAsync(
                string query);
    }
}
