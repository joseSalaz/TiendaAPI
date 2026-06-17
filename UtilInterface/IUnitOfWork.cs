using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilInterface
{
    public interface IUnitOfWork : IDisposable
    {
        Task BeginTransactionAsync();

        Task<int> SaveChangesAsync();

        Task CommitAsync();

        Task RollbackAsync();
    }
}
