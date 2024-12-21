using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs
{
    public interface IDTO
    {
        Guid Id { get; set; }
        string? Name { get; set; }
    }

    public abstract class DTO : IDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; } = null;

        public DTO() { }
    }
}
