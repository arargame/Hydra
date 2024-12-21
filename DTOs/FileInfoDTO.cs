using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs
{
    public class FileInfoDTO
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public string Extension { get; set; }

        public FileInfoDTO() { }

        public FileInfoDTO(Guid id, string name, string extension)
        {
            Id = id;

            Name = name;

            Extension = extension;
        }
    }
}
