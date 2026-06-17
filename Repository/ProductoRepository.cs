using DBModel.Models;
using IRepository;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class ProductoRepository :
        GenericRepository<Producto>,
        IProductoRepository
    {
        public ProductoRepository(
            _TiendaDbContext context)
            : base(context)
        {
        }

        public async Task<List<Producto>>
            GetAutoCompleteAsync(
                string query)
        {
            return await _dbSet
                .Where(x =>
                    x.Nombre.Contains(query))
                .ToListAsync();
        }
    }
}