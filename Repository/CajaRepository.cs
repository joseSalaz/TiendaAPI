using DBModel.DBModels;
using IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class CajaRepository : GenericRepository<Caja>, ICajaRepository
    {
        public CajaRepository(_TiendaDbContext context) : base(context)
        {
        }
    }
}
