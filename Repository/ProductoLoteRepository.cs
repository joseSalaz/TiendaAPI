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
    public class ProductoLoteRepository : GenericRepository<ProductoLote>, IProductoLoteRepository
    {
        public ProductoLoteRepository(_TiendaDbContext context) : base(context)
        {
        }

        public async Task<ProductoStock?> GetByProductoSucursalAsync(int productoId, int sucursalId)
        {
            return await _context.ProductoStocks
                .FirstOrDefaultAsync(x =>
                    x.ProductoId == productoId &&
                    x.SucursalId == sucursalId);
        }
        public async Task<List<ProductoLote>> GetLotesDisponiblesFefoAsync(int productoId, int sucursalId)
        {
            return await _context.ProductoLotes
                .Where(x =>
                    x.ProductoId == productoId &&
                    x.SucursalId == sucursalId &&
                    x.StockActual > 0 &&
                    x.Estado == "ACTIVO")
                .OrderBy(x => x.FechaVencimiento == null)
                .ThenBy(x => x.FechaVencimiento)
                .ThenBy(x => x.Id)
                .ToListAsync();
        }
    }
}
