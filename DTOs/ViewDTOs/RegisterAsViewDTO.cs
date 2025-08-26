using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs.ViewDTOs
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegisterAsViewDTO : Attribute
    {
        public string TableName { get; }

        public RegisterAsViewDTO(string tableName)
        {
            TableName = tableName;
        }
    }

}
