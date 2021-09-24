using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FishBook.DAL.Interfaces;

namespace FishBook.DAL.Repositories
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        private readonly FishBookContext _context;

        public BaseRepository(FishBookContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(TEntity item)
        {
            await _context.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task CreateAsync(List<TEntity> items)
        {
            await _context.AddRangeAsync(items);
            await _context.SaveChangesAsync();
        }

        public async Task EditAsync(TEntity item)
        {
            _context.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task EditAsync(List<TEntity> items)
        {
            _context.UpdateRange(items);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            var dbSet = _context.Set<TEntity>();
            var item = await dbSet.FindAsync(id);

            if (item == null)
                return false;
            else
            {
                _context.Entry(item).State = EntityState.Detached;
                return true;
            }
        }

        public async Task<IList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _context.Set<TEntity>()
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<TEntity> FindByIdAsync(int id)
        {
            TEntity item = await _context.Set<TEntity>()
                .FindAsync(id);

            return item == null ? null : item;
        }

        public async Task<TEntity> GetAsync(Guid id)
        {
            TEntity item = await _context.Set<TEntity>()
                .FindAsync(id);

            return item == null ? null : item;
        }

        public async Task RemoveAsync(int id)
        {
            TEntity item = await _context.Set<TEntity>()
                            .FindAsync(id);

            _context.Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(Guid id)
        {
            TEntity item = await _context.Set<TEntity>()
                            .FindAsync(id);

            await RemoveAsync(item);
        }

        public async Task RemoveAsync(TEntity item)
        {
            _context.Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _context.Set<TEntity>()
                .SingleOrDefaultAsync(predicate);
        }
    }
}
