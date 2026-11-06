using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hydra.Core;
using Hydra.Core.Interfaces;
using Hydra.ValidationManagement;

namespace Hydra.Core.HumanResources
{
    public class Position : BaseObject<Position>, IHierarchicalObject<Position>
    {
        public Guid OrganizationUnitId { get; set; }
        
        [ForeignKey("OrganizationUnitId")]
        public OrganizationUnit? OrganizationUnit { get; set; }

        // Upper position (IT Manager > Software Developer)
        public Guid? ParentId { get; set; }
        
        [ForeignKey("ParentId")]
        public Position? Parent { get; set; }
        public List<Position> Children { get; set; } = new();

        // Employees in this position
        public List<Employee> Employees { get; set; } = new();

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (var result in base.Validate(validationContext))
                yield return result;

            if (string.IsNullOrWhiteSpace(Name))
                yield return ValidationManager.Required(nameof(Name), "Position Title");

            if (OrganizationUnitId == Guid.Empty)
                yield return ValidationManager.MustBeSelected(nameof(OrganizationUnitId), "Organization Unit");
        }
    }
}
