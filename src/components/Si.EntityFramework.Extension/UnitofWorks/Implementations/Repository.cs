using Microsoft.EntityFrameworkCore;
using Si.EntityFramework.Extension.Data.Context;
using Si.EntityFramework.Extension.UnitofWorks.Abstractions;
using System.Linq.Expressions;

namespace Si.EntityFramework.Extension.UnitofWorks.Implementations
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly DbSet<T> DbSet;
        public Repository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            DbSet = _dbContext.Set<T>();
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await DbSet.ToListAsync();
        }
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await DbSet.Where(predicate).ToListAsync();
        }
        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await DbSet.SingleOrDefaultAsync(predicate);
        }
        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await DbSet.FirstOrDefaultAsync(predicate);
        }
        public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageIndex = 1,
            int pageSize = 20,
            Expression<Func<T, bool>> predicate = null,
            Expression<Func<T, object>> orderBy = null,
            bool ascending = true)
        {
            IQueryable<T> query = DbSet;

            if(pageIndex<=0 || pageSize <= 0)
            {
                throw new ArgumentException("Page index and page size must be greater than zero.");
            }
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            int totalCount = await query.CountAsync();

            if (orderBy != null)
            {
                query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            }

            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, totalCount);
        }

        public async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await DbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            await DbSet.AddRangeAsync(entities);
        }
        public async Task DeleteAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                DbSet.Remove(entity);
            }
        }
        public async Task DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            var entities = await DbSet.Where(predicate).ToListAsync();
            DbSet.RemoveRange(entities);
        }
        public Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            DbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await DbSet.AnyAsync(predicate);
        }
        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
                return await DbSet.CountAsync();

            return await DbSet.CountAsync(predicate);
        }
        public async Task<T> GetByIdAsync(int id)
        {
            return await DbSet.FindAsync(id);
        }
        public Task UpdateAsync(T entity)
        {
            DbSet.Update(entity);
            return Task.CompletedTask;
        }
        public Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            DbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }
        public IQueryable<T> Query()
        {
            return DbSet;
        }
        public IQueryable<T> QueryNoTracking()
        {
            return DbSet.AsNoTracking();
        }
        public IQueryable<T> Where(Expression<Func<T, bool>> predicate)
        {
            return DbSet.Where(predicate);
        }
    }
}
