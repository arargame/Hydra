using System;
using System.Collections.Generic;
using Hydra.Core;
using Hydra.Core.Interfaces;

namespace Hydra.Core.HumanResources
{
    public class Position : BaseObject<Position>, IHierarchicalObject<Position>
    {
        public Guid OrganizationUnitId { get; set; }
        public OrganizationUnit OrganizationUnit { get; set; } = null!;

        // Upper position (IT Manager > Software Developer)
        public Guid? ParentId { get; set; }
        public Position? Parent { get; set; }
        public List<Position> Children { get; set; } = new();

        // Employees in this position
        public List<Employee> Employees { get; set; } = new();
    }
}
