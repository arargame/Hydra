using Hydra.Core;
using Hydra.DataModels;
using Hydra.DataModels.Filter;
using Hydra.DI;
using Hydra.DTOs;
using Hydra.DTOs.ViewDTOs;
using Hydra.Http;
using Hydra.AccessManagement;
using Hydra.Services.Cache;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Hydra.Services.Core
{
    public interface IService<T> where T : BaseObject<T>
    {

        //SessionInformation SessionInformation {  get; }

        ICacheService<Guid, T>? CacheService { get; set; }
        bool HasCache { get;}

        //ILogService? LogService { get; }

        //IRepository<T>? Repository { get; set; }

        //IService<T> AddExtraMessagesToResponseWhenItIsFailed(ResponseObject responseObject);
        Task<bool> AnyAsync(Expression<Func<T, bool>>? filter = null);

        Task<bool> CommitAsync();

        Task<IResponseObject> CreateAsync(T entity);

        Task<IResponseObject> CreateOrUpdateAsync(T entity, Expression<Func<T, bool>>? expression = null);

        Task<int> CountAsync(Expression<Func<T, bool>>? expression = null);

        IService<T> DisableToCommit();

        Task<IResponseObject> DeleteAsync(Expression<Func<T, bool>> filter);

        Task<IResponseObject> DeleteAsync(T entity);

        Task<IResponseObject> DeleteAsync(Guid id);

        Task<IResponseObject> DeleteRangeAsync(List<T> entities);

        Task<IResponseObject> DeleteRangeAsync(List<Guid> idList);

        IService<T> EnableToCommit();

        IQueryable<T> FilterWithLinq(Expression<Func<T, bool>>? filter = null);

        string[] GetAllIncludes();


        Task<T?> GetAsync(Expression<Func<T, bool>> filter, bool withAllIncludes = false, params string[] includes);
        Task<T?> GetAsync(Expression<Func<T, bool>> filter, params string[] includes);
        Task<T?> GetByIdAsync(Guid id, bool withAllIncludes = false, params string[] includes);
        Task<T?> GetByIdAsync(Guid id, params string[] includes);

        Task<(TableDTO FinalDTO, TResult? Result)> GetDetailsAsync<TResult>(Guid id) where TResult : class;

        List<ResponseObjectMessage> GetRepositoryMessages();

        Table GetTable(string? tableName = null, string? alias = null, int? pageSize = null, int? pageNumber = null);

        Task<T?> GetUniqueAsync(T entity, bool withAllIncludes = false, params string[] includes);

        Task<T?> GetUniqueAsync(T entity, params string[] includes);

        Task<bool> IsItNewAsync(T entity);


        Task<List<T>> SelectWithLinqAsync(Expression<Func<T, bool>>? filter = null,
                                Expression<Func<T, T>>? selector = null,
                                int? countToSkip = null,
                                int? countToTake = null,
                                bool asNoTracking = true,
                                Func<IQueryable<T>, IQueryable<T>>? actionToOrder = null,
                                bool firstOrDefault = false,
                                bool selectDistinct = false,
                                params string[] includes);

        Task<List<TResult>> SelectWithLinqAsync<TResult>(Expression<Func<T, TResult>> selector,
                                                Expression<Func<T, bool>>? filter = null,
                                                int? countToSkip = null,
                                                int? countToTake = null,
                                                bool asNoTracking = true,
                                                Func<IQueryable<T>, IQueryable<T>>? actionToOrder = null,
                                                bool firstOrDefault = false,
                                                bool selectDistinct = false,
                                                params string[] includes);


        Task<List<T>> SelectWithTableAsync(string? tableName = null,
                        string? tableAlias = null,
                        List<IMetaColumn>? metaColumns = null,
                        Expression<Func<ITable, IJoinFilter>>? expressionToManageFilters = null,
                        Expression<Func<ITable, List<IJoinTable>>>? expressionToSetJoins = null,
                        int? pageNumber = null,
                        int? pageSize = null);

        Task<(TableDTO TableDTO, List<TResult> Results)> SelectWithTableAsync<TResult>(
                TableDTO? tableDTO = null,
                ViewType? viewType = null,
                Type? viewDTOTypeToPrepareUsingConfigurations = null,
                List<MetaColumnDTO>? externalMetaColumns = null
            ) where TResult : class;

        
        Task<List<T>> SelectWithTableAsync(ITable table);

        Task<List<TResult>> SelectWithTableAsync<TResult>(ITable table) where TResult : class;

        Task<List<T>> SelectThenCache(Expression<Func<T, bool>> filter);

        void SetCacheService(ICacheService<Guid, T> service);

        //ServiceInjector GetInjector();

        Task<IResponseObject> UpdateAsync(T entity);

        Task<ResponseObjectForBulkUpdate> UpdateBulkAsync(List<T> entities);
    }
}
