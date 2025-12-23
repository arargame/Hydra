using System;
using System.Collections.Generic;
using Hydra.Core;
using Hydra.Core.Interfaces;

using System.ComponentModel.DataAnnotations.Schema;

namespace Hydra.Core.HumanResources
{
    public class OrganizationUnit : BaseObject<OrganizationUnit>, IHierarchicalObject<OrganizationUnit>
    {
        public Guid? ParentId { get; set; }
        
        [ForeignKey("ParentId")]
        public OrganizationUnit? Parent { get; set; }
        public List<OrganizationUnit> Children { get; set; } = new();

        // Unit positions
        public List<Position> Positions { get; set; } = new();

        // Unit manager (via position)
        public Guid? ManagerPositionId { get; set; }
        
        [ForeignKey("ManagerPositionId")]
        public Position? ManagerPosition { get; set; }
    }
}
