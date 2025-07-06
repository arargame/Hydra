using Azure;
using Hydra.Core;
using Hydra.Core.Http;
using Hydra.Http;
using Hydra.Services.Cache;
using System.Linq.Expressions;

namespace Hydra.Services.Core
{
    //COMMAND
    public partial class Service<T> : IService<T> where T : BaseObject<T>
    {
        public virtual async Task<IResponseObject> CreateAsync(T entity)
        {
            var response = new ResponseObject()
                            .SetActionName("Create")
                            .SetId(entity.Id)
                            .UseDefaultMessages();

            bool isDone = false;
            bool isCommitted = false;

            try
            {
                isDone = await Repository.CreateAsync(entity);

                //if (isDone)
                //{
                //    if (EnableForCommitting)
                //        isCommitted = isDone = await CommitAsync();

                //    if (isCommitted && HasCache)
                //        CacheService?.Add(entity.Id, entity);
                //}

                if (isDone)
                {
                    isCommitted = EnableForCommitting ? await CommitAsync() : true;

                    if (isCommitted && HasCache)
                        CacheService?.Add(entity.Id, entity);
                }

                response.SetSuccess(isDone && isCommitted);

                await SaveRepositoryLogsAsync();

            }
            catch (Exception ex)
            {
                await SaveErrorLogAsync(ex.Message, entity.Id, LogProcessType.Create);

                response.Fail("Create Failed", ex.Message);

            }
            finally 
            {
                response.MergeRepositoryMessages(this);
            }

            return response;
        }

        public virtual async Task<IResponseObject> CreateOrUpdateAsync(T entity, Expression<Func<T, bool>>? expression = null)
        {
            IResponseObject response = new ResponseObject() ;

            bool isNew = false;

            try
            {
                if (expression != null)
                    isNew = !await Repository.AnyAsync(expression);
                else
                    isNew = await IsItNewAsync(entity);

                if (isNew)
                {
                    response = await CreateAsync(entity);
                }
                else
                {
                    var existing = await Repository.GetUniqueAsync(entity);
                    entity.Id = existing.Id;

                    response = await UpdateAsync(entity);
                }
            }
            catch (Exception ex)
            {
                await SaveErrorLogAsync(ex.Message, entity.Id, isNew ? LogProcessType.Create : LogProcessType.Update);

                response.Fail($"CreateOrUpdate Failed(isNew : {isNew})",ex.Message);
            }

            return response;
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
                await SaveErrorLogAsync(ex.Message, entity.Id.ToString(), LogProcessType.Delete);

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

        public virtual async Task<IResponseObject> UpdateAsync(T entity)
        {
            var updateResponse = new ResponseObjectForUpdate()
                                    .SetActionName("Update")
                                    .SetId(entity.Id)
                                    .UseDefaultMessages();

            try
            {
                updateResponse = await Repository.UpdateAsync(entity);

                if (updateResponse.Success)
                {
                    var isCommitted = EnableForCommitting ? await CommitAsync() : true;

                    updateResponse.SetSuccess(isCommitted);

                    if (isCommitted && HasCache)
                        CacheService?.TryRefresh(entity.Id, entity);
                }

                await SaveRepositoryLogsAsync();
            }
            catch (Exception ex)
            {
                await SaveErrorLogAsync(ex.Message, entity.Id, LogProcessType.Update);

                updateResponse.Fail("Update Failed", ex.Message);
            }
            finally
            {
                updateResponse.MergeRepositoryMessages(this);
            }

            return updateResponse;
        }

    }
}
