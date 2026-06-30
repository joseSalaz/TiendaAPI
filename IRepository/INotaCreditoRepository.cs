using DBModel.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilInterface;

namespace IRepository
{
    public interface INotaCreditoRepository : ICRUDRepositorio<NotasCredito>
    {
        Task<NotasCredito?> ObtenerPorVentaIdAsync(int ventaId);
        Task<NotasCredito?> ObtenerPorComprobanteAsync(string tipoDocumento, string serie, int correlativo);
    }
}
