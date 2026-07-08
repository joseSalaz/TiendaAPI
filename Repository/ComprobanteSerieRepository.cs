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
    public class ComprobanteSerieRepository : GenericRepository<ComprobanteSeries>, IComprobanteSerieRepository
    {
        public ComprobanteSerieRepository(_TiendaDbContext context) : base(context)
        {
        }
        public async Task<ComprobanteSeries?> GetSerieActivaAsync(
            int sucursalId,
            string tipoComprobante,
            string serie
        )
        {
            return await _context.ComprobanteSeries
                .FirstOrDefaultAsync(x =>
                    x.SucursalId == sucursalId &&
                    x.TipoComprobante == tipoComprobante &&
                    x.Serie == serie &&
                    x.Estado == "ACTIVO");
        }
    }
}
