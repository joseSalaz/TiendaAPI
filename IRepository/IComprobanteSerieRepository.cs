using DBModel.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilInterface;

namespace IRepository
{
    public interface IComprobanteSerieRepository : ICRUDRepositorio<ComprobanteSeries>
    {
        Task<ComprobanteSeries?> GetSerieActivaAsync(int sucursalId, string tipoComprobante, string serie);
    }
}
