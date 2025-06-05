using Hydra.Core;
using Hydra.DAL;
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

        bool Commit();
    }


    public class Service<T> : IService<T> where T : BaseObject<T>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ISessionInformation _sessionInformation;

        private readonly IRepositoryFactoryService _repositoryFactory;

        public IRepository<T>? Repository { get; set; }

        public Service(IUnitOfWork unitOfWork,
                IRepositoryFactoryService repositoryFactory,
                ISessionInformation sessionInformation)
        {
            _unitOfWork = unitOfWork;

            _repositoryFactory = repositoryFactory;

            _sessionInformation = sessionInformation;

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
    }
}
