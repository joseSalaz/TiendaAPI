using DBModel.DBModels;
using Models.RequestResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilInterface;
using UtilPaginados.RequestResponse;

namespace IRepository
{
    public interface IVentaRepository : ICRUDRepositorio<Venta>
    {
        Task<PaginacionResponse<Venta>> ListarFiltradoAsync(VentaFiltroRequest filtro);
    }
}
