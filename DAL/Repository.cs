using Hydra.Core;
using Hydra.DataModels.Filter;
using Hydra.DI;
using Hydra.Http;
using Hydra.IdentityAndAccess;
using Hydra.Services;
using Hydra.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DAL
{
    public interface IRepository<T> where T : IBaseObject<T>
    {
        DbContext? Context { get; set; }
        List<Log> Logs { get; set; }

        List<ResponseObjectMessage> Messages { get; set; }

        IQueryable<T> All(params string[] includes);

        bool Any(Expression<Func<T, bool>>? filter = null);

        bool Create(T entity);

        bool Contains(Expression<Func<T, bool>> predicate);

        List<Log> ConsumeLogs();

        int Count(Expression<Func<T, bool>>? predicate = null);

        ResponseObjectForUpdate Update(T entity);

        bool Delete(T entity);

        bool DeleteRange(List<T> entities);

        //IQueryable<T> FilterWithDynamicLinq(IFilter? filter = null);

        //IQueryable<T> FilterWithDynamicLinq(string filter, object[] parameters);

        //IQueryable<T> FilterWithLinq(Expression<Func<T, bool>>? filter = null);

        T Get(Expression<Func<T, bool>> filter, bool withAllIncludes = false, params string[] includes);

        T GetById(Guid id, bool withAllIncludes = false, params string[] includes);

        string[] GetAllIncludes();

        IRepository<T> SetContext(DbContext context);

        T GetUnique(T entity, bool withAllIncludes = false, params string[] includes);

        bool IsItNew(T entity);
        Expression<Func<T, bool>> UniqueFilter(T entity, bool forEntityFramework = true);


        void ShowChangeTrackerEntriesStates();

        //RepositoryInjector GetInjector();

        string GetContextConnectionString
        {
            get;
        }

        T GetEntityFromContext(T entity);
    }

    public class Repository<T> : IRepository<T> where T : BaseObject<T>
    {
        private readonly LogService LogService;

        public DbContext? Context { get; set; } = null;

        public string GetContextConnectionString
        {
            get
            {
                return Context.Database.GetDbConnection().ConnectionString;
            }
        }

        public SessionInformation SessionInformation;

        protected DbSet<T> DbSet;
        public bool ProxyCreationEnabled { get; set; }
        public List<Log> Logs { get; set; } = new List<Log>();

        public List<ResponseObjectMessage> Messages { get; set; } = new List<ResponseObjectMessage>();

        private readonly string TypeName = typeof(T).Name;

        public Repository(RepositoryInjector injector)
        {
            SetContext(injector.Context);

            //SetSessionInformation(injector.SessionInformation);

            LogService = injector.LogService;
        }

        //public Repository(DbContext context, SessionInformation sessionInformation = null) : this(new RepositoryInjector(context, sessionInformation))
        //{

        //}


        //public RepositoryInjector GetInjector()
        //{
        //    return new RepositoryInjector(Context, SessionInformation);
        //}

        public IQueryable<T> FromSqlRaw(string sql, bool asNoTracking = true, params object[] parameters)
        {
            IQueryable<T> list = null;

            try
            {
                list = DbSet.FromSqlRaw(sql, parameters);
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

        public IRepository<T> SetContext(DbContext context)
        {
            try
            {
                if (context == null)
                    throw new ArgumentNullException("context");

                Context = context;

                DbSet = context.Set<T>();
            }
            catch (Exception ex)
            {
                //Dosyaya yazan Log mekanizması yazılabilir
            }

            return this;
        }

        public Repository<T> SetSessionInformation(SessionInformation sessionInformation)
        {
            SessionInformation = sessionInformation;

            return this;
        }

        public T Get(Expression<Func<T, bool>> filter, bool withAllIncludes = false, params string[] includes)
        {
            try
            {
                IQueryable<T> query = DbSet;

                includes = withAllIncludes ? GetAllIncludes() : includes;

                if (includes != null && includes.Any())
                    foreach (var include in includes)
                    {
                        query = query.Include(include);
                    }

                return query.SingleOrDefault(filter);
            }
            catch (Exception ex)
            {
                //LogManager.Save(new Log(description: ex.Message.ToString(),
                //            logType: LogType.Error,
                //            entityId: null,
                //            vesselId: null,
                //            processType: LogProcessType.None,
                //            sessionInformation: SessionInformation));

                throw;
            }
        }

        public T GetById(Guid id, bool withAllIncludes = false, params string[] includes)
        {
            return Get(filter: e => e.Id == id, withAllIncludes: withAllIncludes, includes: includes);
        }

        //public List<GroupObject> GroupBy(Expression<Func<T, object>> keySelector)
        //{
        //    var list = Context.Set<T>().GroupBy(keySelector).Select(g => new GroupObject { Key = g.Key, Count = g.Count() }).ToList();

        //    return list;
        //}

        public IQueryable<T> All(params string[] includes)
        {
            IQueryable<T> query = DbSet;

            if (includes != null && includes.Any())
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }

            return query;
        }

        public virtual bool Any(Expression<Func<T, bool>> filter = null)
        {
            var isExisted = false;

            try
            {
                IQueryable<T> query = DbSet;

                isExisted = query.Any(filter);
            }
            catch (Exception ex)
            {
                //LogManager.Save(new Log(string.Format("Class : {0}, Error : {1}", typeof(T).FullName, ex.ToString())));
            }

            return isExisted;
        }

        public virtual bool Create(T entity)
        {
            bool isInserted = false;

            try
            {
                var existingEntity = GetExistingEntity(entity, throwException: false);

                if (existingEntity != null)
                {
                    var state2 = GetAsEntityEntry(existingEntity);

                    var alreadyExitsMessage = "This record already exists in the database";

                    Messages.Add(new ResponseObjectMessage(title: "Availability", text: alreadyExitsMessage, showWhenSuccess: false));

                    throw new Exception(alreadyExitsMessage);
                }

                entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;

                entity.AddedDate = DateTime.Now;

                entity.ModifiedDate = DateTime.Now;

                var state = GetAsEntityEntry(entity);

                ShowChangeTrackerEntriesStates();

                ChangeEntityState(entity, EntityState.Added);

                isInserted = true;

                //if (entity.GetType().Name != nameof(Log) && entity.GetType().BaseType.Name != nameof(Log))
                //    Logs.Add(new Log(category: TypeName,
                //            name: null,
                //            description: null,
                //            logType: LogType.Info,
                //            entityId: entity.Id.ToString(),
                //            vesselId: (entity as ISyncObject)?.GetVesselId,
                //            processType: LogProcessType.Create,
                //            sessionInformation: SessionInformation));
            }
            catch (Exception ex)
            {
                //LogManager.Save(new Log(description: ex.Message.ToString(),
                //    logType: LogType.Error,
                //    entityId: entity.Id.ToString(),
                //    vesselId: (entity as ISyncObject)?.GetVesselId,
                //    processType: LogProcessType.Create,
                //    sessionInformation: SessionInformation));
            }

            return isInserted;
        }


        public IQueryable<T> FilterWithLinq(Expression<Func<T, bool>> filter = null)
        {
            IQueryable<T> query = DbSet;

            try
            {
                query = filter != null ? query.Where(filter) : query;
            }
            catch (Exception ex)
            {

                throw;
            }

            return query;
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

        public virtual bool DeleteRange(List<T> entities)
        {
            try
            {
                foreach (var entity in entities)
                {
                    Delete(entity);
                }

                //foreach (var entity in entities)
                //{
                //    if (Context.Entry(entity).State == EntityState.Detached)
                //    {
                //        DbSet.Attach(entity);
                //    }

                //    ChangeEntityState(entity, EntityState.Deleted);

                //    var message = new
                //    {
                //        Count = GetContextChangeTrackerEntries().Count(),
                //        Description = string.Join(",\n", GetContextChangeTrackerEntries().Select(e => string.Format("{0} : {1}", e.Entity.GetType().Name, e.State)))
                //    };

                //    Logs.Add(new Log(category: typeof(T).Name,
                //        name: null,
                //        string.Format("\nIn deleting process {0} entities was affected.Message : {1} \n", message.Count, message.Description),
                //        logType: LogType.Info,
                //        entityId: entity.Id.ToString(),
                //        vesselId: (entity as ISyncObject)?.GetVesselId,
                //        userId: "userId",
                //        processType: LogProcessType.Delete,
                //        ip: "ip"));
                //}

                //Tek tek siliyor
                //Context.RemoveRange(entities);

                return true;
            }
            catch (Exception ex)
            {
                //LogManager.Save(new Log(description: ex.Message.ToString(),
                //                 logType: LogType.Error,
                //                 entityId: null,
                //                 vesselId: null,
                //                 processType: LogProcessType.Delete,
                //                 sessionInformation: SessionInformation));
            }


            return false;
        }

        public virtual bool Delete(T entity)
        {
            bool isDeleted = false;

            try
            {
                if (Context.Entry(entity).State == EntityState.Detached) //Concurrency için
                {
                    DbSet.Attach(entity);
                }

                //DeleteDependencies(entity);

                ChangeEntityState(entity, EntityState.Deleted);

                isDeleted = true;

                if (isDeleted)
                {
                    var message = new
                    {
                        Count = GetContextChangeTrackerEntries().Count(),
                        Description = string.Join(",\n", GetContextChangeTrackerEntries().Select(e => string.Format("{0} : {1}", e.Entity.GetType().Name, e.State)))
                    };

                    ////Log nesnesi silindiğinde test edilecek
                    //if (typeof(T).Name != "Log")
                    //    Logs.Add(new Log(category: typeof(T).Name,
                    //                                name: null,
                    //                                description: string.Format("\nIn deleting process {0} entities was affected.Message : {1} \n", message.Count, message.Description),
                    //                                logType: LogType.Info,
                    //                                entityId: entity.Id.ToString(),
                    //                                vesselId: (entity as ISyncObject)?.GetVesselId,
                    //                                processType: LogProcessType.Delete,
                    //                                sessionInformation: SessionInformation));

                    Console.WriteLine(string.Format("\nIn deleting process {0} entities was affected.Message : {1} \n", message.Count, message.Description));

                    ShowChangeTrackerEntriesStates();
                }
            }
            catch (Exception ex)
            {
                //LogManager.Save(new Log(description: ex.Message.ToString(),
                //                                 logType: LogType.Error,
                //                                 entityId: entity.Id.ToString(),
                //                                 vesselId: (entity as ISyncObject)?.GetVesselId,
                //                                 processType: LogProcessType.Delete,
                //                                 sessionInformation: SessionInformation));
            }

            return isDeleted;
        }

        public bool Contains(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public int Count(Expression<Func<T, bool>> predicate = null)
        {
            try
            {
                IQueryable<T> query = DbSet;

                return query.Count(predicate);
            }
            catch (Exception ex)
            {
                //LogManager.Save(new Log(string.Format("Class : {0}, Error : {1}", typeof(T).FullName, ex.ToString())));
            }

            return 0;
        }

        public EntityEntry GetAsEntityEntry(T entity)
        {
            return Context.Entry(entity);
        }

        public void ChangeEntityState(T entity, EntityState entityState)
        {
            GetAsEntityEntry(entity).State = entityState;
        }

        public IEnumerable<EntityEntry> GetContextChangeTrackerEntries()
        {
            return Context.ChangeTracker.Entries();
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

                        while (i < first.Length && (first[i] == second[i]))
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

        public bool HasAnyModifiedProperty(T entity)
        {
            return GetModifiedProperties(entity).Any();
        }
        public T GetExistingEntity(T entity, bool throwException = true, bool withAllIncludes = false)
        {
            T t = GetEntityFromContext(entity) ?? GetUnique(entity, withAllIncludes);
            //T t = GetUnique(entity, withAllIncludes);

            if (t == null && throwException)
                throw new Exception("There is no such an entity in either Db or Context");

            return t;
        }


        public T GetEntityFromContext(T entity)
        {
            var entityInContext = GetContextChangeTrackerEntries()
                        .Select(entityEntry => entityEntry.Entity as T)
                        .Where(ee => ee != null)
                        .FirstOrDefault(UniqueFilter(entity).Compile());

            return entityInContext;
        }


        public virtual T GetUnique(T entity, bool withAllIncludes = false, params string[] includes)
        {
            return Get(UniqueFilter(entity), withAllIncludes: withAllIncludes, includes: includes);
        }
        public virtual Expression<Func<T, bool>> UniqueFilter(T entity, bool forEntityFramework = true)
        {
            return t => t.Id == entity.Id;
        }
        public virtual bool IsItNew(T entity)
        {
            var isItNew = false;

            try
            {
                isItNew = GetUnique(entity) == null;
            }
            catch (Exception ex)
            {

            }

            return isItNew;
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
}
