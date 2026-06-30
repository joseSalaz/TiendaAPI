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
    public class SucursalRepository : GenericRepository<Sucursale>, ISucursalRepository
    {
        public SucursalRepository(_TiendaDbContext context) : base(context)
        {
        }
        public async Task<List<Sucursale>> GetActivasAsync()
        {
            return await _context.Sucursales
                .AsNoTracking()
                .Where(s => s.Estado == "ACTIVO")
                .ToListAsync();
        }
    }
}
