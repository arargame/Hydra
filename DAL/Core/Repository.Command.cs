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

    }
}
