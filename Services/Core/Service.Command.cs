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
                var isDone = await Repository.CreateAsync(entity);

                if (isDone)
                {
                    var isCommitted = false;

                    if (EnableForCommitting)
                        isCommitted = isDone = await CommitAsync();
                    
                    if(isCommitted && HasCache)
                        CacheService?.Add(entity.Id, entity);
                }

                await SaveRepositoryLogsAsync();

                return isDone;
            }
            catch (Exception ex)
            {
                //var errorLog = new Log(ex.Message, LogType.Error, entity.Id.ToString(), LogProcessType.Create, SessionInformation);
                //await LogService.SaveAsync(errorLog, LogRecordType.Database);

                await SaveErrorLogAsync(ex.Message,entity.Id,LogProcessType.Create);

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
                var isDone = await Repository.DeleteAsync(entity);

                if (isDone)
                {
                    var isCommitted = false;

                    if (EnableForCommitting)
                        isCommitted = isDone = await _unitOfWork.CommitAsync();

                    if (isCommitted && HasCache)
                        CacheService?.Remove(entity.Id);
                }

                await SaveRepositoryLogsAsync();

                return isDone;
            }
            catch (Exception ex)
            {
                await SaveErrorLogAsync(ex.Message, entity.Id, LogProcessType.Delete);

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
                var isDone = await Repository.DeleteRangeAsync(entities);

                if (isDone)
                {
                    var isCommitted = false;

                    if (EnableForCommitting)
                        isCommitted = isDone = await _unitOfWork.CommitAsync();

                    if (isCommitted && HasCache)
                    {
                        entities.ForEach(e=> CacheService?.Remove(e.Id));
                    }

                }

                await SaveRepositoryLogsAsync();

                return isDone;
            }
            catch (Exception ex)
            {
                entities.ForEach(async e => await SaveErrorLogAsync(ex.Message, e.Id, LogProcessType.DeleteBulk));

                return false;
            }
        }

        public async Task<bool> DeleteRangeAsync(List<Guid> idList)
        {
            try
            {
                var entities = await Repository.FilterWithLinqAsync(e => idList.Contains(e.Id));

                return await DeleteRangeAsync(entities);
            }
            catch (Exception ex)
            {
                foreach (var id in idList)
                {
                    await SaveErrorLogAsync(ex.Message, id, LogProcessType.DeleteBulk);
                }

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
                    var isCommitted = false;

                    if (EnableForCommitting)
                        isCommitted = updateResponse.IsSuccess =  await CommitAsync();

                    if (isCommitted && HasCache)
                        CacheService?.TryRefresh(entity.Id, entity);
                }

                await SaveRepositoryLogsAsync();

                return updateResponse;
            }
            catch (Exception ex)
            {
                await SaveErrorLogAsync(ex.Message,entity.Id,LogProcessType.Update);

                return new ResponseObjectForUpdate()
                    .SetSuccess(false)
                    .AddMessage("Update failed: " + ex.Message);
            }
        }


    }
}
