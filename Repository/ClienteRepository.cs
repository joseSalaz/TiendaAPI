using DBModel.DBModels;
using IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class ClienteRepository : GenericRepository<Cliente>, IClienteRepository
    {
        public ClienteRepository(_TiendaDbContext context) : base(context)
        {
        }
        public IQueryable<Cliente> GetQueryable()
        {
            return _context.Clientes;
        }

        public async Task<List<Cliente>>
            GetAutoCompleteAsync(
                string query)
        {
            return await _dbSet
                .Where(x =>
                    x.Nombres.Contains(query))
                .ToListAsync();
        }
        public Cliente GetByTipoNroDocumento(string TipoDocumento, string NumeroDocumento)
        {
            if (string.IsNullOrEmpty(TipoDocumento) || string.IsNullOrEmpty(NumeroDocumento))
            {

                return new Cliente();
            }

            Cliente vPersona = new Cliente();
            //tipoDocumento ==> RUC | DNI

            int tDocumento = 0;

            switch (TipoDocumento.ToLower())
            {
                case "dni":
                    tDocumento = 1;
                    break;
                case "ruc":
                    tDocumento = 2;
                    break;
                default:
                    return vPersona;
            }
            vPersona = _context.Clientes
                .Where(x => x.TipoDocumento == TipoDocumento && x.NumeroDocumento == NumeroDocumento)
                .FirstOrDefault();

            return vPersona;
        }
        public async Task<Cliente?> ObtenerPorDocumentoAsync(string tipoDocumento,string numeroDocumento)
        {
            var tipo = tipoDocumento.Trim().ToUpper();
            var numero = numeroDocumento.Trim();

            return await _context.Clientes
                .FirstOrDefaultAsync(x =>
                    x.NumeroDocumento == numero &&
                    x.TipoDocumento.ToUpper() == tipo &&
                    x.Estado == "ACTIVO"
                );
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Clientes
                .AsNoTracking()
                .AnyAsync(x => x.Id == id && x.Estado == "ACTIVO");
        }
    }
}
