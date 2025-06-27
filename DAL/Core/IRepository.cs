using Hydra.Core;
using Hydra.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DAL.Core
{
    public interface IRepository<T> where T : IBaseObject<T>
    {
        //DbContext? Context { get; set; }

        RepositoryResult Result { get; }

        string? GetContextConnectionString
        {
            get;
        }

        IQueryable<T> All(params string[] includes);

        Task<bool> AnyAsync(Expression<Func<T, bool>>? filter = null);

        Task<bool> CreateAsync(T entity);

        bool Contains(Expression<Func<T, bool>> predicate);

        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        Task<bool> DeleteAsync(T entity);

        Task<bool> DeleteRangeAsync(List<T> entities);

        Task<bool> DeleteRangeAsync(List<Guid> idList);

        IQueryable<T> FilterWithLinq(Expression<Func<T, bool>>? filter = null);

        Task<List<T>> FilterWithLinqAsync(Expression<Func<T, bool>>? filter = null);

        string[] GetAllIncludes();
        Task<T?> GetAsync(Expression<Func<T, bool>> filter, bool withAllIncludes = false, params string[] includes);

        Task<T?> GetByIdAsync(Guid id, bool withAllIncludes = false, params string[] includes);

        //T? GetEntityFromContext(T entity);

        Task<T?> GetUniqueAsync(T entity, bool withAllIncludes = false, params string[] includes);

        //Task<bool> IsItNewAsync(T entity);

        void ShowChangeTrackerEntriesStates();
        Expression<Func<T, bool>> UniqueFilter(T entity, bool forEntityFramework = true);


        Task<ResponseObjectForUpdate> UpdateAsync(T entity);

        Task<List<ResponseObjectForUpdate>> UpdateRangeAsync(List<T> entities)
    }
}
