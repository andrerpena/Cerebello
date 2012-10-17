using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests
{
    [TestClass]
    public class AssemblyTest
    {
        static int count = 0;

        [AssemblyInitialize()]
        public static void InitializeAssembly(TestContext ctx)
        {
            if (Interlocked.Increment(ref count) == 1)
            {
                try
                {
                    DatabaseHelper.AttachCerebelloTestDatabase();
                }
                catch
                {
                }
            }
        }

        [AssemblyCleanup()]
        public static void CleanupAssembly()
        {
            if (Interlocked.Decrement(ref count) == 0)
            {
                try
                {
                    DatabaseHelper.DetachCerebelloTestDatabase();
                }
                catch
                {
                }
            }
        }
    }
}
