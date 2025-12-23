using System;
using Hydra.Core;
using Hydra.AccessManagement;

using System.ComponentModel.DataAnnotations.Schema;

namespace Hydra.Core.HumanResources
{
    public class Employee : BaseObject<Employee>
    {
        public Guid PositionId { get; set; }
        
        [ForeignKey("PositionId")]
        public Position Position { get; set; } = null!;

        // Hydra SystemUser (0–1)
        public Guid? SystemUserId { get; set; }
        
        [ForeignKey("SystemUserId")]
        public SystemUser? SystemUser { get; set; }

        public bool IsActiveEmployee { get; set; } = true;
    }
}
