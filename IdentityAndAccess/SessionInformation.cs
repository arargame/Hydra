using Hydra.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

    public interface ISessionInformation : IBaseObject<ISessionInformation>
    {
        SessionType SessionType { get; set; }

        Guid SystemUserId { get; set; }

        SystemUser? SystemUser { get; set; }

        string? Ip { get; set; }

        string? UserAgent { get; set; }

        List<Log> Logs { get; set; }

        DateTime? StartTime { get;}

        DateTime? EndTime { get; }


        DateTime? LastActivityTime { get; set; }

        Guid? GeoLocationId { get; set; }

        GeoLocation? GeoLocation { get; set; }

        ISessionInformation SetLastActiviyTime();
    }

    public class SessionInformation : BaseObject<SessionInformation>, ISessionInformation
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

        public ISessionInformation SetLastActiviyTime()
        {
            LastActivityTime = DateTime.UtcNow; 

            return this;
        }
    }

}
