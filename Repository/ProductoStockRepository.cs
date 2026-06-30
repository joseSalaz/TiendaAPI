using DBModel.DBModels;
using IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class ProductoStockRepository : GenericRepository<ProductoStock>, IProductoStockRepository
    {
        public ProductoStockRepository(_TiendaDbContext context) : base(context)
        {
        }
        public async Task CreateRangeAsync(List<ProductoStock> stocks)
        {
            await _context.ProductoStocks.AddRangeAsync(stocks);
        }

        public async Task<bool> ExistsAsync(int productoId, int sucursalId)
        {
            return await _context.ProductoStocks
                .AnyAsync(s => s.ProductoId == productoId && s.SucursalId == sucursalId);
        }

        public async Task<ProductoStock?> GetByProductoSucursalAsync(int productoId, int sucursalId)
        {
            return await _context.ProductoStocks
                .FirstOrDefaultAsync(x =>
                    x.ProductoId == productoId &&
                    x.SucursalId == sucursalId);
        }
    }
}
