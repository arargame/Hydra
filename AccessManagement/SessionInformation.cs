using Hydra.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.AccessManagement
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

        [MaxLength(45)]
        public string? Ip { get; set; } = null;

        [MaxLength(512)]
        public string? UserAgent { get; set; } = null;
        public List<Log> Logs { get; set; } = new List<Log>();

        [NotMapped]
        public bool IsAuthenticated { get; set; }

        [NotMapped]
        public DateTime? StartTime => Logs
                 .Where(x => x.ProcessType == LogProcessType.Login)
                 .OrderBy(x => x.AddedDate)
                 .Select(x => (DateTime?)x.AddedDate)
                 .FirstOrDefault();

        [NotMapped]
        public DateTime? EndTime => Logs
            .Where(x => x.ProcessType == LogProcessType.Logout)
            .OrderByDescending(x => x.AddedDate)
            .Select(x => (DateTime?)x.AddedDate)
            .FirstOrDefault();

        public DateTime? LastActivityTime { get; set; }

        public Guid? GeoLocationId { get; set; } 

        [ForeignKey("GeoLocationId")]
        public GeoLocation? GeoLocation { get; set; }

        public SessionInformation() 
        {
        
        }

        public SessionInformation SetLastActiviyTime()
        {
            LastActivityTime = DateTime.UtcNow; 

            return this;
        }
    }

}
