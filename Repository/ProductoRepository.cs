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
        public IQueryable<Producto> GetQueryable()
        {
            return _context.Productos;
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
        public async Task<Producto?> GetByCodigoBarras(string codigoBarras)
        {
            return await _context.Productos
                .FirstOrDefaultAsync(p => p.CodigoBarras == codigoBarras);
        }
    }
}