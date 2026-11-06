using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hydra.AccessManagement;
using Hydra.Core;
using Hydra.ValidationManagement;

namespace Hydra.Core.HumanResources
{
    public class Employee : BaseObject<Employee>
    {
        public Guid PositionId { get; set; }
        
        [ForeignKey("PositionId")]
        public Position? Position { get; set; }

        // Hydra SystemUser (0–1)
        public Guid? SystemUserId { get; set; }
        
        [ForeignKey("SystemUserId")]
        public SystemUser? SystemUser { get; set; }

        private bool _isActiveEmployee = true;
        public bool IsActiveEmployee
        {
            get => IsActive;
            set
            {
                _isActiveEmployee = value;
                IsActive = value;
            }
        }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (var result in base.Validate(validationContext))
                yield return result;

            if (string.IsNullOrWhiteSpace(Name))
                yield return ValidationManager.Required(nameof(Name), "Full Name");

            if (PositionId == Guid.Empty)
                yield return ValidationManager.MustBeSelected(nameof(PositionId), "Position");
        }
    }
}
