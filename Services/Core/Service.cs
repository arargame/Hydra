using Hydra.Core;
using Hydra.DAL.Core;
using Hydra.DI;
using Hydra.IdentityAndAccess;
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

        private readonly SessionInformation _sessionInformation;

        public SessionInformation SessionInformation { get { return _sessionInformation; } }

        private readonly IRepositoryFactoryService _repositoryFactory;

        private Lazy<ILogService>? _lazyLogService;

        protected ILogService? LogService => _lazyLogService?.Value;

        private string TypeName => typeof(T).Name;

        public bool EnableForCommitting { get; set; } = true;

        public Service(ServiceInjector injector)
        {
            _unitOfWork = injector.UnitOfWork;

            _repositoryFactory = injector.RepositoryFactory;

            _sessionInformation = injector.SessionInformation;

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
                await LogErrorAsync(ex.Message, processType: LogProcessType.Update);

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

        //public ServiceInjector GetInjector()
        //{
        //    return new ServiceInjector(UnitOfWork, SessionInformation, Configuration);
        //}

        protected async Task LogErrorAsync(string description, Guid? entityId = null, LogProcessType processType = LogProcessType.Unspecified)
        {
            if (LogService == null) return;

            var log = new Log(
                description: description,
                logType: LogType.Error,
                entityId: entityId?.ToString(),
                processType: processType,
                sessionInformation: SessionInformation,
                frameIndex: 2
            );

            await LogService.SaveAsync(log, LogRecordType.Database);
        }

        protected async Task LogInfoAsync(string description, Guid? entityId = null, LogProcessType processType = LogProcessType.Unspecified)
        {
            if (LogService == null) return;

            var log = new Log(
                description: description,
                logType: LogType.Info,
                entityId: entityId?.ToString(),
                processType: processType,
                sessionInformation: SessionInformation,
                frameIndex: 2
            );

            await LogService.SaveAsync(log, LogRecordType.Database);
        }

        protected async Task LogWarningAsync(string description, Guid? entityId = null, LogProcessType processType = LogProcessType.Unspecified)
        {
            if (LogService == null) return;

            var log = new Log(
                description: description,
                logType: LogType.Warning,
                entityId: entityId?.ToString(),
                processType: processType,
                sessionInformation: SessionInformation,
                frameIndex: 2
            );

            await LogService.SaveAsync(log, LogRecordType.Database);
        }

        public void SetCacheService(ICacheService<Guid, T> service)
        {
            CacheService = service;
        }

        public virtual void SetRepository(IRepository<T>? repository = null)
        {
            Repository = repository ?? _repositoryFactory.CreateRepository<T>(_unitOfWork.Context, _sessionInformation);
        }

        //protected ServiceInjector GetInjector()
        //{
        //    return new ServiceInjector(_unitOfWork, _repositoryFactory, SessionInformation, Configuration, serviceProvider);
        //}
    }
}
