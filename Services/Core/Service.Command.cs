using Hydra.Core;
using Hydra.Http;
using System.Linq.Expressions;
using Hydra.Services.Cache;

namespace Hydra.Services.Core
{
    //COMMAND
    public partial class Service<T> : IService<T> where T : BaseObject<T>
    {
        public virtual async Task<bool> CreateAsync(T entity)
        {
            try
            {
                var result = await Repository.CreateAsync(entity);

                if (result)
                {
                    if(EnableForCommitting)
                        await CommitAsync();
                    
                    if(HasCache)
                        CacheService?.Add(entity.Id, entity);
                }

                var logs = Repository.ConsumeLogs();

                foreach (var log in logs)
                {
                    await LogService.SaveAsync(log, LogRecordType.Database); 
                }

                return result;
            }
            catch (Exception ex)
            {
                var errorLog = new Log(ex.Message, LogType.Error, entity.Id.ToString(), LogProcessType.Create, SessionInformation);
                await LogService.Create(errorLog, LogRecordType.All);

                return false;
            }
        }

        public async Task<bool> CreateOrUpdate(T entity, Expression<Func<T, bool>>? expression = null)
        {
            bool isDone = false;

            if ((expression != null && !Repository.Any(expression)) || ((expression == null) && IsItNew(entity)))
            {
                isDone = await CreateAsync(entity);
            }
            else
            {
                entity.Id = (await GetUniqueAsync(entity)).Id;

                var updateResponse = UpdateAsync(entity);

                isDone = updateResponse.IsSuccess;
            }

            //task.Start();

            //await Task.WhenAll(new[] { task });

            return await Task.FromResult(isDone);
        }

        public async Task<bool> DeleteAsync(Expression<Func<T, bool>> filter)
        {
            var entity = await Repository.GetAsync(filter);
            if (entity == null)
                return false;

            return await DeleteAsync(entity);
        }

        public async Task<bool> DeleteAsync(T entity)
        {
            try
            {
                var success = await Repository.DeleteAsync(entity);

                if (!success)
                    throw new Exception("DeleteAsync failed.");

                var isCommitted = await _unitOfWork.CommitAsync();

                await LogInfoAsync($"Entity deleted", entity.Id, LogProcessType.Delete);

                if (isCommitted && HasCache)
                    CacheService?.Remove(entity.Id);

                return true;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("DeleteAsync Exception : " + ex.Message, entity.Id, LogProcessType.Delete);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await DeleteAsync(e => e.Id == id);
        }

        public async Task<bool> DeleteRangeAsync(List<T> entities)
        {
            try
            {
                var success = await Repository.DeleteRangeAsync(entities);

                if (!success)
                    throw new Exception("DeleteRangeAsync failed.");

                await _unitOfWork.CommitAsync();

                foreach (var entity in entities)
                {
                    await LogInfoAsync($"Entity deleted",entity.Id, LogProcessType.Delete);

                    if (HasCache)
                        CacheService?.Remove(entity.Id);
                }

                return true;
            }
            catch (Exception ex)
            {
                await LogErrorAsync(description: "DeleteRangeAsync Exception : "+ ex.Message,processType: LogProcessType.Delete);
                return false;
            }
        }

        public async Task<bool> DeleteRangeAsync(List<Guid> idList)
        {
            try
            {
                var entities = await Repository.(e => idList.Contains(e.Id));
                return await DeleteRangeAsync(entities);
            }
            catch (Exception ex)
            {
                await LogErrorAsync("DeleteRangeAsync (by id list) Exception: " + ex.Message, null, LogProcessType.Delete);
                return false;
            }
        }

        public virtual async Task<ResponseObjectForUpdate> UpdateAsync(T entity)
        {
            try
            {
                var updateResponse = await Repository.UpdateAsync(entity);

                if (updateResponse.IsSuccess)
                {
                    if (EnableForCommitting)
                        await CommitAsync();

                    if (HasCache)
                        CacheService?.TryRefresh(entity.Id, entity);

                    var logs = Repository.ConsumeLogs();

                    foreach (var log in logs)
                        await LogService.SaveAsync(log, LogRecordType.Database);
                }

                return updateResponse;
            }
            catch (Exception ex)
            {
                var errorLog = new Log(ex.Message, LogType.Error, entity.Id.ToString(), LogProcessType.Update, SessionInformation);
                await LogService.Create(errorLog, LogRecordType.All);

                return new ResponseObjectForUpdate()
                    .SetSuccess(false)
                    .AddMessage("Update failed: " + ex.Message);
            }
        }


    }
}
