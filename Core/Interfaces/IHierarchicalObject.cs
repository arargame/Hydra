using System;
using System.Collections.Generic;

namespace Hydra.Core.Interfaces
{
    public interface IHierarchicalObject<T>
    {
        Guid? ParentId { get; set; }
        T? Parent { get; set; }
        List<T> Children { get; set; }
    }
}
