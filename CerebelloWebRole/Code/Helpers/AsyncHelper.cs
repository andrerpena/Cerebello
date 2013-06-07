using System;
using System.Threading;

namespace CerebelloWebRole.Code
{
    public class AsyncHelper {
        class TargetInfo {
            internal TargetInfo(Delegate d, object[] args) {
                Target = d;
                Args = args;
            }

            internal readonly Delegate Target;
            internal readonly object[] Args;
        }

        private static WaitCallback dynamicInvokeShim = new WaitCallback(DynamicInvokeShim);

        public static void FireAndForget(Delegate d, params object[] args) {
            ThreadPool.QueueUserWorkItem(dynamicInvokeShim, new TargetInfo(d, args));
        }

        static void DynamicInvokeShim(object o) {
            try {
                TargetInfo ti = (TargetInfo)o;
                ti.Target.DynamicInvoke(ti.Args);
            } catch (Exception ex) {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }
    }
}
