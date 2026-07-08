using Models.RequestResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilInterface;

namespace IBussines
{
    public interface IProductoLoteBussines
    {
        Task<RegistrarEntradaStockResponse> RegistrarEntradaAsync(RegistrarEntradaStockRequest request);
    }
}
