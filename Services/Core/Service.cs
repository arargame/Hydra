using Hydra.Core;
using Hydra.DAL;
using Hydra.DataModels;
using Hydra.DataModels.Filter;
using Hydra.DI;
using Hydra.DTOs;
using Hydra.DTOs.ViewConfigurations;
using Hydra.DTOs.ViewDTOs;
using Hydra.Http;
using Hydra.IdentityAndAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Hydra.DataModels.SortingFilterDirectionExtension;

namespace Hydra.Services.Core
{


        public partial class Service<T> : IService<T> where T : BaseObject<T>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly SessionInformation _sessionInformation;

        private readonly IRepositoryFactoryService _repositoryFactory;

        private string TypeName => typeof(T).Name;

        public bool HasCache { get; set; }

        protected IRepository<T> Repository { get; set; } = null!;

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

        protected ServiceInjector GetInjector()
        {
            return new ServiceInjector(_unitOfWork,_repositoryFactory, SessionInformation, Configuration,serviceProvider);
        }
    }
}
