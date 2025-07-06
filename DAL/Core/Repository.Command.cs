using Hydra.Core;
using Hydra.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
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
                            .Logs.Add(LogFactory.Info(category: TypeName,
                                                         name: null,
                                                         description: null,
                                                         entityId: entity.Id.ToString(),
                                                         processType: LogProcessType.Create
                                                         ));
                }

                return true;
            }
            catch (Exception ex)
            {
                Result.Logs.Add(LogFactory.Error(ex.Message, entity.Id, LogProcessType.Create));

                return false;
            }
        }

        public virtual async Task<bool> CreateOrUpdateAsync(T entity, Expression<Func<T, bool>>? expression = null)
        {
            bool isDone = false;

            if ((expression != null && !await AnyAsync(expression)) || (expression == null && await IsItNewAsync(entity)))
            {
                var created = await CreateAsync(entity);

                isDone = created ;
            }
            else
            {
                var existing = await GetUniqueAsync(entity);

                entity.Id = existing.Id;

                isDone = (await UpdateAsync(entity)).Success;
            }

            return isDone;
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
                    response.AddModifiedProperties(modifiedProps);

                    ChangeEntityState(entity, EntityState.Modified);
                    isUpdated = true;
                }
                else
                {
                   Result.AddErrorMessage(title: "Nothing changed", text: "This record has no modified field");
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
