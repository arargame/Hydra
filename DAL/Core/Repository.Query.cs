using Hydra.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DAL.Core
{
    //QUERY
    public partial class Repository<T> : IRepository<T> where T : BaseObject<T>
    {

        public IQueryable<T> All(params string[] includes)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null && includes.Any())
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }

            return query;
        }
        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>>? filter = null)
        {
            return filter == null
                ? await _dbSet.AnyAsync()
                : await _dbSet.AnyAsync(filter);
        }

        public bool Contains(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }


        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            IQueryable<T> query = _dbSet;

            if (predicate != null)
                query = query.Where(predicate);

            return await query.CountAsync();
        }

        public IQueryable<T> FilterWithLinq(Expression<Func<T, bool>>? filter = null)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
                query = query.Where(filter);

            return query;
        }


        public virtual async Task<T?> GetAsync(Expression<Func<T, bool>> filter, bool withAllIncludes = false, params string[] includes)
        {
            try
            {
                IQueryable<T> query = _dbSet;

                includes = withAllIncludes ? GetAllIncludes() : includes;

                if (includes != null && includes.Any())
                {
                    foreach (var include in includes)
                        query = query.Include(include);
                }

                return await query.SingleOrDefaultAsync(filter);
            }
            catch
            {
                throw;
            }
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, bool withAllIncludes = false, params string[] includes)
        {
            return await GetAsync(e => e.Id == id, withAllIncludes, includes);
        }

        public virtual async Task<T?> GetUniqueAsync(T entity, bool withAllIncludes = false, params string[] includes)
        {
            return await GetAsync(UniqueFilter(entity), withAllIncludes, includes);
        }
    }
}
