using System;
using System.Linq;

namespace CerebelloWebRole.Code
{
    public class Disposer : IDisposable
    {
        public Disposer()
        {
        }

        public Disposer(Action onDisposing)
        {
            this.Disposing += onDisposing;
        }

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
            this.Disposing += disposable.Dispose;
        }

        public event Action Disposing;
    }
}