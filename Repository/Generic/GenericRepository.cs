
using DBModel.DBModels;
using Microsoft.EntityFrameworkCore;
using UtilInterface;

namespace Repository
{
    public class GenericRepository<TEntity> :
        ICRUDRepositorio<TEntity>
        where TEntity : class
    {
        protected readonly _TiendaDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public GenericRepository(
            _TiendaDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public virtual async Task<List<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<TEntity?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<TEntity> CreateAsync(
            TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual Task<TEntity> UpdateAsync(
            TEntity entity)
        {
            _dbSet.Update(entity);
            return Task.FromResult(entity);
        }

        public virtual async Task DeleteAsync(object id)
        {
            var entity = await _dbSet.FindAsync(id);

            if (entity != null)
                _dbSet.Remove(entity);
        }

        public virtual async Task<List<TEntity>>
            InsertMultipleAsync(List<TEntity> lista)
        {
            await _dbSet.AddRangeAsync(lista);
            return lista;
        }

        public virtual Task<List<TEntity>>
            UpdateMultipleAsync(List<TEntity> lista)
        {
            _dbSet.UpdateRange(lista);
            return Task.FromResult(lista);
        }

        public virtual Task DeleteMultipleItemsAsync(
            List<TEntity> lista)
        {
            _dbSet.RemoveRange(lista);
            return Task.CompletedTask;
        }

        public Task<List<TEntity>> GetAutoCompleteAsync(string query)
        {
            throw new NotImplementedException();
        }
    }
}