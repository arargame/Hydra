using Hydra.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Background
{
    public class JobBuilder
    {
        private AutomatJob _job;

        public JobBuilder(string name,
                        string code = "",
                        bool isContinuous = true,
                        bool isPersistent = false)
        {
            _job = AutomatJobService.CreateAutomatJob(name: name,
                                                    code: code,
                                                    isContinuous: isContinuous,
                                                    isPersistent: isPersistent);
        }

        public JobBuilder SetInterval(TimeSpan interval)
        {
            _job.SetInterval(interval);
            return this;
        }

        public JobBuilder SetInterval(int days = 0, int hours = 0, int minutes = 0, int seconds = 0, int miliseconds = 0)
        {
            _job.SetInterval(new TimeSpan(days, hours, minutes, seconds, miliseconds));

            return this;
        }

        public JobBuilder SetContinuous(bool isContinuous)
        {
            _job.SetContinuous(isContinuous);
            return this;
        }

        public JobBuilder OnlyWorkOnProductionMode()
        {
            _job.OnlyWorkOnProduction = true;
            return this;
        }

        public JobBuilder SetFunction(Func<AutomatJob,Task<bool>> function)
        {
            _job.SetFunction(function);
            return this;
        }

        public JobBuilder SetActionWhenFailure(Action<Exception> action)
        {
            _job.SetActionWhenFailure(action);
            return this;
        }

        public AutomatJob Build()
        {
            return _job;
        }
    }

}
