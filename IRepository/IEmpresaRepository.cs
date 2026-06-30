using DBModel.DBModels;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilInterface;

namespace IRepository
{
    public interface IEmpresaRepository : ICRUDRepositorio<Empresa>
    {
        Task<Empresa?> ObtenerEmpresaPorSucursalAsync(int sucursalId);
    }
}
