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
        Task<Producto?> GetByIdConPresentacionesAsync(int id);
        Task<PaginacionResponse<ProductoVentaResponse>> BuscarParaVentaAsync(ProductoVentaFiltroRequest filtro);
    }
}
