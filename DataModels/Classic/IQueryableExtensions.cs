using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;

namespace Hydra.Core.Extensions
{

    public class SortDescriptor
    {
        public string Field { get; set; } = string.Empty;
        public SortDirection SortDirection { get; set; } = SortDirection.Asc;
    }

    public enum SortDirection
    {
        Asc,
        Desc
    }

    public class FilterDescriptor
    {
        public string Field { get; set; } = string.Empty;
        public string Operator { get; set; } = "equals"; // equals, contains, etc.
        public object? Value { get; set; }
    }


    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, List<SortDescriptor> sortDescriptors)
        {
            if (sortDescriptors == null || !sortDescriptors.Any()) return query;

            IOrderedQueryable<T>? orderedQuery = null;

            foreach (var sort in sortDescriptors)
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                var property = Expression.PropertyOrField(parameter, sort.Field);
                var lambda = Expression.Lambda(property, parameter);

                var methodName = sort.SortDirection == SortDirection.Desc
                    ? (orderedQuery == null ? "OrderByDescending" : "ThenByDescending")
                    : (orderedQuery == null ? "OrderBy" : "ThenBy");

                var method = typeof(Queryable).GetMethods()
                    .Where(m => m.Name == methodName && m.GetParameters().Length == 2)
                    .Single()
                    .MakeGenericMethod(typeof(T), property.Type);

                query = (IQueryable<T>)method.Invoke(null, new object[] { orderedQuery ?? query, lambda })!;
                orderedQuery = (IOrderedQueryable<T>)query;
            }

            return orderedQuery ?? query;
        }

        public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, List<FilterDescriptor> filters)
        {
            foreach (var filter in filters)
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                var property = Expression.PropertyOrField(parameter, filter.Field);

                var constant = Expression.Constant(Convert.ChangeType(filter.Value, property.Type));
                Expression? comparison = null;

                switch (filter.Operator.ToLower())
                {
                    case "equals":
                        comparison = Expression.Equal(property, constant);
                        break;
                    case "notequals":
                        comparison = Expression.NotEqual(property, constant);
                        break;
                    case "contains":
                        comparison = Expression.Call(property, "Contains", null, constant);
                        break;
                    case "greaterthan":
                        comparison = Expression.GreaterThan(property, constant);
                        break;
                    case "greaterthanorequal":
                        comparison = Expression.GreaterThanOrEqual(property, constant);
                        break;
                    case "lessthan":
                        comparison = Expression.LessThan(property, constant);
                        break;
                    case "lessthanorequal":
                        comparison = Expression.LessThanOrEqual(property, constant);
                        break;
                    case "isnull":
                        comparison = Expression.Equal(property, Expression.Constant(null));
                        break;
                    case "isnotnull":
                        comparison = Expression.NotEqual(property, Expression.Constant(null));
                        break;
                        // TODO: Between, In, NotIn, StartsWith, EndsWith vs. eklenebilir
                }

                if (comparison != null)
                {
                    var lambda = Expression.Lambda<Func<T, bool>>(comparison, parameter);
                    query = query.Where(lambda);
                }
            }

            return query;
        }
    }
}


