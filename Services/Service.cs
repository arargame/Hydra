using Hydra.Core;
using Hydra.DAL;
using Hydra.DI;
using Hydra.IdentityAndAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services
{
    public interface IService<T> where T : IBaseObject<T>
    {
        //IUnitOfWork UnitOfWork { get; set; }

        IRepository<T>? Repository { get; set; }

        bool HasCache { get; set; }

        bool Create(T entity);

        bool Commit();
    }


    public class Service<T> : IService<T> where T : BaseObject<T>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly SessionInformation _sessionInformation;

        private readonly IRepositoryFactoryService _repositoryFactory;

        public bool HasCache { get; set; }

        public IRepository<T>? Repository { get; set; }

        public Service(ServiceInjector injector)
        {
            _unitOfWork = injector.UnitOfWork;

            _repositoryFactory = injector.RepositoryFactory;

            _sessionInformation = injector.SessionInformation;

            SetRepository();
        }

        public bool Commit()
        {
            return false;
        }

        public virtual void SetRepository(IRepository<T>? repository = null)
        {
            Repository = repository ?? _repositoryFactory.CreateRepository<T>(_unitOfWork.Context,_sessionInformation);
        }

        public virtual bool Create(T entity)
        {
            //var isCommitted = Repository.Create(entity) && Commit();

            //try
            //{
            //    if (isCommitted)
            //    {
            //        var log = Repository.ConsumeLogs().Where(l => l.ProcessType == LogProcessType.Create).SingleOrDefault();

            //        //LogManager.Save(log);

            //        if (log == null)
            //            return isCommitted;

            //        var logRepository = new LogRepository(Repository.GetInjector());

            //        logRepository.Create(log);

            //        Commit();

            //        if (HasCache)
            //            Cache<T>.AddObject(entity);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LogManager.Save(new Log(ex.Message, entityId: entity.UniqueProperty));
            //}

            //return isCommitted;

            return false;
        }
    }
}
