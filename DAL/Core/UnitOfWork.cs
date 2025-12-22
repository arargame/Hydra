using Hydra.DI;
using Hydra.AccessManagement;
using Hydra.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hydra.Core;
using Hydra.Utils;

namespace Hydra.DAL.Core
{
    public interface IUnitOfWork : IDisposable
    {
        DbContext Context { get; }
        Task<bool> CommitAsync();
    }
    public class UnitOfWork : IUnitOfWork
    {
        private bool _disposed;
        public DbContext Context { get; }
        private readonly ILogService _logService;

        public UnitOfWork(DbContext context, ILogService logService)
        {
            Context = context;
            _logService = logService;
        }

        //public IRepository<T> LoadRepository<T>(SessionInformation sessionInformation) where T : BaseObject<T>
        //{
        //    return new Repository<T>(new RepositoryInjector(Context, sessionInformation));
        //}

        public async Task<bool> CommitAsync()
        {
            int retryCount = 3;

            while (retryCount > 0)
            {
                await using var transaction = await Context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.ReadCommitted);

                try
                {
                    await Context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    retryCount--;

                    await _logService.SaveAsync(
                        LogFactory.Warning(
                            category: nameof(UnitOfWork),
                            name: nameof(CommitAsync),
                            description: $"Concurrency conflict. Retries left: {retryCount}. Exception: {ex.Message}"),
                        LogRecordType.Database);

                    ResolveConcurrencyConflicts(ex);
                    await transaction.RollbackAsync();

                    if (retryCount == 0)
                        return false;
                }
                catch (Exception ex)
                {
                    var friendlyMessage = SqlExceptionHelper.ToUserFriendlyMessage(ex);

                    await _logService.SaveAsync(LogFactory.Error(description: $"Commit failed: {friendlyMessage}"),
                                                LogRecordType.Database);

                    await transaction.RollbackAsync();
                    return false;
                }
            }

            return false;
        }


        private void ResolveConcurrencyConflicts(DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                // Get current database values
                var databaseValues = entry.GetDatabaseValues();

                if (databaseValues == null)
                {
                    // Entity has been deleted from database
                    entry.State = EntityState.Detached;
                    continue;
                }

                // Client Wins strategy: Update OriginalValues to match database
                // CurrentValues (client changes) remain unchanged
                entry.OriginalValues.SetValues(databaseValues);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Context?.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

}
