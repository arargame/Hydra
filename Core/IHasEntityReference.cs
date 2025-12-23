using System;

namespace Hydra.Core
{
    public interface IHasEntityReference
    {
        string? EntityId { get; set; }
        string? EntityType { get; set; }
    }
}
