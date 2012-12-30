using System;
using System.Diagnostics;
using System.Threading;
using System.Transactions;
using Cerebello.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests
{
    [TestClass]
    public class DbTestBase
    {
        #region TestInitialize, TestCleanup
        protected CerebelloEntities db = null;
        protected TransactionScope scope = null;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            scope = new TransactionScope();
            this.db = CreateNewCerebelloEntities();
        }

        [TestCleanup]
        public virtual void TestCleanup()
        {
            if (this.db != null)
                this.db.Dispose();
            scope.Dispose();
        }
        #endregion

        #region Attach and Detach
        static int attachCount = 0;

        protected static void AttachCerebelloTestDatabase()
        {
            if (Interlocked.Increment(ref attachCount) == 1)
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

        protected static void DetachCerebelloTestDatabase()
        {
            if (Interlocked.Decrement(ref attachCount) == 0)
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
        #endregion

        public static CerebelloEntities CreateNewCerebelloEntities()
        {
            return new CerebelloEntities(string.Format("name={0}", Constants.CONNECTION_STRING_EF));
        }

        [DebuggerStepThrough]
        protected static void InconclusiveInit(Exception ex, string message = null)
        {
            Assert.Inconclusive(
                "{3}: {0}\nMessage: {1}\nStackTrace:\n{2}",
                ex.GetType().Name,
                ex.FlattenMessages(),
                ex.StackTrace,
                message ?? "Init failed");
        }
    }
}
