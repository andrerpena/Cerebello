using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CerebelloWebRole.WorkerRole.Code.Workers
{
    public abstract class BaseWorkerScheduler : IEnumerable<BaseWorker>
    {
        private readonly List<BaseWorker> workers = new List<BaseWorker>();

        public abstract void Start(TaskScheduler taskScheduler = null);

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