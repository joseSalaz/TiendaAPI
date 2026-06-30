using DBModel.DBModels;
using IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class StockMovimientoRepository : GenericRepository<StockMovimiento>, IStockMovimientoRepository
    {
        public StockMovimientoRepository(_TiendaDbContext context) : base(context)
        {
        }
    }
}
