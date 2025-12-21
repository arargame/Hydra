using Hydra.Services.Core;
using Hydra.DI;
using Hydra.Core.HumanResources;

namespace Hydra.Services.HumanResources
{
    [RegisterAsService(typeof(IService<OrganizationUnit>))]
    public class OrganizationUnitService : Service<OrganizationUnit>
    {
        public OrganizationUnitService(ServiceInjector injector) : base(injector)
        {
        }
    }
}
