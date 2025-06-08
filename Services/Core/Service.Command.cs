using Hydra.Core;
using Hydra.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services.Core
{
    //COMMAND
    public partial class Service<T> : IService<T> where T : BaseObject<T>
    {
        public virtual bool Create(T entity)
        {
            var isCommitted = Repository.Create(entity) && Commit();

            try
            {
                if (isCommitted)
                {
                    var log = Repository.ConsumeLogs().Where(l => l.ProcessType == LogProcessType.Create).SingleOrDefault();

                    //LogManager.Save(log);

                    if (log == null)
                        return isCommitted;

                    var logRepository = new LogRepository(Repository.GetInjector());

                    logRepository.Create(log);

                    Commit();

                    if (HasCache)
                        Cache<T>.AddObject(entity);
                }
            }
            catch (Exception ex)
            {
                LogManager.Save(new Log(ex.Message, entityId: entity.UniqueProperty));
            }

            return isCommitted;
        }

        public async Task<bool> CreateOrUpdate(T entity, Expression<Func<T, bool>> expression = null)
        {
            bool isDone = false;

            //Task<bool> task;

            if ((expression != null && !Repository.Any(expression)) || ((expression == null) && IsItNew(entity)))
            {
                isDone = Create(entity);
            }
            else
            {
                entity.Id = GetUnique(entity).Id;

                var updateResponse = Update(entity);

                isDone = updateResponse.IsSuccess;
            }

            //task.Start();

            //await Task.WhenAll(new[] { task });

            return await Task.FromResult(isDone);
        }

        public bool Delete(T entity)
        {
            return Delete(entity.Id);
        }

        public virtual bool Delete(Guid id)
        {
            return Delete(i => i.Id == id);
        }

        public bool Delete(Expression<Func<T, bool>> filter)
        {
            var isCommitted = false;

            Guid? entityId = null;

            try
            {
                var existingEntity = Repository.Get(filter);

                if (existingEntity == null)
                    throw new NullReferenceException();

                entityId = existingEntity?.Id;

                if (!Repository.Delete(existingEntity))
                    throw new Exception(string.Format("{0} Delete Exception", Repository.GetType().Name));

                isCommitted = Commit();

                if (isCommitted)
                {
                    var log = Repository.ConsumeLogs().Where(l => l.ProcessType == LogProcessType.Delete).FirstOrDefault();

                    //LogManager.Save(log);

                    var logRepository = new LogRepository(Repository.GetInjector());
                    if (log != null)
                        logRepository.Create(log);

                    Commit();

                    if (HasCache)
                    {
                        Cache<T>.RemoveObjectById(existingEntity.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Save(new Log(ex.Message, entityId: entityId?.ToString()));
            }

            return isCommitted;
        }

        public virtual bool DeleteRange(List<T> entities)
        {
            var isCommitted = false;

            if (!entities.Any())
                return isCommitted;

            try
            {
                if (!Repository.DeleteRange(entities))
                    throw new Exception(string.Format("{0} DeleteRange Exception", Repository.GetType().Name));

                isCommitted = Commit();

                if (isCommitted)
                {
                    var logs = Repository.Logs.Where(l => l.ProcessType == LogProcessType.Delete).ToList();

                    //logs.ForEach(l=>LogManager.Save(l));

                    var logRepository = new LogRepository(Repository.GetInjector());

                    foreach (var log in logs)
                    {
                        logRepository.Create(log);
                    }

                    Commit();

                    if (HasCache)
                    {
                        entities.ForEach(e => Cache<T>.RemoveObjectById(e.Id));
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Save(new Log(ex.Message));
            }

            return isCommitted;
        }

        public bool DeleteRange(List<Guid> idList)
        {
            var entities = SelectWithLinq(e => idList.Contains(e.Id));

            return DeleteRange(entities);
        }

        public virtual ResponseObjectForUpdate Update(T entity)
        {
            var isCommitted = false;

            if (!EnableForCommitting && !Any(e => e.Id == entity.Id))
            {
                return new ResponseObjectForUpdate().SetSuccess(true);
            }

            var responseObjectForUpdate = Repository.Update(entity);

            if (!responseObjectForUpdate.IsSuccess)
                return responseObjectForUpdate;

            isCommitted = Commit();

            if (isCommitted)
            {
                var logs = Repository.ConsumeLogs().Where(l => l.ProcessType == LogProcessType.Update);

                foreach (var log in logs)
                {
                    //LogManager.Save(log);

                    var logRepository = new LogRepository(Repository.GetInjector());

                    logRepository.Create(log);
                }

                Commit();

                if (HasCache)
                {
                    Cache<T>.Refresh(entity);
                }
            }

            return responseObjectForUpdate
                    .SetSuccess(isCommitted);
        }

    }
}
