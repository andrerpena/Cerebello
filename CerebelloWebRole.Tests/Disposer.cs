using System;
using System.Linq;

namespace CerebelloWebRole.Tests
{
    public class Disposer : IDisposable
    {
        void IDisposable.Dispose()
        {
            if (this.Disposing != null)
                foreach (var eachAction in this.Disposing
                    .GetInvocationList()
                    .Cast<Action>()
                    .Reverse()) // The actions must be called in reverse order.
                {
                    eachAction();
                }
        }

        public void Add(IDisposable disposable)
        {
            this.Disposing += new Action(disposable.Dispose);
        }

        public event Action Disposing;
    }
}
