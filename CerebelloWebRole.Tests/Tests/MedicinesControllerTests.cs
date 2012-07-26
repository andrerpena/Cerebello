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
using CerebelloWebRole.Models;

namespace CerebelloWebRole.Tests.Tests
{
    [TestClass]
    public class MedicinesControllerTests
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
            MedicinesController controller;

            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository();
                controller = Mvc3TestHelper.CreateControllerForTesting<MedicinesController>(this.db, mr);

                controller.Create(new MedicineViewModel()
                {
                    Name = "Pristiq",
                    LaboratoryName = "MyLab",
                    Usage = (int)TypeUsage.Cutaneo,
                    ActiveIngredients = new List<MedicineActiveIngredientViewModel>()
                    {
                        new MedicineActiveIngredientViewModel() { ActiveIngredientName = "P1" },
                        new MedicineActiveIngredientViewModel() { ActiveIngredientName = "P2" }
                    }
                });

                controller.Create(new MedicineViewModel()
                {
                    Name = "Novalgina",
                    LaboratoryName = "MyLab2",
                    Usage = (int)TypeUsage.Cutaneo,
                    ActiveIngredients = new List<MedicineActiveIngredientViewModel>()
                    {
                        new MedicineActiveIngredientViewModel() { ActiveIngredientName = "P1" },
                    }
                });
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

            Assert.IsInstanceOfType(resultAsView.Model, typeof(SearchViewModel<MedicineViewModel>));
            var model = resultAsView.Model as SearchViewModel<MedicineViewModel>;

            Assert.AreEqual(2, model.Count);
        }

        [TestMethod]
        public void Search_ShouldRespectTheSearchTermWhenItsPresent()
        {
            MedicinesController controller;

            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository();
                controller = Mvc3TestHelper.CreateControllerForTesting<MedicinesController>(this.db, mr);

                controller.Create(new MedicineViewModel()
                {
                    Name = "Pristiq",
                    LaboratoryName = "MyLab",
                    Usage = (int)TypeUsage.Cutaneo,
                    ActiveIngredients = new List<MedicineActiveIngredientViewModel>()
                    {
                        new MedicineActiveIngredientViewModel() { ActiveIngredientName = "P1" },
                        new MedicineActiveIngredientViewModel() { ActiveIngredientName = "P2" }
                    }
                });

                controller.Create(new MedicineViewModel()
                {
                    Name = "Novalgina",
                    LaboratoryName = "MyLab2",
                    Usage = (int)TypeUsage.Cutaneo,
                    ActiveIngredients = new List<MedicineActiveIngredientViewModel>()
                    {
                        new MedicineActiveIngredientViewModel() { ActiveIngredientName = "P1" },
                    }
                });
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            var searchTerm = "nova";

            // making an empty search
            var result = controller.Search(new Areas.App.Models.SearchModel()
            {
                Term = searchTerm,
                Page = 1
            });

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var resultAsView = result as ViewResult;

            Assert.IsInstanceOfType(resultAsView.Model, typeof(SearchViewModel<MedicineViewModel>));
            var model = resultAsView.Model as SearchViewModel<MedicineViewModel>;

            Assert.AreEqual(1, model.Count);
        }

        #endregion
    }
}
