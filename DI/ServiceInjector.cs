using Hydra.DTOs.ViewConfigurations;
using Hydra.IdentityAndAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DI
{
    public class ServiceInjector : Injector
    {
        public UnitOfWork UnitOfWork { get; set; }

        public SessionInformation SessionInformation { get; set; }

        public IConfiguration Configuration { get; set; }

        public ServiceInjector(UnitOfWork unitOfWork, SessionInformation sessionInformation, IConfiguration configuration)
        {
            UnitOfWork = unitOfWork;

            SessionInformation = sessionInformation;

            Configuration = configuration;
        }

        public ServiceInjector(UnitOfWork unitOfWork) : this(unitOfWork, null, null)
        {

        }
    }
}
