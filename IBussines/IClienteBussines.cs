using DBModel.DBModels;
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
    public interface IClienteBussines : ICRUDBussnies<ClienteRequest, ClienteResponse>
    {
        Task<PaginacionResponse<ClienteResponse>> GetAllClientesPaged(int pagina, int cantidad);

        Task<Cliente> GetByTipoNroDocumento(string TipoDocumento, string NumeroDocumento);
    }
}
