using Azure;
using Hydra.Core;
using Hydra.Core.Http;
using Hydra.Http;
using Hydra.Http.Hydra.Http;
using Hydra.Services.Cache;
using Hydra.ValidationManagement;
using System.Linq.Expressions;

namespace Hydra.Services.Core
{
    //COMMAND
    public partial class Service<T> : IService<T> where T : BaseObject<T>, new()
    {
        public virtual async Task<IResponseObject> CreateAsync(T entity)
        {
            var response = new ResponseObject()
                            .SetActionName("Create")
                            .SetId(entity.Id)
                            .UseDefaultMessages();

            if (!entity.IsValid(out var validationResults))
                return response.AddValidationMessages(validationResults)
                               .SetSuccess(false);

            bool isDone = false;
            bool isCommitted = false;

            try
            {
                isDone = await Repository.CreateAsync(entity);

                if (isDone)
                {
                    isCommitted = EnableForCommitting ? await CommitAsync() : true;

                    if (isCommitted && HasCache)
                        CacheService?.Add(entity.Id, entity);
                }

                response.SetSuccess(isDone && isCommitted);

                if (response.Success)
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

            if (!entity.IsValid(out var validationResults))
                return response.AddValidationMessages(validationResults)
                               .SetSuccess(false);

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


        public async Task<IResponseObject> DeleteAsync(Expression<Func<T, bool>> filter)
        {
            var entity = await Repository.GetAsync(filter);

            if (entity == null)
                return new ResponseObject()
                            .SetActionName("Delete")
                            .SetSuccess(false)
                            .AddExtraMessage(new ResponseObjectMessage("Delete", "Entity not found", false));

            return await DeleteAsync(entity);
        }


        public async Task<IResponseObject> DeleteAsync(T entity)
        {
            var response = new ResponseObject()
                .SetActionName("Delete")
                .SetId(entity.Id)
                .UseDefaultMessages();

            bool isDone = false;
            bool isCommitted = false;


            try
            {
                isDone = await Repository.DeleteAsync(entity);

                if (isDone)
                {
                    isCommitted = EnableForCommitting ? await _unitOfWork.CommitAsync() : true;

                    if (isCommitted && HasCache)
                        CacheService?.Remove(entity.Id);
                }

                response.SetSuccess(isDone && isCommitted);

                if (response.Success)
                    await SaveRepositoryLogsAsync();
            }
            catch (Exception ex)
            {
                await SaveErrorLogAsync(ex.Message, entity.Id, LogProcessType.Delete);

                response.Fail("Delete Failed", ex.Message);
            }
            finally
            {
                response.MergeRepositoryMessages(this);
            }

            return response;
        }

        public async Task<IResponseObject> DeleteAsync(Guid id)
        {
            return await DeleteAsync(e => e.Id == id);
        }

        public async Task<IResponseObject> DeleteRangeAsync(List<Guid> ids)
        {
            var response = new ResponseObjectForBulk()
                                .SetActionName("Delete Range")
                                .UseDefaultMessages();

            foreach (var id in ids)
            {
                try
                {
                    var entity = await Repository.GetAsync(e => e.Id == id);

                    if (entity == null)
                    {
                        response.AddFailure(id, "Entity not found");

                        continue;
                    }

                    var deleted = await Repository.DeleteAsync(entity);

                    if (deleted)
                    {
                        response.AddSuccess(id);
                        CacheService?.Remove(id);
                    }
                    else
                    {
                        response.AddFailure(id, "Deletion failed");
                    }
                }
                catch (Exception ex)
                {
                    response.AddFailure(id, $"Exception: {ex.Message}");
                    await SaveErrorLogAsync(ex.Message, id, LogProcessType.DeleteBulk);
                }
            }

            var isCommitted = EnableForCommitting ? await _unitOfWork.CommitAsync() : true;

            return response.SetSuccess(isCommitted && !response.FailedIds.Any());
        }


        public async Task<IResponseObject> DeleteRangeAsync(List<T> entities)
        {
            var response = new ResponseObject()
                    .SetActionName("Delete Range")
                    .UseDefaultMessages();

            response = await DeleteRangeAsync(entities.Select(e => e.Id).ToList());

            return response;
        }

        public virtual async Task<IResponseObject> UpdateAsync(T entity)
        {
            var updateResponse = new ResponseObjectForUpdate()
                                    .SetActionName("Update")
                                    .SetId(entity.Id)
                                    .UseDefaultMessages();

            if (!entity.IsValid(out var validationResults))
                return updateResponse.AddValidationMessages(validationResults)
                               .SetSuccess(false);

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

                if (updateResponse.Success)
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

        public virtual async Task<ResponseObjectForBulkUpdate> UpdateBulkAsync(List<T> entities)
        {
            var response = new ResponseObjectForBulkUpdate()
                                .SetActionName("Bulk Update")
                                .UseDefaultMessages();

            foreach (var entity in entities)
            {
                if (!entity.IsValid(out var validationResults))
                {
                    var messages = validationResults
                                    .Select(ResponseMessageProvider.FromValidationResult)
                                    .ToList();

                    response.AddValidationErrors(entity.Id, messages);
                    continue;
                }


                var updateResponse = await Repository.UpdateAsync(entity);

                if (updateResponse.Success)
                {
                    var isCommitted = EnableForCommitting ? await CommitAsync() : true;

                    if (isCommitted)
                    {
                        var modifiedProps = (updateResponse as ResponseObjectForUpdate)?.ModifiedProperties;

                        response.AddSuccess(entity.Id, modifiedProps.ToArray());

                        if (HasCache)
                            CacheService?.TryRefresh(entity.Id, entity);
                    }
                    else
                    {
                        response.AddValidationErrors(entity.Id, new List<ResponseObjectMessage>
                {
                    new ResponseObjectMessage("Commit Failed", $"Entity {entity.Id} commit operation failed", false)
                });
                    }
                }
                else
                {
                    response.AddValidationErrors(entity.Id, updateResponse.GetNegativeMessages);
                }
            }

            var isAllSuccessful = response.UpdatedIds.Count == entities.Count;
            response.SetSuccess(isAllSuccessful);

            if (response.Success)
                await SaveRepositoryLogsAsync();

            return response;
        }



    }
}
