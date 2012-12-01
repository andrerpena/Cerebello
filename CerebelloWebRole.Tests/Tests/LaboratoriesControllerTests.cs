using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Cerebello.Firestarter;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests
{
    [TestClass]
    public class LaboratoriesControllerTests : DbTestBase
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

        [TestInitialize]
        public override void InitializeDb()
        {
            base.InitializeDb();
            Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
        }

        #endregion

        #region Search

        [TestMethod]
        public void Search_ShouldReturnEverythingInEmptySearch()
        {
            LaboratoriesController controller;

            try
            {
                var mr = new MockRepository(true);
                controller = mr.CreateController<LaboratoriesController>();

                controller.Create(
                    new MedicineLaboratoryViewModel()
                    {
                        Name = "Lab1"
                    });

                controller.Create(
                    new MedicineLaboratoryViewModel()
                    {
                        Name = "Lab2"
                    });
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // making an empty search
            var result = controller.Search(
                new SearchModel()
                {
                    Term = "",
                    Page = 1
                });

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var resultAsView = result as ViewResult;

            Debug.Assert(resultAsView != null, "resultAsView must not be null");
            Assert.IsInstanceOfType(resultAsView.Model, typeof(SearchViewModel<MedicineLaboratoryViewModel>));
            var model = resultAsView.Model as SearchViewModel<MedicineLaboratoryViewModel>;

            Debug.Assert(model != null, "model must not be null");
            Assert.AreEqual(2, model.Count);
        }

        [TestMethod]
        public void Search_ShouldRespectTheSearchTermWhenItsPresent()
        {
            LaboratoriesController controller;

            try
            {
                var mr = new MockRepository(true);
                controller = mr.CreateController<LaboratoriesController>();

                controller.Create(
                    new MedicineLaboratoryViewModel()
                    {
                        Name = "Bash"
                    });

                controller.Create(
                    new MedicineLaboratoryViewModel()
                    {
                        Name = "Novartis"
                    });
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            const string searchTerm = "ba";

            // making an empty search
            var result = controller.Search(
                new SearchModel()
                {
                    Term = searchTerm,
                    Page = 1
                });

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var resultAsView = result as ViewResult;

            Debug.Assert(resultAsView != null, "resultAsView must not null");
            Assert.IsInstanceOfType(resultAsView.Model, typeof(SearchViewModel<MedicineLaboratoryViewModel>));
            var model = resultAsView.Model as SearchViewModel<MedicineLaboratoryViewModel>;

            Debug.Assert(model != null, "model must not be null");
            Assert.AreEqual(1, model.Count);
        }

        #endregion
    }
}
