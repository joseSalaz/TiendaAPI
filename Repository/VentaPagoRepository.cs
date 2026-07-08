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
    public class VentaPagoRepository : GenericRepository<VentaPago>, IVentaPagoRepository
    {
        public VentaPagoRepository(_TiendaDbContext context) : base(context)
        {
        }
        public async Task<List<VentaPago>> ObtenerPorVentaIdAsync(int ventaId)
        {
            return await _context.VentaPagos
                .Include(x => x.MedioPago)
                .Where(x => x.VentaId == ventaId)
                .ToListAsync();
        }
    }
}
