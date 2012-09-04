using Cerebello.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests
{
    [TestClass]
    public class DbTestBase
    {
        #region TEST_SETUP
        protected CerebelloEntities db = null;

        [TestInitialize]
        public void InitializeDb()
        {
            this.db = CreateNewCerebelloEntities();
        }

        [TestCleanup]
        public void CleanupDb()
        {
            this.db.Dispose();
        }
        #endregion

        protected static CerebelloEntities CreateNewCerebelloEntities()
        {
            return new CerebelloEntities(string.Format("name={0}", Constants.CONNECTION_STRING_EF));
        }
    }
}
