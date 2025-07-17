using Hydra.Core;
using Hydra.DI;
using Hydra.Http;
using Hydra.AccessManagement;
using Hydra.Services;
using Hydra.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Hydra.DAL.Core
{
    public partial class Repository<T> : IRepository<T> where T : BaseObject<T>
    {
        private readonly DbContext? _context;

        public string? GetContextConnectionString
        {
            get
            {
                return _context?.Database.GetDbConnection().ConnectionString;
            }
        }

        private readonly DbSet<T> _dbSet;

        public RepositoryResult Result { get; } = new RepositoryResult();

        private readonly string TypeName = typeof(T).Name;

        public Repository(RepositoryInjector injector)
        {
            _context = injector.Context;

            _dbSet = injector.Context.Set<T>();
        }

        public void ChangeEntityState(T entity, EntityState entityState)
        {
            GetAsEntityEntry(entity).State = entityState;
        }

        public EntityEntry GetAsEntityEntry(T entity)
        {
            if (_context == null)
                throw new ArgumentNullException(nameof(_context));

            return _context.Entry(entity);
        }
        private IEnumerable<EntityEntry> GetContextChangeTrackerEntries()
        {
            if (_context == null)
                throw new ArgumentNullException(nameof(_context));

            return _context.ChangeTracker.Entries();
        }

        private T? GetEntityFromContext(T entity)
        {
            var entityInContext = GetContextChangeTrackerEntries()
                        .Select(entityEntry => entityEntry.Entity as T)
                        .Where(ee => ee != null)
                        .FirstOrDefault(UniqueFilter(entity).Compile());

            return entityInContext;
        }

        private async Task<T?> GetExistingEntityAsync(T entity, bool throwException = true, bool withAllIncludes = false)
        {
            var entityFromContext = GetEntityFromContext(entity);
            if (entityFromContext != null)
                return entityFromContext;

            var uniqueEntity = await GetUniqueAsync(entity, withAllIncludes);

            if (uniqueEntity == null && throwException)
                throw new Exception($"There is no such entity({typeof(T)}) in either Db or Context");

            return uniqueEntity;
        }

        public virtual string[] GetIncludes()
        {
            return new[] { "" };
        }

        public virtual string[] GetThenIncludes()
        {
            return new[] { "" };
        }

        public string[] GetAllIncludes()
        {
            var includes = GetIncludes();

            var thenIncludes = GetThenIncludes();

            return includes.Union(thenIncludes)
                            .Where(i => !string.IsNullOrWhiteSpace(i))
                            .ToArray();
        }


        private IEnumerable<string> GetModifiedProperties(T entity)
        {
            var ee = GetAsEntityEntry(entity);
            var dbValues = ee.GetDatabaseValues();

            if (dbValues == null)
                yield break; 

            var ByteArrayEquality = new Func<byte[], byte[], bool>((first, second) =>
            {
                if (first == null || second == null)
                    return false;

                if (first.Length != second.Length)
                    return false;

                for (int i = 0; i < first.Length; i++)
                {
                    if (first[i] != second[i])
                        return false;
                }

                return true;
            });

            var byteArrayProperties = entity.GetType()
                                            .GetProperties()
                                            .Where(p => p.PropertyType == typeof(byte[]));

            foreach (var propertyName in ee.CurrentValues.Properties.Select(p => p.Name))
            {
                if (new[] { nameof(IHasAuditFields.AddedDate), nameof(IHasAuditFields.ModifiedDate), nameof(IHasAuditFields.RowVersion) }.Contains(propertyName))
                    continue;

                if (byteArrayProperties.Any(p => p.Name == propertyName))
                {
                    var original = dbValues[propertyName] as byte[];
                    var current = ee.CurrentValues[propertyName] as byte[];

                    if (!ByteArrayEquality(original, current))
                        yield return propertyName;
                }
                else
                {
                    var original = dbValues[propertyName]?.ToString();
                    var current = ee.CurrentValues[propertyName]?.ToString();

                    if (original != current)
                        yield return propertyName;
                }
            }
        }




        public List<GroupObject<TKey>> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            if (_context == null)
                throw new ArgumentNullException(nameof(_context));

            var list = _context.Set<T>()
                .GroupBy(keySelector)
                .Select(g => new GroupObject<TKey>
                {
                    Key = g.Key,
                    Count = g.Count()
                })
                .ToList();

            return list;
        }

        private bool HasAnyModifiedProperty(T entity)
        {
            return GetModifiedProperties(entity).Any();
        }

        public virtual async Task<bool> IsItNewAsync(T entity)
        {
            try
            {
                var existing = await GetUniqueAsync(entity);

                return existing == null;
            }
            catch (Exception ex)
            {
                throw new Exception($"[Repository] IsItNew failed for {typeof(T).Name}: {ex.Message}");
            }
        }



        public IQueryable<T> FromSqlInterpolated(FormattableString sql, bool asNoTracking = true)
        {
            try
            {
                var query = _dbSet.FromSqlInterpolated(sql);

                return asNoTracking ? query.AsNoTracking() : query;
            }
            catch (Exception ex)
            {
                Result.Logs.Add(LogFactory.Error(description: $"SQL: {sql.Format}, Message: {ex.Message}",processType: LogProcessType.Unspecified));
                
                return Enumerable.Empty<T>().AsQueryable();
            }
        }



        public void ShowChangeTrackerEntriesStates()
        {
            Console.Write("\nShowChangeTrackerEntriesStates({0}) \n", typeof(T).Name);

            foreach (var e in GetContextChangeTrackerEntries())
            {
                var entityTypeName = e.Entity.GetType().Name.Contains('_') ? e.Entity.GetType().Name.Split('_')[0] : e.Entity.GetType().Name;

                Console.WriteLine("{0} : {1} ({2})", entityTypeName, e.State, ((IBaseObject<T>)e.Entity).Id);
            }
        }

        public virtual Expression<Func<T, bool>> UniqueFilter(T entity, bool forEntityFramework = true)
        {
            return t => t.Id == entity.Id;
        }
    }

    public class GroupObject<TKey>
    {
        public TKey Key { get; set; } = default!;
        public int Count { get; set; }
    }
}
