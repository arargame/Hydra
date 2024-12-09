using Hydra.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.AddressManagement
{
    public class Address : BaseObject<Address>
    {
        public Continent Continent { get; set; }
        public string? Country { get; set; } = null;

        public string? City { get; set; } = null;

        public string? State { get; set; } = null;

        public string? Street { get; set; } = null;

        public string? PostalCode { get; set; } = null;

        public override string UniqueProperty
        {
            get
            {
                return $"{Street}, {City}, {Country}";
            }
        }

        [NotMapped]
        public string FullAddress
        {
            get
            {
                return $"{Street}, {City}, {State}, {PostalCode}, {Country}";
            }
        }

        public Address()
        {

        }
    }
}
