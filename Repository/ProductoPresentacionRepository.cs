using DBModel.DBModels;
using IRepository;
using Microsoft.EntityFrameworkCore;
using Models.RequestResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilInterface;

namespace Repository
{
    public class ProductoPresentacionRepository : GenericRepository<ProductoPresentacione>, IProductoPresentacionRepository
    {
        public ProductoPresentacionRepository(_TiendaDbContext context) : base(context)
        {
        }
        public IQueryable<ProductoPresentacione> GetQueryable()
        {
            return _context.ProductoPresentaciones;
        }

        public async Task<List<ProductoPresentacione>>
            GetAutoCompleteAsync(
                string query)
        {
            return await _dbSet
                .Where(x =>
                    x.Nombre.Contains(query))
                .ToListAsync();
        }

        public async Task CreateRangeAsync(List<ProductoPresentacione> presentaciones)
        {
            await _context.ProductoPresentaciones.AddRangeAsync(presentaciones);
        }

        public async Task<bool> ExistsCodigoBarrasAsync(string codigoBarras)
        {
            return await _context.ProductoPresentaciones
                .AnyAsync(p => p.CodigoBarras == codigoBarras);
        }

        public async Task<List<ProductoPresentacione>> GetByProductoIdAsync(int productoId)
        {
            return await _context.ProductoPresentaciones
                .AsNoTracking()
                .Where(p => p.ProductoId == productoId)
                .ToListAsync();
        }

        public async Task<ProductoPresentacione?> GetByIdAndProductoAsync(int id, int productoId)
        {
            return await _context.ProductoPresentaciones
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.ProductoId == productoId &&
                    x.Estado == "ACTIVO");
        }
    }
}
