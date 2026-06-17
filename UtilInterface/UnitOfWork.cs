using DBModel.Models;
using Microsoft.EntityFrameworkCore.Storage;
using UtilInterface;

namespace Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly _TiendaDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(
            _TiendaDbContext context)
        {
            _context = context;
        }

        public async Task BeginTransactionAsync()
        {
            _transaction =
                await _context.Database
                    .BeginTransactionAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
        }
    }
}