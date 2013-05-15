using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CerebelloWebRole.WorkerRole.Code.Workers
{
    /// <summary>
    /// Schedules workers to run in specific time intervals.
    /// </summary>
    public class IntervalWorkerScheduler : BaseWorkerScheduler
    {
        private readonly TimeSpan timeSpan;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntervalWorkerScheduler"/> class,
        /// that schedules workers to run every specific time interval.
        /// </summary>
        /// <param name="timeSpan">
        /// The interval of time between scheduling the workers to run.
        /// </param>
        public IntervalWorkerScheduler(TimeSpan timeSpan)
        {
            this.timeSpan = timeSpan;
        }

        /// <summary>
        /// Starts a new thread that will schedule all workers for running.
        /// Then it sleeps for some time.
        /// </summary>
        /// <param name="taskScheduler">The TaskScheduler to use to schedule tasks to be run.</param>
        protected override void StartInternal(TaskScheduler taskScheduler)
        {
            var thread = new Thread(() => this.Run(taskScheduler));
            thread.Start();
        }

        /// <summary>
        /// Runs this scheduler forever... scheduling workers for running,
        /// and sleeping for the amount of time indicated in the constructor.
        /// </summary>
        /// <param name="taskScheduler">The TaskScheduler to use to schedule tasks to be run.</param>
        private void Run([NotNull]TaskScheduler taskScheduler)
        {
            Action<Exception> logException = ex =>
            {
                if (ex != null)
                    Trace.TraceError(ex.Message);
            };

            while (true)
            {
                try
                {
                    // This foreach is instantaneous... it just schedules each worker to run using a taskScheduler.
                    foreach (var eachWorker in this)
                    {
                        var task = new Task(eachWorker.RunOnce);
                        task.Start(taskScheduler);

                        // Observing Exception so that Task finalization does not rethrows it.
                        // If Task finalization is allowed to rethrow exceptions, then process will die,
                        // regardless of the try/catch block surrouding this method.
                        task.ContinueWith(t => logException(t.Exception));
                    }

                    Thread.Sleep(this.timeSpan);
                }
                catch
                {
                    // Never going to crash... so this is safe!
                }
            }
            // ReSharper disable FunctionNeverReturns
        }
        // ReSharper restore FunctionNeverReturns
    }
}
