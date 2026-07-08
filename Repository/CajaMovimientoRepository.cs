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
    public class CajaMovimientoRepository : GenericRepository<CajaMovimiento>, ICajaMovimientoRepository
    {
        public CajaMovimientoRepository(_TiendaDbContext context) : base(context)
        {
        }

        public async Task<List<CajaMovimiento>> ObtenerMovimientosActivosPorSesionAsync(int cajaSesionId)
        {
            return await _context.CajaMovimientos
                .Where(x =>
                    x.CajaSesionId == cajaSesionId &&
                    x.Estado == "ACTIVO")
                .ToListAsync();
        }
    }
}
