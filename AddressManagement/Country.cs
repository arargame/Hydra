using Hydra.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.AddressManagement
{
    public enum Continent
    {
        Africa,
        Antarctica,
        Asia,
        Europe,
        NorthAmerica,
        Oceania,  // Or Australia, depending on your preference
        SouthAmerica
    }

    public class Country : BaseObject<Country>
    {
        public Continent Continent { get; set; }

        public string? Code { get; set; } = null;
        public string? Abbreviation { get; set; } = null;

        public Country()
        {

        }
    }
}
