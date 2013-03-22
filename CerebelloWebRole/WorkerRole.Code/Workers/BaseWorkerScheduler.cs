using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CerebelloWebRole.WorkerRole.Code.Workers
{
    /// <summary>
    /// Base class for worker schedulers.
    /// </summary>
    /// <remarks>
    /// A worker scheduler is a class that can be used to schedule workers to run by using the TaskScheduler class.
    /// The default TaskScheduler is the TaskScheduler.Default, defined by the .Net infrastructure.
    /// </remarks>
    public abstract class BaseWorkerScheduler : IEnumerable<BaseWorker>
    {
        private readonly List<BaseWorker> workers = new List<BaseWorker>();

        /// <summary>
        /// Starts the scheduling of workers, using the passed TaskScheduler.
        /// </summary>
        /// <param name="taskScheduler">The TaskScheduler used to schedule workers to run.</param>
        protected abstract void StartInternal([NotNull]TaskScheduler taskScheduler);

        /// <summary>
        /// Starts the scheduling of workers, using the passed TaskScheduler or the default scheduler.
        /// </summary>
        /// <param name="taskScheduler">The TaskScheduler used to schedule workers to run.</param>
        public void Start(TaskScheduler taskScheduler = null)
        {
            this.StartInternal(taskScheduler ?? TaskScheduler.Default);
        }

        /// <summary>
        /// Adds a worker to be scheduled to run by this class.
        /// </summary>
        /// <param name="worker">The worker to be added.</param>
        public void Add(BaseWorker worker)
        {
            this.workers.Add(worker);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        IEnumerator<BaseWorker> IEnumerable<BaseWorker>.GetEnumerator()
        {
            return this.workers.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.workers.GetEnumerator();
        }
    }
}