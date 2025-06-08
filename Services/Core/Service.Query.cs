using Hydra.Core;
using Hydra.DataModels;
using Hydra.DTOs;
using Hydra.DTOs.ViewDTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

//using HydraTable = Hydra.DataModels.Table;

namespace Hydra.Services.Core
{

    //QUERY
    public partial class Service<T> : IService<T> where T : BaseObject<T>
    {
        public bool Any(Expression<Func<T, bool>>? filter = null)
        {
            return Repository.Any(filter);
        }

        public int Count(Expression<Func<T, bool>>? expression = null)
        {
            return Repository.Count();
        }

        public IQueryable<T> FilterWithLinq(Expression<Func<T, bool>>? filter = null)
        {
            return Repository.FilterWithLinq(filter);
        }

        public T Get(Expression<Func<T, bool>> filter, bool withAllIncludes = false, params string[] includes)
        {
            T entity = Repository.Get(filter, withAllIncludes: withAllIncludes, includes: includes);

            return entity;
        }

        public T Get(Expression<Func<T, bool>> filter, params string[] includes)
        {
            return Get(filter, false, includes);
        }

        public T GetById(Guid id, bool withAllIncludes = false, params string[] includes)
        {
            T entity = Repository.GetById(id, withAllIncludes, includes);

            return entity;
        }

        public T GetById(Guid id, params string[] includes)
        {
            return GetById(id, false, includes);
        }


        public string[] GetAllIncludes()
        {
            return Repository.GetAllIncludes();
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

        public T GetUnique(T entity, bool withAllIncludes = false, params string[] includes)
        {
            return Get(Repository.UniqueFilter(entity), withAllIncludes, includes);
        }

        public T GetUnique(T entity, params string[] includes)
        {
            return GetUnique(entity, false, includes);
        }

        public virtual bool IsItNew(T entity)
        {
            return Repository.IsItNew(entity);
        }

        public List<T> SelectWithLinq(Expression<Func<T, bool>> filter = null,
        Expression<Func<T, T>> selector = null,
        int? countToSkip = null,
        int? countToTake = null,
        bool asNoTracking = true,
        Func<IQueryable<T>, IQueryable<T>> actionToOrder = null,
        bool firstOrDefault = false,
        bool selectDistinct = false,
        params string[] includes)
        {
            Func<T, T> func = new Func<T, T>((T t) =>
            {
                return t;
            });

            Expression<Func<T, T>> selectorExpression = t => func(t);

            selector = selector ?? selectorExpression;

            return SelectWithLinq<T>(selector, filter, countToSkip, countToTake, asNoTracking, actionToOrder, firstOrDefault, selectDistinct, includes);
        }

        public List<TResult> SelectWithLinq<TResult>(Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>> filter = null,
            int? countToSkip = null,
            int? countToTake = null,
            bool asNoTracking = true,
            Func<IQueryable<T>, IQueryable<T>> actionToOrder = null,
            bool firstOrDefault = false,
            bool selectDistinct = false,
            params string[] includes)
        {
            var iQueryable = FilterWithLinq(filter);

            if (includes.Any())
            {
                foreach (var include in includes)
                {
                    iQueryable = iQueryable.Include(include);
                }
            }
            //iQueryable = iQueryable.Include(string.Join(",", includes));

            if (countToSkip != null)
                iQueryable = iQueryable.Skip(countToSkip.Value);

            if (countToTake != null)
                iQueryable = iQueryable.Take(countToTake.Value);

            if (asNoTracking)
                iQueryable = iQueryable.AsNoTracking();

            //if (firstOrDefault)
            //    iQueryable = iQueryable.Skip(1).Take(1);

            if (actionToOrder != null)
                iQueryable = actionToOrder(iQueryable);

            var iQueryableForTResult = iQueryable.Select(selector);

            if (selectDistinct)
                iQueryableForTResult = iQueryableForTResult.Distinct();

            var result = firstOrDefault ? new List<TResult>() { iQueryableForTResult.FirstOrDefault() } : iQueryableForTResult.ToList();

            return result;
        }

        public List<T> SelectWithTable(string tableName = null,
                                string tableAlias = null,
                        List<IMetaColumn> metaColumns = null,
                        Expression<Func<Table, JoinFilter>> expressionToManageFilters = null,
                        Expression<Func<Table, List<JoinTable>>> expressionToSetJoins = null,
                        int? pageNumber = null,
                        int? pageSize = null)
        {
            tableName = string.IsNullOrEmpty(tableName) ? TypeName : tableName;

            var table = Table.Create(tableName, tableAlias, metaColumns, expressionToManageFilters, expressionToSetJoins, pageNumber, pageSize);

            return SelectWithTable<T>(table);
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

        public List<T> SelectWithTable(Table table)
        {
            return SelectWithTable<T>(table);
        }


        public List<T2> SelectWithTable<T2>(Table table) where T2 : class
        {
            return table.PrepareQueryStringToSelect()
                                .SetRows()
                                .Cast<T2>();
        }

        public List<T> SelectThenCache(Expression<Func<T, bool>> filter)
        {
            var results = SelectWithLinq(filter: filter);

            results.ForEach(r => Cache<T>.AddObject(r));

            return results;
        }
    }
}
