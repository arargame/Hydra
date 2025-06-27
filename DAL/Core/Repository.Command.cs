using Hydra.Core;
using Hydra.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DAL.Core
{
    //COMMAND
    public partial class Repository<T> : IRepository<T> where T : BaseObject<T>
    {
        public virtual async Task<bool> CreateAsync(T entity)
        {
            try
            {
                var existingEntity = await GetExistingEntityAsync(entity, throwException: false);

                if (existingEntity != null)
                {
                    Result.AddErrorMessage("Availability", "This record already exists in the database");

                    return false;
                }

                entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
                entity.AddedDate = DateTime.Now;
                entity.ModifiedDate = DateTime.Now;

                ChangeEntityState(entity, EntityState.Added);

                if (!(entity is Log))
                {
                    Result.SetSuccess(true)
                            .Logs.Add(new Log(category: TypeName,
                             name: null,
                             description: null,
                             logType: LogType.Info,
                             entityId: entity.Id.ToString(),
                             processType: LogProcessType.Create,
                             sessionInformation: SessionInformation));
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

    }
}
