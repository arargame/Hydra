using Hydra.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.IdentityAndAccess
{
    public enum SessionType
    {
        Web,        
        Mobile,     
        Desktop,    
        Tablet,     
        Other       
    }

    public class SessionInformation : BaseObject<SessionInformation>
    {
        public SessionType SessionType { get; set; }

        public Guid SystemUserId { get; set; }

        [ForeignKey("SystemUserId")]
        public SystemUser? SystemUser { get; set; } = null;

        public string? Ip { get; set; } = null;
        public string? UserAgent { get; set; } = null;
        public List<Log> Logs { get; set; } = new List<Log>();

        [NotMapped]
        public bool IsAuthenticated { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public Guid? GeoLocationId { get; set; } 

        [ForeignKey("GeoLocationId")]
        public GeoLocation? GeoLocation { get; set; }

        public SessionInformation() 
        {
        
        }
    }

}
