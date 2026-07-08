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
    public class VentaDetalleRepository : GenericRepository<VentaDetalle>, IVentaDetalleRepository
    {
        public VentaDetalleRepository(_TiendaDbContext context) : base(context)
        {
        }
        public async Task<List<VentaDetalle>> ObtenerPorVentaIdAsync(int ventaId)
        {
            return await _context.VentaDetalles
                .Where(x => x.VentaId == ventaId)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        public async Task<List<VentaDetalleLote>> ObtenerPorVentaDetalleIdAsync(int ventaDetalleId)
        {
            return await _context.VentaDetalleLotes
                .Where(x => x.VentaDetalleId == ventaDetalleId)
                .ToListAsync();
        }
    }
}
