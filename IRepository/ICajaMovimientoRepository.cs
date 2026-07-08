using DBModel.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilInterface;

namespace IRepository
{
    public interface ICajaMovimientoRepository : ICRUDRepositorio<CajaMovimiento>
    {
        Task<List<CajaMovimiento>> ObtenerMovimientosActivosPorSesionAsync(int cajaSesionId);
    }
}
