using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cerebello.Model;
using Cerebello.Firestarter;
using CerebelloWebRole.Areas.App.Controllers;
using Cerebello.Firestarter.Helpers;
using System.Web.Mvc;
using CerebelloWebRole.Areas.App.Models;

namespace CerebelloWebRole.Tests.Tests
{
    [TestClass]
    public class PatientsControllerTests
    {
        #region TEST_SETUP
        protected CerebelloEntities db = null;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            DatabaseHelper.AttachCerebelloTestDatabase();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            DatabaseHelper.DetachCerebelloTestDatabase();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            this.db = new CerebelloEntities(string.Format("name={0}", Constants.CONNECTION_STRING_EF));

            Firestarter.ClearAllData(this.db);
            Firestarter.InitializeDatabaseWithSystemData(this.db);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            this.db.Dispose();
        }
        #endregion

        #region Search

        [TestMethod]
        public void Search_ShouldReturnEverythingInEmptySearch()
        {
            PatientsController controller;

            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = Mvc3TestHelper.CreateControllerForTesting<PatientsController>(this.db, mr);
                Firestarter.CreateFakePatients(doctor, this.db, 100);
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            var patientsCount = this.db.Patients.Count();

            // making an empty search
            var result = controller.Search(new Areas.App.Models.SearchModel()
            {
                 Term = "",
                 Page = 1
            });

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var resultAsView = result as ViewResult;

            Assert.IsInstanceOfType(resultAsView.Model, typeof(SearchViewModel<PatientViewModel>));
            var model = resultAsView.Model as SearchViewModel<PatientViewModel>;

            Assert.AreEqual(100, model.Count);
            Assert.AreEqual(CerebelloWebRole.Code.Constants.GRID_PAGE_SIZE, model.Objects.Count);
        }

        [TestMethod]
        public void Search_ShouldRespectTheSearchTermWhenItsPresent()
        {
            PatientsController controller;

            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = Mvc3TestHelper.CreateControllerForTesting<PatientsController>(this.db, mr);
                Firestarter.CreateFakePatients(doctor, this.db, 200);
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            var searchTerm = "an";
            var matchingPatientsCount = this.db.Patients.Count(p => p.Person.FullName.Contains(searchTerm));

            // making an empty search
            var result = controller.Search(new Areas.App.Models.SearchModel()
            {
                Term = searchTerm,
                Page = 1
            });

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var resultAsView = result as ViewResult;

            Assert.IsInstanceOfType(resultAsView.Model, typeof(SearchViewModel<PatientViewModel>));
            var model = resultAsView.Model as SearchViewModel<PatientViewModel>;

            Assert.AreEqual(matchingPatientsCount, model.Count);
            Assert.IsTrue(model.Objects.Count >= CerebelloWebRole.Code.Constants.GRID_PAGE_SIZE);
        }

        #endregion
    }
}
