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
    public class EmpresaRepository : GenericRepository<Empresa>, IEmpresaRepository
    {
        public EmpresaRepository(_TiendaDbContext context) : base(context)
        {
        }
        public async Task<Empresa?> ObtenerEmpresaPorSucursalAsync(int sucursalId)
        {
            return await (
                from s in _context.Sucursales
                join e in _context.Empresas on s.EmpresaId equals e.Id
                where s.Id == sucursalId
                      && e.Estado == "ACTIVO"
                select e
            ).FirstOrDefaultAsync();
        }
    }
}
