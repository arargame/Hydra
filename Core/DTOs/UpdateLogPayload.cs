using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Core.DTOs
{
    public class EntityChangeSet
    {
        public string Property { get; set; } = default!;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
    }

    public class UpdateLogPayload
    {
        public string EntityName { get; set; } = default!;
        public string EntityId { get; set; } = default!;
        public List<EntityChangeSet> Changes { get; set; } = new();
    }
}
