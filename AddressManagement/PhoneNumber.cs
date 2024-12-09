using Hydra.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.AddressManagement
{
    public enum PhoneNumberType
    {
        Cell,
        Home,
        Work,
        Fax,    
        Other   
    }

    public class PhoneNumber : BaseObject<PhoneNumber>
    {
        public string? CountryCode { get; set; } = null;
        public string? Number { get; set; } = null;
        public PhoneNumberType Type { get; set; }

        [NotMapped]
        public string FullNumber
        {
            get
            {
                return $"{CountryCode} {Number}";
            }
        }

        public override string UniqueProperty
        {
            get
            {
                return FullNumber;
            }
        }
        public PhoneNumber() { }
    }
}
