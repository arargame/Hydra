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
        private readonly IUnitOfWork UnitOfWork;

        public readonly SessionInformation SessionInformation;

        private readonly IRepositoryFactoryService RepositoryFactory;

        public IRepository<T>? Repository { get; set; }

        public Service(IUnitOfWork unitOfWork, IRepositoryFactoryService repositoryFactory,SessionInformation sessionInformation)
        {
            UnitOfWork = unitOfWork;

            SessionInformation = sessionInformation;

            RepositoryFactory = repositoryFactory;

            SetRepository();
        }

        public bool Commit()
        {
            return false;
        }

        public virtual void SetRepository(IRepository<T>? repository = null)
        {
            Repository = repository ?? RepositoryFactory.CreateRepository<T>(UnitOfWork.Context,SessionInformation);
        }
    }
}
