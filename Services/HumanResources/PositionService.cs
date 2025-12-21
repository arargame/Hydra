using Hydra.Services.Core;
using Hydra.DI;
using Hydra.Core.HumanResources;

namespace Hydra.Services.HumanResources
{
    [RegisterAsService(typeof(IService<Position>))]
    public class PositionService : Service<Position>
    {
        public PositionService(ServiceInjector injector) : base(injector)
        {
        }
    }
}
