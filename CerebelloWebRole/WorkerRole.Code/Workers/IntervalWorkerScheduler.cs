using System;
using System.Threading;
using System.Threading.Tasks;

namespace CerebelloWebRole.WorkerRole.Code.Workers
{
    public class IntervalWorkerScheduler : BaseWorkerScheduler
    {
        private readonly TimeSpan timeSpan;

        public IntervalWorkerScheduler(TimeSpan timeSpan)
        {
            this.timeSpan = timeSpan;
        }

        public override void Start(TaskScheduler taskScheduler = null)
        {
            var task = new Task(() => this.Run(taskScheduler));
            task.Start(taskScheduler ?? TaskScheduler.Default);
        }

        private void Run(TaskScheduler taskScheduler)
        {
            while (true)
            {
                foreach (var eachWorker in this)
                {
                    var task = new Task(eachWorker.RunOnce);
                    task.Start(taskScheduler ?? TaskScheduler.Default);
                }
                Thread.Sleep(this.timeSpan);
            }
        }
    }
}