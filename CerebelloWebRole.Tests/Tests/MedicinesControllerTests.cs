using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Firestarter;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests
{
    [TestClass]
    public class MedicinesControllerTests : DbTestBase
    {
        #region TEST_SETUP
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            AttachCerebelloTestDatabase();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            DetachCerebelloTestDatabase();
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
                var mr = new MockRepository(true);
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
                var mr = new MockRepository(true);
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
