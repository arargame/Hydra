using System;
using Hydra.Core;
using Hydra.AccessManagement;

namespace Hydra.Core.HumanResources
{
    public class Employee : BaseObject<Employee>
    {
        public Guid PositionId { get; set; }
        public Position Position { get; set; } = null!;

        // Hydra SystemUser (0–1)
        public Guid? SystemUserId { get; set; }
        public SystemUser? SystemUser { get; set; }

        public bool IsActiveEmployee { get; set; } = true;
    }
}
