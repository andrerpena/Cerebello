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
        #region InitializeDb, CleanupDb
        protected CerebelloEntities db = null;

        [TestInitialize]
        public virtual void InitializeDb()
        {
            this.db = CreateNewCerebelloEntities();
        }

        [TestCleanup]
        public virtual void CleanupDb()
        {
            if (this.db != null)
                this.db.Dispose();
        }
        #endregion

        #region StartTransaction, RollbackTransaction
        protected TransactionScope scope = null;

        [TestInitialize]
        public virtual void StartTransaction()
        {
            scope = new TransactionScope();
        }

        [TestCleanup]
        public virtual void RollbackTransaction()
        {
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
        protected static void InconclusiveInit(Exception ex)
        {
            Assert.Inconclusive("Init failed with {0}\nMessage: {1}\nStackTrace:\n{2}",
                                ex.GetType().Name, ex.Message, ex.StackTrace);
        }

    }
}
