using DBModel.Models;
using IRepository;
using Microsoft.EntityFrameworkCore;
using UtilInterface;

namespace Repository
{
    public class CajaSesionRepository :
        GenericRepository<CajaSesione>,
        ICajaSesionRepository
    {
        public CajaSesionRepository(
            _TiendaDbContext context)
            : base(context)
        {
        }

        public async Task<CajaSesione?> ObtenerSesionAbiertaPorCajaAsync(int cajaId)
        {
            return await _context.CajaSesiones
                .FirstOrDefaultAsync(cs =>
                    cs.CajaId == cajaId &&
                    cs.Estado == "ABIERTA");
        }

        public async Task<CajaSesione> AbrirCajaAsync(CajaSesione    cajaSesion)
        {
            cajaSesion.FechaApertura = DateTime.UtcNow;
            cajaSesion.Estado = "ABIERTA";
            _context.CajaSesiones.Add(cajaSesion);
            await _context.SaveChangesAsync();
            return cajaSesion;
        }

        public async Task<CajaSesione> CerrarCajaAsync(CajaSesione cajaSesion)
        {
            cajaSesion.FechaCierre = DateTime.UtcNow;
            cajaSesion.Estado = "CERRADA";
            cajaSesion.Diferencia = cajaSesion.MontoCierre - cajaSesion.MontoInicial;
            _context.CajaSesiones.Update(cajaSesion);
            await _context.SaveChangesAsync();
            return cajaSesion;
        }

        public async Task<CajaSesione?> ObtenerPorIdAsync(int id)
        {
            return await _context.CajaSesiones.FindAsync(id);
        }

        public IQueryable<CajaSesione> GetQueryable()
        {
            return _context.CajaSesiones;
        }
        #region
        
        #endregion


    }
}