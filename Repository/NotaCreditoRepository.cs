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
    public class NotaCreditoRepository : GenericRepository<NotasCredito>, INotaCreditoRepository
    {
        public NotaCreditoRepository(_TiendaDbContext context) : base(context)
        {
        }
        public async Task<NotasCredito?> ObtenerPorVentaIdAsync(int ventaId)
        {
            return await _context.NotasCreditos
                .FirstOrDefaultAsync(x => x.VentaId == ventaId && x.Estado == "ACTIVO");
        }
        public async Task<NotasCredito?> ObtenerPorComprobanteAsync(string tipoDocumento, string serie, int correlativo)
        {
            return await _context.NotasCreditos
                .FirstOrDefaultAsync(x =>
                    x.TipoDocumento == tipoDocumento &&
                    x.Serie == serie &&
                    x.Correlativo == correlativo);
        }
    }
}
