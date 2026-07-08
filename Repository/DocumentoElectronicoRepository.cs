using DBModel.DBModels;
using IRepository;
using Microsoft.EntityFrameworkCore;
using Models.RequestResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilInterface;
using UtilPaginados.RequestResponse;

namespace Repository
{
    public class DocumentoElectronicoRepository : GenericRepository<DocumentosElectronico>, IDocumentoElectronicoRepository
    {
        public DocumentoElectronicoRepository(_TiendaDbContext context) : base(context)
        {
        }

        public async Task<DocumentosElectronico?> ObtenerPorVentaIdAsync(int ventaId)
        {
            return await _context.DocumentosElectronicos
                .FirstOrDefaultAsync(x => x.VentaId == ventaId);
        }

        public async Task<DocumentosElectronico?> ObtenerPorComprobanteAsync(string tipoDocumento, string serie, int correlativo)
        {
            return await _context.DocumentosElectronicos
                .FirstOrDefaultAsync(x =>
                    x.TipoDocumento == tipoDocumento &&
                    x.Serie == serie &&
                    x.Correlativo == correlativo);
        }

        public async Task<DocumentosElectronico?> ObtenerUltimoPorVentaAsync(int ventaId)
        {
            return await _context.DocumentosElectronicos
                .Where(x => x.VentaId == ventaId)
                .OrderByDescending(x => x.FechaCreacion)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<PaginacionResponse<DocumentosElectronico>> ListarFiltradoAsync(DocumentoElectronicoFiltroRequest filtro)
        {
            var query = _context.DocumentosElectronicos.AsQueryable();

            if (filtro.FechaInicio.HasValue)
            {
                var inicioUtc = ConvertirFechaPeruAUtc(filtro.FechaInicio.Value.Date);
                query = query.Where(x => x.FechaCreacion >= inicioUtc);
            }

            if (filtro.FechaFin.HasValue)
            {
                var finLocalExclusivo = filtro.FechaFin.Value.Date.AddDays(1);
                var finUtc = ConvertirFechaPeruAUtc(finLocalExclusivo);

                query = query.Where(x => x.FechaCreacion < finUtc);
            }

            if (!string.IsNullOrWhiteSpace(filtro.TipoDocumento))
                query = query.Where(x => x.TipoDocumento == filtro.TipoDocumento);

            if (!string.IsNullOrWhiteSpace(filtro.Estado))
                query = query.Where(x => x.Estado == filtro.Estado);

            if (!string.IsNullOrWhiteSpace(filtro.Serie))
                query = query.Where(x => x.Serie == filtro.Serie);

            if (filtro.Correlativo.HasValue)
                query = query.Where(x => x.Correlativo == filtro.Correlativo.Value);

            query = query
                .OrderByDescending(x => x.FechaCreacion)
                .ThenByDescending(x => x.Id);

            return await query.PaginarAsync(
                filtro.Pagina,
                filtro.Cantidad
            );
        }
        private static DateTime ConvertirFechaPeruAUtc(DateTime fechaLocal)
        {
            var fechaSinKind = DateTime.SpecifyKind(
                fechaLocal,
                DateTimeKind.Unspecified
            );

            var fechaPeru = new DateTimeOffset(
                fechaSinKind.Year,
                fechaSinKind.Month,
                fechaSinKind.Day,
                fechaSinKind.Hour,
                fechaSinKind.Minute,
                fechaSinKind.Second,
                TimeSpan.FromHours(-5)
            );

            return fechaPeru.UtcDateTime;
        }

        public async Task<List<DocumentosElectronico>> ObtenerPorVentaAsync(int ventaId)
        {
            return await _context.DocumentosElectronicos
                .Where(x => x.VentaId == ventaId)
                .OrderBy(x => x.FechaCreacion)
                .ThenBy(x => x.Id)
                .ToListAsync();
        }
        public async Task<List<DocumentosElectronico>> ObtenerPorVentasAsync(List<int> ventaIds)
        {
            if (ventaIds == null || !ventaIds.Any())
                return new List<DocumentosElectronico>();

            return await _context.DocumentosElectronicos
                .Where(x => ventaIds.Contains(x.VentaId))
                .OrderByDescending(x => x.FechaCreacion)
                .ThenByDescending(x => x.Id)
                .ToListAsync();
        }


    }
}

