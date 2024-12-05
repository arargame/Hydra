using Hydra.Core;
using Hydra.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Background
{
    public class Automat : BaseObject<Automat>
    {
        public string? Code { get; set; } = null;


        //public Guid? SystemUserId { get; set; }

        //[ForeignKey("SystemUserId")]
        //public SystemUser SystemUser { get; set; }

        public ICollection<AutomatJob> Jobs { get; set; }

        [NotMapped]
        public List<AutomatJob>? GetUncompletedJobs
        {
            get
            {
                return Jobs?.Where(j => !j.IsCompleted)?.ToList() ?? null;
            }
        }

        public Automat()
        {
            Jobs = new List<AutomatJob>();
        }

        public Automat(string name)
        {
            Name = name;

            Jobs = new List<AutomatJob>();
        }

        public Automat AddJob(AutomatJob job)
        {
            job.Automat = this;

            Jobs.Add(job);

            return this;
        }

        public Automat SetCode(string code)
        {
            Code = code;

            return this;
        }

        //public SystemUser CreateSystemUser()
        //{
        //    return new SystemUser()
        //    {
        //        Name = $"Automat_{Code}",
        //        Email = "automat@gedenlines.com",
        //        ErpPassword = Helper.GetRandomAlphanumericString(6),
        //        ErpId = -77777,
        //        IsForAutomat = true
        //    };
        //}
    }
}
