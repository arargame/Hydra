using Hydra.Core;
using Hydra.DI;
using Hydra.IdentityAndAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Hydra.DAL
{
    public interface IUnitOfWork : IDisposable
    {
        bool Commit();
    }
    public class UnitOfWork : IUnitOfWork
    {
        private bool _disposed;
        public DbContext Context { get; }
        private readonly ILogger _logger;

        public UnitOfWork(DbContext context, ILogger logger)
        {
            Context = context;
            _logger = logger;
        }

        public IRepository<T> LoadRepository<T>(SessionInformation sessionInformation) where T : BaseObject<T>
        {
            return new Repository<T>(new RepositoryInjector(Context, sessionInformation));
        }

        public bool Commit()
        {
            bool isCommitted = false;
            int retryCount = 3;

            using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromSeconds(30)
            }))
            {
                while (retryCount > 0)
                {
                    try
                    {
                        Context.SaveChanges();
                        transaction.Complete();
                        isCommitted = true;
                        break;
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        retryCount--;
                        _logger.LogWarning($"Concurrency exception. Retries left: {retryCount}. Exception: {ex.Message}");
                        ResolveConcurrencyConflicts(ex);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Commit failed: {ex.Message}");
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
