using DBModel.DBModels;
using Models.RequestResponse;
using UtilInterface;
using UtilPaginados.RequestResponse;

namespace IRepository
{
    public interface IProductoRepository : ICRUDRepositorio<Producto>
    {
        Task<List<Producto>>
            GetAutoCompleteAsync(
                string query);
        IQueryable<Producto> GetQueryable();
<<<<<<< HEAD
        Task<Producto?> GetByCodigoBarras(string codigoBarras);
=======
        Task<Producto?> GetByIdConPresentacionesAsync(int id);
        Task<PaginacionResponse<ProductoVentaResponse>> BuscarParaVentaAsync(ProductoVentaFiltroRequest filtro);
>>>>>>> 787cd51adb08540b2ce86f2737763df37a392c8d
    }
}
