using Hydra.Services.Core;
using Hydra.DI;
using Hydra.Core.HumanResources;

namespace Hydra.Services.HumanResources
{
    [RegisterAsService(typeof(IService<Employee>))]
    public class EmployeeService : Service<Employee>
    {
        public EmployeeService(ServiceInjector injector) : base(injector)
        {
        }
    }
}
