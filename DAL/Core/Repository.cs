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

        Task<T?> GetAsync(Expression<Func<T, bool>> filter, bool withAllIncludes = false, params string[] includes);

        Task<T?> GetByIdAsync(Guid id, bool withAllIncludes = false, params string[] includes);

        T GetEntityFromContext(T entity);

        string[] GetAllIncludes();

        Task<T?> GetUniqueAsync(T entity, bool withAllIncludes = false, params string[] includes);

        Task<bool> IsItNewAsync(T entity);

        void ShowChangeTrackerEntriesStates();
        Expression<Func<T, bool>> UniqueFilter(T entity, bool forEntityFramework = true);


        ResponseObjectForUpdate Update(T entity);
    }

    public class Repository<T> : IRepository<T> where T : BaseObject<T>
    {
        private readonly ILogService LogService;

        private readonly DbContext? _context;

        //public DbContext? Context { get { return _context; } }

        public string? GetContextConnectionString
        {
            get
            {
                return _context.Database.GetDbConnection().ConnectionString;
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

        public virtual async Task<bool> CreateAsync(T entity)
        {
            try
            {
                var existingEntity = await GetExistingEntityAsync(entity, throwException: false);

                if (existingEntity != null)
                {
                    Logs.Add(new Log("Record already exists", LogType.Warning, entity.Id.ToString(), LogProcessType.Create, SessionInformation));
                    return false;
                }

                entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
                entity.AddedDate = DateTime.Now;
                entity.ModifiedDate = DateTime.Now;

                ChangeEntityState(entity, EntityState.Added);

                if (!(entity is Log))
                {
                    Logs.Add(new Log($"{TypeName} created", LogType.Info, entity.Id.ToString(), LogProcessType.Create, SessionInformation));
                }

                return true;
            }
            catch (Exception ex)
            {
                Logs.Add(new Log(ex.Message, LogType.Error, entity.Id.ToString(), LogProcessType.Create, SessionInformation));
                return false;
            }
        }

        public virtual Task<bool> DeleteAsync(T entity)
        {
            if (_context?.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }

            ChangeEntityState(entity, EntityState.Deleted);

            return Task.FromResult(true);
        }

        public virtual async Task<bool> DeleteRangeAsync(List<Guid> idList)
        {
            var entities = _dbSet.Where(e => idList.Contains(e.Id)).ToList();

            return await DeleteRangeAsync(entities);
        }

        public virtual Task<bool> DeleteRangeAsync(List<T> entities)
        {
            if (entities == null || !entities.Any())
                return Task.FromResult(false);

            foreach (var entity in entities)
            {
                if (_context?.Entry(entity).State == EntityState.Detached)
                {
                    _dbSet.Attach(entity);
                }

                ChangeEntityState(entity, EntityState.Deleted);
            }

            return Task.FromResult(true);
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


        //public List<GroupObject> GroupBy(Expression<Func<T, object>> keySelector)
        //{
        //    var list = Context.Set<T>().GroupBy(keySelector).Select(g => new GroupObject { Key = g.Key, Count = g.Count() }).ToList();

        //    return list;
        //}

        public async Task<T?> GetExistingEntityAsync(T entity, bool throwException = true, bool withAllIncludes = false)
        {
            var entityFromContext = GetEntityFromContext(entity);
            if (entityFromContext != null)
                return entityFromContext;

            var uniqueEntity = await GetUniqueAsync(entity, withAllIncludes);
            if (uniqueEntity == null && throwException)
                throw new Exception("There is no such entity in either Db or Context");

            return uniqueEntity;
        }



        public virtual async Task<T?> GetUniqueAsync(T entity, bool withAllIncludes = false, params string[] includes)
        {
            return await GetAsync(UniqueFilter(entity), withAllIncludes, includes);
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
                throw new Exception($"[Repository] IsItNew failed for {typeof(T).Name}: {ex.Message}", ex);
            }
        }








        public Repository<T> SetSessionInformation(SessionInformation sessionInformation)
        {
            SessionInformation = sessionInformation;

            return this;
        }

        public virtual ResponseObjectForUpdate Update(T entity)
        {
            bool isUpdated = false;

            var responseObjectForUpdate = new ResponseObjectForUpdate();

            try
            {
                //var entityFromContext = GetEntityFromContext(entity);

                //if (entityFromContext != null)
                //{
                //    ChangeEntityState(entityFromContext, EntityState.Detached);
                //}


                var existingEntity = GetExistingEntity(entity, withAllIncludes: true);

                entity.Id = existingEntity.Id;
                entity.AddedDate = existingEntity.AddedDate;

                //var foreignKeyProperties = existingEntity.GetType()
                //                                        .GetProperties()
                //                                        .Where(p => Helper.IsPropertyFrom<Guid>(p) && p.CanWrite);

                //foreach (var foreignKeyProperty in foreignKeyProperties)
                //{
                //    if (alteredForeignKeys == null || 
                //        (alteredForeignKeys != null && alteredForeignKeys.Contains(foreignKeyProperty.Name)))
                //        continue;

                //    foreignKeyProperty.SetValue(entity, foreignKeyProperty.GetValue(existingEntity));
                //}

                var entityEntry = GetAsEntityEntry(existingEntity);
                entityEntry.CurrentValues.SetValues(entity);


                /*
                 Warning : Please dont remove below line
                 The instance of entity type 'SystemUser' cannot be tracked because another instance with the same key value for {'Id'} is already being tracked. When attaching existing entities, ensure that only one entity instance with a given key value is attached. Consider using 'DbContextOptionsBuilder.EnableSensitiveDataLogging' to see the conflicting key values.
                 */
                entity = existingEntity;

                if (HasAnyModifiedProperty(entity))
                {
                    entity.ModifiedDate = DateTime.Now;

                    Console.Write("\nModified Type : {0} \n", typeof(T).Name);

                    var modifiedProperties = GetModifiedProperties(entity).ToArray();

                    responseObjectForUpdate.ModifiedProperties = modifiedProperties;

                    foreach (var modifiedProperty in modifiedProperties)
                    {

                        var newValue = GetAsEntityEntry(entity).CurrentValues[modifiedProperty];
                        var oldValue = GetAsEntityEntry(entity).GetDatabaseValues()[modifiedProperty];

                        System.Diagnostics.Debug.WriteLine($"modifiedProperty : {modifiedProperty} \t newValue : {newValue}");

                        //if (entity.GetType().Name != nameof(Log) && entity.GetType().BaseType.Name != nameof(Log))
                        //    Logs.Add(LogManager.LogToUpdateLog(new Log(category: TypeName,
                        //        name: modifiedProperty,
                        //        description: null,
                        //        logType: LogType.Info,
                        //        entityId: entity.Id.ToString(),
                        //        vesselId: (entity as ISyncObject)?.GetVesselId,
                        //        processType: LogProcessType.Update,
                        //        sessionInformation: SessionInformation),
                        //        oldValue: oldValue?.ToString(),
                        //        newValue: newValue?.ToString(),
                        //        rowVersion: entity.RowVersion));
                    }

                    ChangeEntityState(entity, EntityState.Modified);

                    isUpdated = true;
                }
                else
                {
                    Messages.Add(new ResponseObjectMessage(title: "Nothing changed", text: "This record has no modified field", showWhenSuccess: false));
                }
            }
            catch (Exception ex)
            {
                //LogManager.Save(new Log(description: ex.Message.ToString(),
                //                    logType: LogType.Error,
                //                    entityId: entity.Id.ToString(),
                //                    vesselId: (entity as ISyncObject)?.GetVesselId,
                //                    processType: LogProcessType.Update,
                //                    sessionInformation: SessionInformation));
            }



            return responseObjectForUpdate
                    .SetSuccess(isUpdated);
        }

        public bool Contains(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }


        public EntityEntry GetAsEntityEntry(T entity)
        {
            return _context.Entry(entity);
        }

        public void ChangeEntityState(T entity, EntityState entityState)
        {
            GetAsEntityEntry(entity).State = entityState;
        }

        public IEnumerable<EntityEntry> GetContextChangeTrackerEntries()
        {
            return _context.ChangeTracker.Entries();
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

        public IEnumerable<string> GetModifiedProperties(T entity)
        {
            var ee = GetAsEntityEntry(entity);

            var ByteArrayEquality = new Func<byte[], byte[], bool>(
                (first, second) =>
                {
                    if (first == null || second == null)
                        return false;

                    int i;

                    if (first.Length == second.Length)
                    {
                        i = 0;

                        while (i < first.Length && first[i] == second[i])
                        {
                            i++;
                        }

                        if (i == first.Length)
                        {
                            return true;
                        }
                    }

                    return false;
                });

            var byteArrayProperties = entity.GetType().GetProperties().Where(p => p.PropertyType.Name == "Byte[]");

            foreach (var propertyName in ee.CurrentValues.Properties.Select(p => p.Name))
            {
                if (new[] { "ModifiedDate", "AddedDate", "RowVersion" }.Any(p => p == propertyName))
                    continue;

                if (byteArrayProperties.Any(p => p.Name == propertyName))
                {
                    if (!ByteArrayEquality((byte[])ee.GetDatabaseValues()[propertyName], (byte[])ee.CurrentValues[propertyName]))
                        yield return propertyName;
                }
                else if (ee.GetDatabaseValues()[propertyName]?.ToString() != ee.CurrentValues[propertyName]?.ToString())
                {
                    yield return propertyName;
                }
            }
        }



        public T GetEntityFromContext(T entity)
        {
            var entityInContext = GetContextChangeTrackerEntries()
                        .Select(entityEntry => entityEntry.Entity as T)
                        .Where(ee => ee != null)
                        .FirstOrDefault(UniqueFilter(entity).Compile());

            return entityInContext;
        }


        //public virtual Task<T?> GetUnique(T entity, bool withAllIncludes = false, params string[] includes)
        //{
        //    return await GetAsync(UniqueFilter(entity), withAllIncludes: withAllIncludes, includes: includes);
        //}

        public bool HasAnyModifiedProperty(T entity)
        {
            return GetModifiedProperties(entity).Any();
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

        public virtual Expression<Func<T, bool>> UniqueFilter(T entity, bool forEntityFramework = true)
        {
            return t => t.Id == entity.Id;
        }
    }
}
