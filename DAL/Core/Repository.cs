using Hydra.Core;
using Hydra.DI;
using Hydra.Http;
using Hydra.IdentityAndAccess;
using Hydra.Services;
using Hydra.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Hydra.DAL.Core
{
    public interface IRepository<T> where T : IBaseObject<T>
    {
        //DbContext? Context { get; set; }

        string? GetContextConnectionString
        {
            get;
        }

        List<Log> Logs { get; set; }

        List<ResponseObjectMessage> Messages { get; set; }

        IQueryable<T> All(params string[] includes);

        Task<bool> AnyAsync(Expression<Func<T, bool>>? filter = null);

        Task<bool> CreateAsync(T entity);

        bool Contains(Expression<Func<T, bool>> predicate);

        List<Log> ConsumeLogs();

        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        Task<bool> DeleteAsync(T entity);

        Task<bool> DeleteRangeAsync(List<T> entities);

        Task<bool> DeleteRangeAsync(List<Guid> idList);

        IQueryable<T> FilterWithLinq(Expression<Func<T, bool>>? filter = null);

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

    public partial class Repository<T> : IRepository<T> where T : BaseObject<T>
    {
        private readonly ILogService LogService;

        private readonly DbContext? _context;

        //public DbContext? Context { get { return _context; } }

        public string? GetContextConnectionString
        {
            get
            {
                return _context?.Database.GetDbConnection().ConnectionString;
            }
        }

        public SessionInformation SessionInformation;

        private readonly DbSet<T> _dbSet;

        public List<Log> Logs { get; set; } = new List<Log>();

        public List<ResponseObjectMessage> Messages { get; set; } = new List<ResponseObjectMessage>();

        private readonly string TypeName = typeof(T).Name;

        public Repository(RepositoryInjector injector)
        {
            _context = injector.Context;

            _dbSet = injector.Context.Set<T>();

            LogService = injector.LogService;
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
                throw new Exception("There is no such entity in either Db or Context");

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

        protected virtual async Task<bool> IsItNewAsync(T entity)
        {
            try
            {
                var existing = await GetUniqueAsync(entity);
                return existing == null;
            }
            catch (Exception ex)
            {
                throw new Exception($"[Repository] IsItNew failed for {typeof(T).Name}: {ex.Message}", ex);
            }
        }



        public IQueryable<T> FromSqlRaw(string sql, bool asNoTracking = true, params object[] parameters)
        {
            IQueryable<T> list = null;

            try
            {
                list = _dbSet.FromSqlRaw(sql, parameters);
            }
            catch (Exception ex)
            {
                //LogManager.Save(new Log(ex.Message));
            }

            return asNoTracking ? list?.AsNoTracking() : list;
        }

        public IQueryable<T> FromSqlInterpolated(FormattableString sql, bool asNoTracking = true)
        {
            IQueryable<T> list = null;

            try
            {
                list = DbSet.FromSqlInterpolated(sql);
            }
            catch (Exception ex)
            {
                //LogManager.Save(new Log(ex.Message));
            }

            return asNoTracking ? list.AsNoTracking() : list;
        }



        public Repository<T> SetSessionInformation(SessionInformation sessionInformation)
        {
            SessionInformation = sessionInformation;

            return this;
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

        public virtual async Task<ResponseObjectForUpdate> UpdateAsync(T entity)
        {
            var response = new ResponseObjectForUpdate();
            var isUpdated = false;

            try
            {
                var existingEntity = await GetExistingEntityAsync(entity, throwException: true, withAllIncludes: true);

                entity.Id = existingEntity.Id;
                entity.AddedDate = existingEntity.AddedDate;

                var entityEntry = GetAsEntityEntry(existingEntity);
                entityEntry.CurrentValues.SetValues(entity);
                entity = existingEntity;

                if (HasAnyModifiedProperty(entity))
                {
                    entity.ModifiedDate = DateTime.Now;

                    var modifiedProps = GetModifiedProperties(entity).ToArray();
                    response.ModifiedProperties = modifiedProps;

                    ChangeEntityState(entity, EntityState.Modified);
                    isUpdated = true;
                }
                else
                {
                    Messages.Add(new ResponseObjectMessage(title: "Nothing changed", text: "This record has no modified field", showWhenSuccess: false));
                }
            }
            catch (Exception)
            {
                throw; 
            }

            return response.SetSuccess(isUpdated);
        }


        public virtual async Task<List<ResponseObjectForUpdate>> UpdateRangeAsync(List<T> entities)
        {
            var results = new List<ResponseObjectForUpdate>();

            foreach (var entity in entities)
            {
                var result = await UpdateAsync(entity);
                results.Add(result);
            }

            return results;
        }



        public static IRepository<T>? GetOwnRepository(RepositoryInjector injector) 
        {
            var repositoryType = ReflectionHelper.GetTypeFromAssembly(typeof(IRepository<>), string.Format("{0}Repository", typeof(T).Name)) ?? typeof(Repository<T>);

            var instance = ReflectionHelper.CreateInstance(typeName:nameof(repositoryType),parameters: new object[]
                {
                    new object[]
                    {
                        injector
                    }
                });

            //var instance = ReflectionHelper.InvokeMethod(invokerType: typeof(ReflectionHelper),
            //    invokerObject: null,
            //    methodName: nameof(ReflectionHelper.CreateInstance),
            //    genericTypes: new[] { repositoryType },
            //    parameters: new object[]
            //    {
            //        new object[]
            //        {
            //            injector
            //        }
            //    });

            return instance as IRepository<T>;
        }

        public List<Log> ConsumeLogs()
        {
            var logList = new List<Log>(Logs);

            Logs.Clear();

            return logList;
        }


    }

    public class GroupObject<TKey>
    {
        public TKey Key { get; set; } = default!;
        public int Count { get; set; }
    }
}
