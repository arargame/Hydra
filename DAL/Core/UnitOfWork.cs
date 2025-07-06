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
using System.Transactions;
using Hydra.Core;

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
            bool isCommitted = false;
            int retryCount = 3;

            using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromSeconds(30)
            }, TransactionScopeAsyncFlowOption.Enabled))
            {
                while (retryCount > 0)
                {
                    try
                    {
                        await Context.SaveChangesAsync();

                        transaction.Complete();

                        isCommitted = true;

                        break;
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        retryCount--;

                        await _logService.SaveAsync(new Log(
                            description: $"Concurrency exception. Retries left: {retryCount}. Exception: {ex.Message}",
                            logType: LogType.Warning), LogRecordType.Database);

                        ResolveConcurrencyConflicts(ex);
                    }
                    catch (Exception ex)
                    {
                        await _logService.SaveAsync(new Log(
                            description: $"Commit failed: {ex.Message}",
                            logType: LogType.Error), LogRecordType.Database);

                        break;
                    }
                }
            }

            return isCommitted;
        }


        private void ResolveConcurrencyConflicts(DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                entry.OriginalValues.SetValues(entry.GetDatabaseValues());
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
