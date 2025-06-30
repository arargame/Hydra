using Hydra.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.IdentityAndAccess
{
    public class GeoLocation : BaseObject<GeoLocation>
    {
        public string? City { get; set; } = null;
        public string? Region { get; set; } = null;
        public string? Country { get; set; } = null;        
        public double Latitude { get; set; }       
        public double Longitude { get; set; }

        public List<SessionInformation> SessionInformations { get; set; } = new List<SessionInformation>();

        public GeoLocation() { }
    }
}
