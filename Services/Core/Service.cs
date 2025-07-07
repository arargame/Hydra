﻿using Hydra.AccessManagement;
using Hydra.Core;
using Hydra.DAL.Core;
using Hydra.DI;
using Hydra.Http;
using Hydra.Services.Cache;
using System.Linq.Expressions;

namespace Hydra.Services.Core
{
    public partial class Service<T> : IService<T> where T : BaseObject<T>
    {
        private readonly IUnitOfWork _unitOfWork;

        protected IRepository<T> Repository { get; set; } = null!;

        public ICacheService<Guid, T>? CacheService { get; set; }

        public bool HasCache => CacheService != null;

        //private readonly SessionInformation _sessionInformation;

        //public SessionInformation SessionInformation { get { return _sessionInformation; } }

        private readonly IRepositoryFactoryService _repositoryFactory;

        private Lazy<ILogService>? _lazyLogService;

        protected ILogService? LogService => _lazyLogService?.Value;

        private string TypeName => typeof(T).Name;

        public bool EnableForCommitting { get; set; } = true;


        public Service(ServiceInjector injector)
        {
            _unitOfWork = injector.UnitOfWork;

            _repositoryFactory = injector.RepositoryFactory;

            //_sessionInformation = injector.SessionInformation;

            SetRepository();

            _lazyLogService = injector.ResolveLazy<ILogService>()!;
        }


        public async Task<bool> CommitAsync()
        {
            try
            {
                if (!EnableForCommitting)
                    return true;

                return await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await SaveErrorLogAsync(ex.Message, processType: LogProcessType.Update);

                return false;
            }
        }

        public IService<T> DisableToCommit()
        {
            EnableForCommitting = false;

            return this;
        }

        public IService<T> EnableToCommit()
        {
            EnableForCommitting = true;

            return this;
        }

        public async Task<T?> GetOrSelectThenCacheAsync(Guid id)
        {
            if (CacheService?.TryGet(id, out var cached) == true)
                return cached;

            return (await SelectThenCache(e => e.Id == id)).FirstOrDefault();
        }

        public List<ResponseObjectMessage> GetRepositoryMessages() => Repository.Result.Messages;


        protected async Task SaveRepositoryLogsAsync()
        {
            if (LogService == null)
                throw new NullReferenceException(nameof(LogService));

            var logs = Repository.Result.ConsumeLogs();

            foreach (var log in logs)
            {
                await LogService.SaveAsync(log, LogRecordType.Database);
            }
        }

        protected async Task SaveErrorLogAsync(string description, Guid? entityId = null, LogProcessType processType = LogProcessType.Unspecified)
        {
            if (LogService == null) return;

            var log = LogFactory.Error(description,entityId?.ToString(),processType);
            
            await LogService.SaveAsync(log, LogRecordType.Database);
        }

        protected async Task SaveInfoLogAsync(string? category, string? name, string description, Guid? entityId = null, LogProcessType processType = LogProcessType.Unspecified)
        {
            if (LogService == null) return;

            var log = LogFactory.Info(category: category,
                name: name,
                description: description,
                entityId: entityId?.ToString(),
                processType: processType,
                sessionInformation: null);

            await LogService.SaveAsync(log, LogRecordType.Database);
        }

        protected async Task SaveWarningLogAsync(string? category, string? name, string description, Guid? entityId = null, LogProcessType processType = LogProcessType.Unspecified)
        {
            if (LogService == null) return;

            var log = LogFactory.Info(category, name, description, entityId?.ToString(), processType);

            log.SetLogType(LogType.Warning);

            await LogService.SaveAsync(log, LogRecordType.Database);
        }

        public void SetCacheService(ICacheService<Guid, T> service)
        {
            CacheService = service;
        }

        public virtual void SetRepository(IRepository<T>? repository = null, params object[] additionalParams)
        {
            Repository = repository ?? _repositoryFactory.CreateRepository<T>(_unitOfWork.Context, additionalParams);
        }
    }
}
