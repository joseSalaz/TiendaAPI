using DBModel.DBModels;
using IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class MedioPagoRepository : GenericRepository<MediosPago>, IMedioPagoRepository
    {
        public MedioPagoRepository(_TiendaDbContext context) : base(context)
        {
        }
    }
}
