using Models.RequestResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilInterface;
using UtilPaginados.RequestResponse;

namespace IBussines
{
    public interface IProductoPresentacionBussines : ICRUDBussnies<ProductoPresentacionRequest, ProductoPresentacionResponse>
    {
        Task<PaginacionResponse<ProductoPresentacionResponse>> GetAllProductosPresentacionPaged(int pagina, int cantidad);
    }
}
