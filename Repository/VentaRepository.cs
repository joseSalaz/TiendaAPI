using DBModel.DBModels;
using IRepository;
using Models.RequestResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilPaginados.RequestResponse;

namespace Repository
{
    public class VentaRepository : GenericRepository<Venta>, IVentaRepository
    {
        public VentaRepository(_TiendaDbContext context) : base(context)
        {
        }
        public async Task<PaginacionResponse<Venta>> ListarFiltradoAsync(VentaFiltroRequest filtro)
        {
            var query = _context.Ventas.AsQueryable();

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

            if (filtro.SucursalId.HasValue)
                query = query.Where(x => x.SucursalId == filtro.SucursalId.Value);

            if (filtro.CajaId.HasValue)
                query = query.Where(x => x.CajaId == filtro.CajaId.Value);

            if (filtro.ClienteId.HasValue)
                query = query.Where(x => x.ClienteId == filtro.ClienteId.Value);

            if (!string.IsNullOrWhiteSpace(filtro.TipoDocumento))
                query = query.Where(x => x.TipoDocumento == filtro.TipoDocumento);

            if (!string.IsNullOrWhiteSpace(filtro.EstadoSunat))
                query = query.Where(x => x.EstadoSunat == filtro.EstadoSunat);

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
    }
}
