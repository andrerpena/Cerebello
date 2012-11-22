using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = mr.CreateController<MedicinesController>();

                controller.Create(
                    new MedicineViewModel
                        {
                            Name = "Pristiq",
                            LaboratoryName = "MyLab",
                            Usage = (int)TypeUsage.Cutaneo,
                            ActiveIngredients = new List<MedicineActiveIngredientViewModel>
                                {
                                    new MedicineActiveIngredientViewModel {ActiveIngredientName = "P1"},
                                    new MedicineActiveIngredientViewModel {ActiveIngredientName = "P2"}
                                }
                        });

                controller.Create(
                    new MedicineViewModel
                        {
                            Name = "Novalgina",
                            LaboratoryName = "MyLab2",
                            Usage = (int)TypeUsage.Cutaneo,
                            ActiveIngredients = new List<MedicineActiveIngredientViewModel>
                                {
                                    new MedicineActiveIngredientViewModel {ActiveIngredientName = "P1"},
                                }
                        });
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // making an empty search
            var result = controller.Search(
                new SearchModel
                    {
                        Term = "",
                        Page = 1
                    });

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var resultAsView = result as ViewResult;

            Debug.Assert(resultAsView != null, "resultAsView must not be null");
            Assert.IsInstanceOfType(resultAsView.Model, typeof(SearchViewModel<MedicineViewModel>));
            var model = resultAsView.Model as SearchViewModel<MedicineViewModel>;

            Debug.Assert(model != null, "model must not be null");
            Assert.AreEqual(2, model.Count);
        }

        [TestMethod]
        public void Search_ShouldRespectTheSearchTermWhenItsPresent()
        {
            MedicinesController controller;

            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = mr.CreateController<MedicinesController>();

                controller.Create(new MedicineViewModel
                {
                    Name = "Pristiq",
                    LaboratoryName = "MyLab",
                    Usage = (int)TypeUsage.Cutaneo,
                    ActiveIngredients = new List<MedicineActiveIngredientViewModel>
                    {
                        new MedicineActiveIngredientViewModel { ActiveIngredientName = "P1" },
                        new MedicineActiveIngredientViewModel { ActiveIngredientName = "P2" }
                    }
                });

                controller.Create(new MedicineViewModel
                {
                    Name = "Novalgina",
                    LaboratoryName = "MyLab2",
                    Usage = (int)TypeUsage.Cutaneo,
                    ActiveIngredients = new List<MedicineActiveIngredientViewModel>
                    {
                        new MedicineActiveIngredientViewModel { ActiveIngredientName = "P1" },
                    }
                });
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            const string searchTerm = "nova";

            // making an empty search
            var result = controller.Search(new SearchModel
            {
                Term = searchTerm,
                Page = 1
            });

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var resultAsView = result as ViewResult;

            Debug.Assert(resultAsView != null, "resultAsView must not null");
            Assert.IsInstanceOfType(resultAsView.Model, typeof(SearchViewModel<MedicineViewModel>));
            var model = resultAsView.Model as SearchViewModel<MedicineViewModel>;

            Debug.Assert(model != null, "model must not be null");
            Assert.AreEqual(1, model.Count);
        }

        #endregion
    }
}
