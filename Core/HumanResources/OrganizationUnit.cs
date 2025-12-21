using System;
using System.Collections.Generic;
using Hydra.Core;
using Hydra.Core.Interfaces;

namespace Hydra.Core.HumanResources
{
    public class OrganizationUnit : BaseObject<OrganizationUnit>, IHierarchicalObject<OrganizationUnit>
    {
        public Guid? ParentId { get; set; }
        public OrganizationUnit? Parent { get; set; }
        public List<OrganizationUnit> Children { get; set; } = new();

        // Unit positions
        public List<Position> Positions { get; set; } = new();

        // Unit manager (via position)
        public Guid? ManagerPositionId { get; set; }
        public Position? ManagerPosition { get; set; }
    }
}
