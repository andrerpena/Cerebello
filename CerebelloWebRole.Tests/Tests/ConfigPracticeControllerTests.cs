using System;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Firestarter;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests
{
    [TestClass]
    public class ConfigPracticeControllerTests : DbTestBase
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
            Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre_Miguel(this.db);
        }
        #endregion

        #region Index
        [TestMethod]
        public void Index_UserIsOwner()
        {
            ConfigPracticeController controller;
            var mr = new MockRepository();
            try
            {
                mr.SetCurrentUser_Andre_CorrectPassword();
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ConfigPracticeController), "Index");

                controller = new ConfigPracticeController();
                Mvc3TestHelper.SetupControllerForTesting(controller, this.db, mr, false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(controller, "Index", "GET")
                            ?? Mvc3TestHelper.RunOnActionExecuting(controller, "Index", "GET")
                            ?? controller.Index();

            // Asserts
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            Assert.AreEqual(null, (actionResult as ViewResult).View);
        }

        [TestMethod]
        public void Index_UserIsAdministrator()
        {
            ConfigPracticeController controller;
            var mr = new MockRepository();
            try
            {
                mr.SetCurrentUser_Miguel_CorrectPassword();
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ConfigPracticeController), "Index");

                controller = new ConfigPracticeController();
                Mvc3TestHelper.SetupControllerForTesting(controller, this.db, mr, false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(controller, "Index", "GET")
                            ?? Mvc3TestHelper.RunOnActionExecuting(controller, "Index", "GET")
                            ?? controller.Index();

            // Asserts
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            Assert.AreEqual(null, (actionResult as ViewResult).View);
        }

        [TestMethod]
        public void Index_UserIsSecretary()
        {
            ConfigPracticeController controller;
            var mr = new MockRepository();
            try
            {
                var milena = Firestarter.CreateSecretary_Milena(this.db, this.db.Practices.First());
                mr.SetCurrentUser(milena.Users.Single(), "milena");
                mr.SetRouteData("Index", "ConfigPractice", "App", "consultoriodrhouse");

                controller = new ConfigPracticeController();
                Mvc3TestHelper.SetupControllerForTesting(controller, this.db, mr, false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(controller, "Index", "GET")
                            ?? Mvc3TestHelper.RunOnActionExecuting(controller, "Index", "GET")
                            ?? controller.Index();

            // Asserts
            Assert.IsInstanceOfType(actionResult, typeof(HttpUnauthorizedResult));
        }
        #endregion

        #region Edit [GET]

        [TestMethod]
        public void Edit_UserIsOwner()
        {
            ConfigPracticeController controller;
            var mr = new MockRepository();
            try
            {
                mr.SetCurrentUser_Andre_CorrectPassword();
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ConfigPracticeController), "Edit");

                controller = new ConfigPracticeController();
                Mvc3TestHelper.SetupControllerForTesting(controller, this.db, mr, false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(controller, "Edit", "GET")
                            ?? Mvc3TestHelper.RunOnActionExecuting(controller, "Edit", "GET")
                            ?? controller.Edit();

            // Asserts
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            Assert.AreEqual(null, (actionResult as ViewResult).View);
        }

        [TestMethod]
        public void Edit_UserIsAdministrator()
        {
            ConfigPracticeController controller;
            var mr = new MockRepository();
            try
            {
                mr.SetCurrentUser_Miguel_CorrectPassword();
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ConfigPracticeController), "Edit");

                controller = new ConfigPracticeController();
                Mvc3TestHelper.SetupControllerForTesting(controller, this.db, mr, false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(controller, "Edit", "GET")
                            ?? Mvc3TestHelper.RunOnActionExecuting(controller, "Edit", "GET")
                            ?? controller.Edit();

            // Asserts
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            Assert.AreEqual(null, (actionResult as ViewResult).View);
        }

        [TestMethod]
        public void Edit_UserIsSecretary()
        {
            ConfigPracticeController controller;
            var mr = new MockRepository();
            try
            {
                var milena = Firestarter.CreateSecretary_Milena(this.db, this.db.Practices.First());
                mr.SetCurrentUser(milena.Users.Single(), "milena");
                mr.SetRouteData("Edit", "ConfigPractice", "App", "consultoriodrhouse");

                controller = new ConfigPracticeController();
                Mvc3TestHelper.SetupControllerForTesting(controller, this.db, mr, false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(controller, "Edit", "GET")
                            ?? Mvc3TestHelper.RunOnActionExecuting(controller, "Edit", "GET")
                            ?? controller.Edit();

            // Asserts
            Assert.IsInstanceOfType(actionResult, typeof(HttpUnauthorizedResult));
        }

        #endregion

        #region Edit [POST]

        [TestMethod]
        public void EditPost_UserIsOwner_WithValidData()
        {
            ConfigPracticeController controller;
            var mr = new MockRepository();
            try
            {
                mr.SetCurrentUser_Andre_CorrectPassword();
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ConfigPracticeController), "Edit");

                controller = new ConfigPracticeController();
                Mvc3TestHelper.SetupControllerForTesting(controller, this.db, mr, false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            var viewModel = new ConfigPracticeViewModel
                                {
                                    PracticeName = "K!",
                                    PracticeTimeZone = 3
                                };

            Mvc3TestHelper.SetModelStateErrors(controller, viewModel);

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(controller, "Edit", "POST")
                            ?? Mvc3TestHelper.RunOnActionExecuting(controller, "Edit", "POST")
                            ?? controller.Edit(viewModel);

            // Asserts
            Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)actionResult;
            Assert.AreEqual(2, redirectResult.RouteValues.Count);
            Assert.AreEqual("ConfigPractice", string.Format("{0}", redirectResult.RouteValues["controller"]), ignoreCase: true);
            Assert.AreEqual("Index", string.Format("{0}", redirectResult.RouteValues["action"]), ignoreCase: true);
        }

        [TestMethod]
        public void EditPost_UserIsOwner_WithInvalidData()
        {
            ConfigPracticeController controller;
            var mr = new MockRepository();
            try
            {
                mr.SetCurrentUser_Andre_CorrectPassword();
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ConfigPracticeController), "Edit");

                controller = new ConfigPracticeController();
                Mvc3TestHelper.SetupControllerForTesting(controller, this.db, mr, false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            var viewModel = new ConfigPracticeViewModel
            {
                PracticeName = "", // Cannot set practice name to empty
                PracticeTimeZone = 3
            };

            Mvc3TestHelper.SetModelStateErrors(controller, viewModel);

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(controller, "Edit", "POST")
                            ?? Mvc3TestHelper.RunOnActionExecuting(controller, "Edit", "POST")
                            ?? controller.Edit(viewModel);

            // Asserts
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            Assert.AreEqual(null, (actionResult as ViewResult).View);
        }

        [TestMethod]
        public void EditPost_UserIsAdministrator()
        {
            ConfigPracticeController controller;
            var mr = new MockRepository();
            try
            {
                mr.SetCurrentUser_Miguel_CorrectPassword();
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ConfigPracticeController), "Edit");

                controller = new ConfigPracticeController();
                Mvc3TestHelper.SetupControllerForTesting(controller, this.db, mr, false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(controller, "Edit", "POST")
                            ?? Mvc3TestHelper.RunOnActionExecuting(controller, "Edit", "POST")
                            ?? controller.Edit(new ConfigPracticeViewModel
                            {
                                PracticeName = "My New Practice Name",
                                PracticeTimeZone = 3
                            });

            // Asserts
            Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)actionResult;
            Assert.AreEqual(2, redirectResult.RouteValues.Count);
            Assert.AreEqual("ConfigPractice", string.Format("{0}", redirectResult.RouteValues["controller"]), ignoreCase: true);
            Assert.AreEqual("Index", string.Format("{0}", redirectResult.RouteValues["action"]), ignoreCase: true);
        }

        [TestMethod]
        public void EditPost_UserIsSecretary()
        {
            ConfigPracticeController controller;
            var mr = new MockRepository();
            try
            {
                var milena = Firestarter.CreateSecretary_Milena(this.db, this.db.Practices.First());
                mr.SetCurrentUser(milena.Users.Single(), "milena");
                mr.SetRouteData("Edit", "ConfigPractice", "App", "consultoriodrhouse");

                controller = new ConfigPracticeController();
                Mvc3TestHelper.SetupControllerForTesting(controller, this.db, mr, false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(controller, "Edit", "POST")
                            ?? Mvc3TestHelper.RunOnActionExecuting(controller, "Edit", "POST")
                            ?? controller.Edit(new ConfigPracticeViewModel
                            {
                                PracticeName = "My New Practice Name",
                                PracticeTimeZone = 3
                            });

            // Asserts
            Assert.IsInstanceOfType(actionResult, typeof(HttpUnauthorizedResult));
        }

        #endregion
    }
}
