using Hydra.Core;
using Hydra.DataModels;
using Hydra.DataModels.Filter;
using Hydra.DTOs;
using Hydra.DTOs.ViewDTOs;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq.Expressions;

//using HydraTable = Hydra.DataModels.Table;

namespace Hydra.Services.Core
{

    //QUERY
    public partial class Service<T> : IService<T> where T : BaseObject<T>
    {
        public async Task<bool> AnyAsync(Expression<Func<T, bool>>? filter = null)
        {
            try
            {
                if (Repository == null)
                    throw new InvalidOperationException("Repository is not initialized.");

                return await Repository.AnyAsync(filter);
            }
            catch (Exception ex)
            {
                await LogService.SaveAsync(new Log(description:ex.Message,logType:LogType.Error,entityId:ex.Message,processType:LogProcessType.Read));

                return false;
            }
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            try
            {
                return await Repository.CountAsync(predicate);
            }
            catch (Exception ex)
            {
                await SaveErrorLogAsync("Count operation failed : " + ex.Message);

                return 0;
            }
        }

        public IQueryable<T> FilterWithLinq(Expression<Func<T, bool>>? filter = null)
        {
            return Repository.FilterWithLinq(filter);
        }


        public string[] GetAllIncludes()
        {
            return Repository.GetAllIncludes();
        }

        public virtual async Task<T?> GetAsync(Expression<Func<T, bool>> filter, bool withAllIncludes = false, params string[] includes)
        {
            try
            {
                return await Repository.GetAsync(filter, withAllIncludes, includes);
            }
            catch (Exception ex)
            {
                await SaveErrorLogAsync(description: $"GetAsync Exception: {ex.Message}", processType: LogProcessType.Read);
                return null;
            }
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> filter, params string[] includes)
        {
            return await GetAsync(filter, false, includes);
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, bool withAllIncludes = false, params string[] includes)
        {
            try
            {
                return await Repository.GetByIdAsync(id, withAllIncludes, includes);
            }
            catch (Exception ex)
            {
                await SaveErrorLogAsync(description: $"GetByIdAsync Exception: {ex.Message}", entityId: id, processType: LogProcessType.Read);
                return null;
            }
        }

        public Task<T?> GetByIdAsync(Guid id, params string[] includes)
        {
            return GetByIdAsync(id, false, includes);
        }

        public virtual async Task<T?> GetUniqueAsync(T entity, bool withAllIncludes = false, params string[] includes)
        {
            return await GetAsync(Repository.UniqueFilter(entity), withAllIncludes, includes);
        }

        public Task<T?> GetUniqueAsync(T entity, params string[] includes)
        {
            return GetUniqueAsync(entity, false, includes);
        }


        public Table GetTable(string? tableName = null, string? alias = null, int? pageSize = null, int? pageNumber = null)
        {
            var table = new Table(tableName ?? TypeName, alias);

            if (pageSize != null)
                table.SetPageSize(pageSize.Value);

            if (pageNumber != null)
                table.SetPageNumber(pageNumber.Value);

            return table;
        }

        public virtual async Task<bool> IsItNewAsync(T entity)
        {
            try
            {
                return await Repository.IsItNewAsync(entity);
            }
            catch (Exception ex)
            {
                await LogErrorAsync(
                    description: ex.Message,
                    entityId: entity?.Id,
                    processType: LogProcessType.Read
                );

                return false;
            }
        }


        public async Task<List<TResult>> SelectWithLinqAsync<TResult>(
                            Expression<Func<T, TResult>> selector,
                            Expression<Func<T, bool>>? filter = null,
                            int? countToSkip = null,
                            int? countToTake = null,
                            bool asNoTracking = true,
                            Func<IQueryable<T>, IQueryable<T>>? actionToOrder = null,
                            bool firstOrDefault = false,
                            bool selectDistinct = false,
                            params string[] includes)
        {
            try
            {
                IQueryable<T> query = FilterWithLinq(filter);

                if (includes != null && includes.Any())
                {
                    foreach (var include in includes)
                        query = query.Include(include);
                }

                if (actionToOrder != null)
                    query = actionToOrder(query);

                if (countToSkip.HasValue)
                    query = query.Skip(countToSkip.Value);

                if (countToTake.HasValue)
                    query = query.Take(countToTake.Value);

                if (asNoTracking)
                    query = query.AsNoTracking();

                var iQueryableForTResult = query.Select(selector);

                if (selectDistinct)
                    iQueryableForTResult = iQueryableForTResult.Distinct();

                var results = firstOrDefault
                                 ? new List<TResult> { await iQueryableForTResult.FirstOrDefaultAsync() }
                                 : await iQueryableForTResult.ToListAsync();
                return results;
            }
            catch (Exception ex)
            {
                await LogErrorAsync(
                    description: $"Selector: {selector?.Body}, Filter: {filter?.Body}, Includes: {string.Join(",", includes ?? Array.Empty<string>())}, Message:{ex.Message}",
                    processType: LogProcessType.Read);

                return new List<TResult>(); 
            }
        }



        public Task<List<T>> SelectWithLinqAsync(Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, T>>? selector = null,
            int? countToSkip = null,
            int? countToTake = null,
            bool asNoTracking = true,
            Func<IQueryable<T>, IQueryable<T>>? actionToOrder = null,
            bool firstOrDefault = false,
            bool selectDistinct = false,
            params string[] includes)
        {
            Expression<Func<T, T>> selectorExpression = t => t;

            selector = selector ?? selectorExpression;

            return SelectWithLinqAsync<T>(selector, filter, countToSkip, countToTake, asNoTracking, actionToOrder, firstOrDefault, selectDistinct, includes);
        }

      

        public List<T2> SelectWithTable<T2>(ref TableDTO tableDTO, ViewType? viewType = null, Type viewDTOTypeToPrepareUsingConfigurations = null, List<MetaColumnDTO> externalMetaColumns = null) where T2 : class
        {
            var table = tableDTO != null ? TableDTO.ConvertToTable(tableDTO) : GetTable(pageSize: 10, pageNumber: 1).SetViewType(viewType != null ? viewType.Value : ViewType.None);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (tableDTO == null)
            {
                tableDTO = TableDTO.FromTableToDTO(table);

                if (viewDTOTypeToPrepareUsingConfigurations != null)
                {
                    tableDTO.PrepareUsingConfigurations(viewDTOTypeToPrepareUsingConfigurations);
                }
            }

            sw.Stop();
            var sw1Elapsed = sw.Elapsed;

            if (externalMetaColumns != null)
            {
                foreach (var externalMetaColumn in externalMetaColumns)
                {
                    tableDTO.AlterOrAddMetaColumn(externalMetaColumn);
                }
            }

            if (viewType != null)
            {
                tableDTO.SetViewType(viewType.Value);
            }




            table = TableDTO.ConvertToTable(tableDTO);

            sw.Restart();

            var results = SelectWithTable<T2>(table);

            sw.Stop();
            var sw2Elapsed = sw.Elapsed;
            sw.Restart();

            tableDTO = TableDTO.FromTableToDTO(table)
                                .SetViewDTOTypeName(typeof(T2).Name);



            if (viewDTOTypeToPrepareUsingConfigurations != null)
            {
                tableDTO.PrepareUsingConfigurations(viewDTOTypeToPrepareUsingConfigurations);
            }

            sw.Stop();
            var sw3Elapsed = sw.Elapsed;

            return results;
        }

        public List<T> SelectWithTable(ITable table)
        {
            return SelectWithTable<T>(table);
        }


        public List<T2> SelectWithTable<T2>(ITable table) where T2 : class
        {
            return table.PrepareQueryStringToSelect()
                                .SetRows()
                                .Cast<T2>();
        }

        public async Task<List<T>> SelectThenCache(Expression<Func<T, bool>> filter)
        {
            var results = await SelectWithLinqAsync(filter: filter);

            if (HasCache)
            {
                foreach (var item in results)
                {
                    CacheService?.Add(item.Id, item);
                }
            }

            return results;
        }
    }
}
