using Hydra.Core;
using Hydra.DAL;
using Hydra.DataModels;
using Hydra.DataModels.Filter;
using Hydra.DI;
using Hydra.DTOs;
using Hydra.DTOs.ViewDTOs;
using Hydra.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Hydra.DataModels.SortingFilterDirectionExtension;

namespace Hydra.Services.Core
{
    public interface IService<T> where T : BaseObject<T>
    {
        bool HasCache { get; set; }

        IRepository<T>? Repository { get; set; }

        bool Any(Expression<Func<T, bool>>? filter = null);

        bool Commit();

        bool Create(T entity);

        Task<bool> CreateOrUpdate(T entity, Expression<Func<T, bool>>? expression = null);

        int Count(Expression<Func<T, bool>>? expression = null);

        bool Delete(Expression<Func<T, bool>> filter);

        bool Delete(T entity);

        bool Delete(Guid id);

        bool DeleteRange(List<T> entities);

        bool DeleteRange(List<Guid> idList);

        //IQueryable<T> FilterWithDynamicLinq(IFilter filter);

        //IQueryable<T> FilterWithDynamicLinq(string filter, object[] parameters);

        IQueryable<T> FilterWithLinq(Expression<Func<T, bool>>? filter = null);

        T Get(Expression<Func<T, bool>> filter, bool withAllIncludes = false, params string[] includes);

        T Get(Expression<Func<T, bool>> filter, params string[] includes);

        T GetById(Guid id, bool withAllIncludes = false, params string[] includes);

        T GetById(Guid id, params string[] includes);

        string[] GetAllIncludes();

        Table GetTable(string? tableName = null, string? alias = null, int? pageSize = null, int? pageNumber = null);

        T GetUnique(T entity, bool withAllIncludes = false, params string[] includes);

        T GetUnique(T entity, params string[] includes);

        bool IsItNew(T entity);

        //List<T> SelectWithDynamicLinq(IFilter? filter = null,
        //    int? countToSkip = null,
        //    int? countToTake = null,
        //    List<SelectedColumn>? selectedColumns = null,
        //    List<OrderedColumn>? orderedColumns = null,
        //    bool asNoTracking = true,
        //    bool firstOrDefault = false,
        //    bool selectDistinct = false,
        //    params string[] includes);

        List<T> SelectWithLinq(Expression<Func<T, bool>>? filter = null,
                Expression<Func<T, T>>? selector = null,
                int? countToSkip = null,
                int? countToTake = null,
                bool asNoTracking = true,
                Func<IQueryable<T>, IQueryable<T>>? actionToOrder = null,
                bool firstOrDefault = false,
                bool selectDistinct = false,
                params string[] includes);

        List<TResult> SelectWithLinq<TResult>(Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>>? filter = null,
            int? countToSkip = null,
            int? countToTake = null,
            bool asNoTracking = true,
            Func<IQueryable<T>, IQueryable<T>>? actionToOrder = null,
            bool firstOrDefault = false,
            bool selectDistinct = false,
            params string[] includes);

        List<T> SelectWithTable(string? tableName = null,
                                string? tableAlias = null,
                        List<IMetaColumn> metaColumns = null,
                        Expression<Func<Table, JoinFilter>>? expressionToManageFilters = null,
                        Expression<Func<Table, List<JoinTable>>>? expressionToSetJoins = null,
                        int? pageNumber = null,
                        int? pageSize = null);

        List<T2> SelectWithTable<T2>(ref TableDTO tableDTO, ViewType? viewType = null, Type viewDTOTypeToPrepareUsingConfigurations = null, List<MetaColumnDTO> externalMetaColumns = null) where T2 : class;

        List<T> SelectWithTable(Table table);

        List<T2> SelectWithTable<T2>(Table table) where T2 : class;

        List<T> SelectThenCache(Expression<Func<T, bool>> filter);

        ResponseObjectForUpdate Update(T entity);

        T PrepareSample();

        ServiceInjector GetInjector();

        Service<T> DisableToCommit();

        Service<T> EnableToCommit();

        Service<T> AddExtraMessagesToResponseWhenItIsFailed(ResponseObject responseObject);
    }
}
