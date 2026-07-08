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
    public class VentaDetalleLoteRepository : GenericRepository<VentaDetalleLote>, IVentaDetalleLoteRepository
    {
        public VentaDetalleLoteRepository(_TiendaDbContext context) : base(context)
        {
        }
        public async Task<List<VentaDetalleLote>> ObtenerPorVentaDetalleIdAsync(int ventaDetalleId)
        {
            return await _context.VentaDetalleLotes
                .Where(x => x.VentaDetalleId == ventaDetalleId)
                .ToListAsync();
        }
    }
}
