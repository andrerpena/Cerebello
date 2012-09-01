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
                DatabaseHelper.AttachCerebelloTestDatabase();
            }
        }

        [AssemblyCleanup()]
        public static void CleanupAssembly()
        {
            if (Interlocked.Decrement(ref count) == 0)
            {
                DatabaseHelper.DetachCerebelloTestDatabase();
            }
        }
    }
}
