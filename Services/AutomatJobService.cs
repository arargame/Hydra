using Hydra.Background;
using System.Timers;
using SystemTimer = System.Timers.Timer;

namespace Hydra.Services
{
    public class AutomatJobService
    {
        public static List<AutomatJob> Jobs = new List<AutomatJob>();

        //private readonly List<SystemTimer> _jobTimers = new List<SystemTimer>();


        public AutomatJobService()
        {
            // Servis yapılandırması
        }

        // Static job yaratma fonksiyonu
        public static AutomatJob CreateAutomatJob(string name,
                                                    string code = "",
                                                    bool isContinuous = true,
                                                    bool isPersistent = false)
        {
            var automatJob = new AutomatJob(name, code, isContinuous);

            if (!isPersistent)
                automatJob.MakeNonPersistent();

            Jobs.Add(automatJob);

            return automatJob;
        }

        // Job'ları başlatma fonksiyonu
        public void Start()
        {
            //Load();

            //var timer = new SystemTimer(1000);
            //timer.Elapsed += Timer_Elapsed;
            //timer.Start();

            foreach (var job in Jobs)
                {
                //    var timer = new SystemTimer(1000);
                //    timer.Elapsed += Timer_Elapsed;
                //    timer.Start();

                //var timer = new Timer(state =>
                //{
                //    Timer_Elapsed(job,);
                //}, null, 0, (int)job.Interval,);

                //    _jobTimers.Add(timer);

                job.Start();
            }
        }

        // Timer olayı tetiklendiğinde çalışacak olan fonksiyon
        //private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    foreach (var job in Jobs)
        //    {
        //        //if (job.OnlyWorkOnProduction && !DAL.GedenLines.Configurations.ConfigurationManager.IsEnvironmentProduction)
        //        //    continue;

        //        Task.Run(() => job.Update());
        //    }
        //}

        //// Job'ları yükleme fonksiyonu (Persistent olanları)
        //public AutomatJobService Load()
        //{
        //    var persistentJobList = AutomatJobs.Where(aj => aj.IsPersistent).ToList();
        //    var automatService = new AutomatService(GetInjector());
        //    var systemUserService = new SystemUserService(GetInjector());

        //    foreach (var persistentJob in persistentJobList)
        //    {
        //        HandleSystemUserForJob(persistentJob, systemUserService);
        //        HandleAutomatForJob(persistentJob, automatService);
        //    }

        //    AutomatJobs.AddRange(SelectWithLinq(aj => !persistentJobList.Select(p => p.Id).Contains(aj.Id)));
        //    return this;
        //}

        // SystemUser kontrol ve oluşturma işlemi
        //private void HandleSystemUserForJob(AutomatJob persistentJob, SystemUserService systemUserService)
        //{
        //    var systemUser = persistentJob.Automat.SystemUser ?? persistentJob.Automat.CreateSystemUser();
        //    var existingSystemUser = systemUserService.Get(su => su.ErpId == systemUser.ErpId && su.Email == systemUser.Email && su.Name == systemUser.Name);

        //    if (existingSystemUser == null)
        //    {
        //        systemUserService.Create(systemUser);
        //        persistentJob.Automat.SystemUserId = systemUser.Id;
        //    }
        //    else
        //    {
        //        persistentJob.Automat.SystemUserId = existingSystemUser.Id;
        //    }
        //}

        //// Automat objesinin kontrol ve güncellemesi
        //private void HandleAutomatForJob(AutomatJob persistentJob, AutomatService automatService)
        //{
        //    var existingAutomat = automatService.Get(a => a.Code == persistentJob.Automat.Code);

        //    if (existingAutomat != null)
        //    {
        //        existingAutomat.SystemUserId = persistentJob.Automat.SystemUserId;
        //        var taskForAutomat = automatService.CreateOrUpdate(existingAutomat);
        //        persistentJob.AutomatId = existingAutomat.Id;
        //    }
        //}
    }

}
