using Hydra.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using SystemTimer = System.Timers.Timer;

namespace Hydra.Background
{
    public enum AutomatJobState
    {
        Failed,
        Running,
        Stopped,
        Successful,
        WaitingForParentToComplete,
        WaitingForChildrenToComplete,
        WaitingToRun
    }

    public class AutomatJob : BaseObject<AutomatJob>
    {
        private SystemTimer Timer { get; set; } = null!;

        public Guid AutomatId { get; set; }

        [ForeignKey("AutomatId")]
        public Automat? Automat { get; set; } = null;

        public string? Code { get; set; } = null;

        public Guid? ParentId { get; set; }

        public AutomatJob? Parent { get; set; } = null;

        public Guid? ContinueWithId { get; set; }

        [ForeignKey("ContinueWithId")]
        public AutomatJob? ContinueWith { get; set; } = null;


        [NotMapped]
        public Func<AutomatJob,Task<bool>>? Function { get; set; } = null;

        [NotMapped]
        public Action<Exception>? ActionWhenFailure { get; set; } = null;
        public string? Path { get; set; } = null;

        public string? FunctionName { get; set; } = null;

        public double Interval { get; set; }

        public DateTime? LastWorkDate { get; set; } = null;
        public DateTime? NextWorkDate { get; set; }= null;

        public bool IsContinuous { get; set; }
        public AutomatJobState State { get; set; }
        public int NumberOfWorking { get; set; }

        [NotMapped]
        public bool OnlyWorkOnProduction { get; set; }

        public bool IsCompleted
        {
            get
            {
                return State == AutomatJobState.Successful || State == AutomatJobState.Failed;
            }
        }

        public AutomatJob()
        {

        }

        public AutomatJob(string name, string code)
        {
            Name = name;

            SetCode(code);
        }

        public override void Initialize()
        {
            base.Initialize();

            IsContinuous = true;

            var interval = 100000;

            Timer = new SystemTimer(interval: interval);
            Timer.Elapsed += Timer_Elapsed;

            SetInterval(interval);

            State = AutomatJobState.WaitingToRun;

            NextWorkDate = DateTime.Now.AddMilliseconds(Interval);
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Task.Run(() => Update());
        }

        public void Start()
        {
            Timer.Start();
        }

        public virtual async Task<AutomatJob> Update()
        {
            if (IsCompleted 
                && (ContinueWith == null || (ContinueWith != null && ContinueWith.IsCompleted))
                && IsContinuous)
                SetState(AutomatJobState.WaitingToRun);

            switch (State)
            {
                case AutomatJobState.Failed:
                    break;

                case AutomatJobState.Running:
                    break;

                case AutomatJobState.Stopped:

                    break;

                case AutomatJobState.Successful:

                    if (ContinueWith == null)
                        break;

                    if (!ContinueWith.IsCompleted)
                    {
                        SetState(AutomatJobState.WaitingForChildrenToComplete);
                    }

                    break;

                case AutomatJobState.WaitingForParentToComplete:

                    if (Parent != null && (Parent.IsCompleted || Parent.State == AutomatJobState.WaitingForChildrenToComplete))
                        SetState(AutomatJobState.WaitingToRun);

                    break;

                case AutomatJobState.WaitingForChildrenToComplete:

                    if (ContinueWith == null)
                        break;

                    //if (ContinueWith.State == AutomatJobState.Stopped)
                    //    ContinueWith.SetState(AutomatJobState.WaitingToRun);

                    if (ContinueWith.IsCompleted)
                        SetState(AutomatJobState.Successful);

                    break;

                case AutomatJobState.WaitingToRun:

                    await Invoke(ActionWhenFailure);

                    break;

                default:
                    break;
            }


            return this;
        }

        public AutomatJob SetContinuous(bool enable = true)
        {
            IsContinuous = enable;

            return this;
        }

        public AutomatJob SetContinueWith(AutomatJob continueWith)
        {
            ContinueWith = continueWith;

            continueWith.SetParent(this);

            //ContinueWith.SetInterval(new TimeSpan((long)Interval * 10000));

            return this;
        }

        public AutomatJob SetParent(AutomatJob parent)
        {
            Parent = parent;

            SetState(AutomatJobState.WaitingForParentToComplete);

            return this;
        }

        public AutomatJob SetInterval(int days = 0, int hours = 0, int minutes = 0, int seconds = 0, int miliseconds = 0)
        {
            return SetInterval(new TimeSpan(days, hours, minutes, seconds, miliseconds));
        }

        public AutomatJob SetInterval(TimeSpan timeSpan)
        {
            return SetInterval(timeSpan.TotalMilliseconds);
        }

        public AutomatJob SetInterval(double interval)
        {
            Interval = interval;
            Timer.Interval = interval;

            NextWorkDate = DateTime.Now.AddMilliseconds(Interval);

            return this;
        }

        public AutomatJob SetNumberOfWorking(int numberOfWorking)
        {
            NumberOfWorking = numberOfWorking;

            return this;
        }

        public AutomatJob SetState(AutomatJobState state)
        {
            State = state;

            return this;
        }

        public AutomatJob SetPathAndFunctionName(string path, string functionName)
        {
            Path = path;

            FunctionName = functionName;

            return this;
        }

        //public AutomatJob SetAction(Action action)
        //{
        //    SetFunction(async () =>
        //    {
        //        await Task.Run(() => action());

        //        return true;
        //    });

        //    return this;
        //}

        public AutomatJob SetFunction(Func<AutomatJob,Task<bool>> function)
        {
            Function = function;

            return this;
        }

        public AutomatJob SetActionWhenFailure(Action<Exception> action)
        {
            ActionWhenFailure = action;

            return this;
        }

        public AutomatJob OnlyWorkOnProductionMode()
        {
            OnlyWorkOnProduction = true;

            return this;
        }

        public AutomatJob SetCode(string code)
        {
            Code = code;

            return this;
        }


        private object AutomatJobActionLockObject = new object();

        public async Task Invoke(Action<Exception>? logAction = null)
        {
            if (NextWorkDate == null)
                return;

            if (NextWorkDate.Value < DateTime.Now)
                NextWorkDate = DateTime.Now;

            var hour = NextWorkDate.Value.Hour;
            var minute = NextWorkDate.Value.Minute;
            var second = NextWorkDate.Value.Second;

            var tempHour = DateTime.Now.Hour;
            var tempMinute = DateTime.Now.Minute;
            var tempSecond = DateTime.Now.Second;

            var isEqual = hour == tempHour && minute == tempMinute && second == tempSecond;

            if (isEqual && Function != null)
            {
                try
                {
                    SetState(AutomatJobState.Running);

                    lock (AutomatJobActionLockObject)
                    {
                        var task = Function.Invoke(this);

                        task.Wait();

                        var isDone = task.Result;

                        if (isDone)
                        {
                            SetState(AutomatJobState.Successful);

                            LastWorkDate = DateTime.Now;

                            NextWorkDate = DateTime.Now.AddMilliseconds(Interval);

                            SetNumberOfWorking(NumberOfWorking++);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (logAction != null)
                        logAction(ex);

                    SetState(AutomatJobState.Failed);
                }
            }

            await Task.FromResult(0);
        }

        public override string ToString()
        {
            return $"[{Name}][{Code}] / {State} / ParentState : {Parent?.State} / ContunieWithState : {ContinueWith?.State} / Interval : {Interval} / LastWorkDate : {LastWorkDate} / [NextWorkDate : {NextWorkDate}]";
        }

    }
}
