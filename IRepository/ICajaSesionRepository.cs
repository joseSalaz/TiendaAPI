using DBModel.DBModels;
using UtilInterface;

namespace IRepository
{
    public interface ICajaSesionRepository : ICRUDRepositorio<CajaSesione>
    {
        Task<List<CajaSesione>>
            GetAutoCompleteAsync(
                string query);
        IQueryable<CajaSesione> GetQueryable();

        /// <summary>Obtiene la sesión abierta activa de una caja (estado = ABIERTA).</summary>
        Task<CajaSesione?> ObtenerSesionAbiertaPorCajaAsync(int cajaId);

        Task<CajaSesione?> GetSesionAbiertaPorCajaAsync(int cajaId);
        Task<CajaSesione> AbrirCajaAsync(CajaSesione cajaSesion);

        Task<CajaSesione> CerrarCajaAsync(CajaSesione cajaSesion);


        Task<CajaSesione?> ObtenerPorIdAsync(int id);
    }
}
