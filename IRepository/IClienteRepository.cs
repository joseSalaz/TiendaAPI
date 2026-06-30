using DBModel.DBModels;
using Microsoft.EntityFrameworkCore.Design.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilInterface;

namespace IRepository
{
    public interface IClienteRepository : ICRUDRepositorio<Cliente>
    {
        Task<List<Cliente>> GetAutoCompleteAsync(string query);
        IQueryable<Cliente> GetQueryable();
        Cliente GetByTipoNroDocumento(string TipoDocumento, string NumeroDocumento);
        Task<Cliente?> ObtenerPorDocumentoAsync(string tipoDocumento, string numeroDocumento);
        Task<bool> ExisteAsync(int id);
    }

}
