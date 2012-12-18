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
    public class PracticeHomeControllerTests : DbTestBase
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
            PracticeHomeController homeController;
            var mr = new MockRepository();
            try
            {
                mr.SetCurrentUser_Andre_CorrectPassword();
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(PracticeHomeController), "Index");

                homeController = mr.CreateController<PracticeHomeController>(callOnActionExecuting: false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(homeController, "Index")
                            ?? Mvc3TestHelper.RunOnActionExecuting(homeController, "Index")
                            ?? homeController.Index();

            // Asserts
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            Assert.AreEqual(null, ((ViewResult)actionResult).View);
        }

        [TestMethod]
        public void Index_UserIsAdministrator()
        {
            PracticeHomeController homeController;
            var mr = new MockRepository();
            try
            {
                mr.SetCurrentUser_Miguel_CorrectPassword();
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(PracticeHomeController), "Index");

                homeController = mr.CreateController<PracticeHomeController>(callOnActionExecuting: false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(homeController, "Index")
                            ?? Mvc3TestHelper.RunOnActionExecuting(homeController, "Index")
                            ?? homeController.Index();

            // Asserts
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            Assert.AreEqual(null, ((ViewResult)actionResult).View);
        }

        [TestMethod]
        public void Index_UserIsSecretary()
        {
            PracticeHomeController homeController;
            var mr = new MockRepository();
            try
            {
                var milena = Firestarter.CreateSecretary_Milena(this.db, this.db.Practices.First());
                mr.SetCurrentUser(milena.Users.Single(), "milena");
                mr.SetRouteData("Index", "practicehome", "App", "consultoriodrhouse");

                homeController = mr.CreateController<PracticeHomeController>(callOnActionExecuting: false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(homeController, "Index")
                            ?? Mvc3TestHelper.RunOnActionExecuting(homeController, "Index")
                            ?? homeController.Index();

            // Asserts
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            Assert.AreEqual(null, ((ViewResult)actionResult).View);
        }
        #endregion

        #region Edit [GET]

        [TestMethod]
        public void Edit_UserIsOwner()
        {
            PracticeHomeController homeController;
            var mr = new MockRepository();
            try
            {
                mr.SetCurrentUser_Andre_CorrectPassword();
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(PracticeHomeController), "Edit");

                homeController = mr.CreateController<PracticeHomeController>(callOnActionExecuting: false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(homeController, "Edit")
                            ?? Mvc3TestHelper.RunOnActionExecuting(homeController, "Edit")
                            ?? homeController.Edit();

            // Asserts
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            Assert.AreEqual(null, ((ViewResult)actionResult).View);
        }

        [TestMethod]
        public void Edit_UserIsAdministrator()
        {
            PracticeHomeController homeController;
            var mr = new MockRepository();
            try
            {
                mr.SetCurrentUser_Miguel_CorrectPassword();
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(PracticeHomeController), "Edit");

                homeController = mr.CreateController<PracticeHomeController>(callOnActionExecuting: false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(homeController, "Edit")
                            ?? Mvc3TestHelper.RunOnActionExecuting(homeController, "Edit")
                            ?? homeController.Edit();

            // Asserts
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            Assert.AreEqual(null, ((ViewResult)actionResult).View);
        }

        [TestMethod]
        public void Edit_UserIsSecretary()
        {
            PracticeHomeController homeController;
            var mr = new MockRepository();
            try
            {
                var milena = Firestarter.CreateSecretary_Milena(this.db, this.db.Practices.First());
                mr.SetCurrentUser(milena.Users.Single(), "milena");
                mr.SetRouteData("Edit", "practicehome", "App", "consultoriodrhouse");

                homeController = mr.CreateController<PracticeHomeController>(callOnActionExecuting: false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(homeController, "Edit")
                            ?? Mvc3TestHelper.RunOnActionExecuting(homeController, "Edit")
                            ?? homeController.Edit();

            // Asserts
            Assert.IsInstanceOfType(actionResult, typeof(HttpUnauthorizedResult));
        }

        #endregion

        #region Edit [POST]

        [TestMethod]
        public void EditPost_UserIsOwner_WithValidData()
        {
            PracticeHomeController homeController;
            var mr = new MockRepository();
            try
            {
                mr.SetCurrentUser_Andre_CorrectPassword();
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(PracticeHomeController), "Edit");

                homeController = mr.CreateController<PracticeHomeController>(callOnActionExecuting: false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            var viewModel = new PracticeHomeControllerViewModel
                                {
                                    PracticeName = "K!",
                                    PracticeTimeZone = 3,
                                    PhoneMain = "(32)91272552",
                                    Address = new AddressViewModel
                                        {
                                            StateProvince = "MG",
                                            CEP = "36030-000",
                                            City = "Juiz de Fora",
                                            Complement = "Sta Luzia",
                                            Street = "Rua Sem Saída",
                                        }
                                };

            Mvc3TestHelper.SetModelStateErrors(homeController, viewModel);

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(homeController, "Edit", "POST")
                            ?? Mvc3TestHelper.RunOnActionExecuting(homeController, "Edit", "POST")
                            ?? homeController.Edit(viewModel);

            // Asserts
            Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)actionResult;
            Assert.AreEqual(2, redirectResult.RouteValues.Count);
            Assert.AreEqual("practicehome", string.Format("{0}", redirectResult.RouteValues["controller"]), ignoreCase: true);
            Assert.AreEqual("Index", string.Format("{0}", redirectResult.RouteValues["action"]), ignoreCase: true);
        }

        [TestMethod]
        public void EditPost_UserIsOwner_WithInvalidData()
        {
            PracticeHomeController homeController;
            var mr = new MockRepository();
            try
            {
                mr.SetCurrentUser_Andre_CorrectPassword();
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(PracticeHomeController), "Edit");

                homeController = mr.CreateController<PracticeHomeController>(callOnActionExecuting: false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            var viewModel = new PracticeHomeControllerViewModel
            {
                PracticeName = "", // Cannot set practice name to empty
                PracticeTimeZone = 3
            };

            Mvc3TestHelper.SetModelStateErrors(homeController, viewModel);

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(homeController, "Edit", "POST")
                            ?? Mvc3TestHelper.RunOnActionExecuting(homeController, "Edit", "POST")
                            ?? homeController.Edit(viewModel);

            // Asserts
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            Assert.AreEqual(null, ((ViewResult)actionResult).View);
        }

        [TestMethod]
        public void EditPost_UserIsAdministrator()
        {
            PracticeHomeController homeController;
            var mr = new MockRepository();
            try
            {
                mr.SetCurrentUser_Miguel_CorrectPassword();
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(PracticeHomeController), "Edit");

                homeController = mr.CreateController<PracticeHomeController>(callOnActionExecuting: false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(homeController, "Edit", "POST")
                            ?? Mvc3TestHelper.RunOnActionExecuting(homeController, "Edit", "POST")
                            ?? homeController.Edit(new PracticeHomeControllerViewModel
                            {
                                PracticeName = "My New Practice Name",
                                PracticeTimeZone = 3
                            });

            // Asserts
            Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)actionResult;
            Assert.AreEqual(2, redirectResult.RouteValues.Count);
            Assert.AreEqual("practicehome", string.Format("{0}", redirectResult.RouteValues["controller"]), ignoreCase: true);
            Assert.AreEqual("Index", string.Format("{0}", redirectResult.RouteValues["action"]), ignoreCase: true);
        }

        [TestMethod]
        public void EditPost_UserIsSecretary()
        {
            PracticeHomeController homeController;
            var mr = new MockRepository();
            try
            {
                var milena = Firestarter.CreateSecretary_Milena(this.db, this.db.Practices.First());
                mr.SetCurrentUser(milena.Users.Single(), "milena");
                mr.SetRouteData("Edit", "practicehome", "App", "consultoriodrhouse");

                homeController = mr.CreateController<PracticeHomeController>(callOnActionExecuting: false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Execute test: owner must have access to this view.
            var actionResult = Mvc3TestHelper.RunOnAuthorization(homeController, "Edit", "POST")
                            ?? Mvc3TestHelper.RunOnActionExecuting(homeController, "Edit", "POST")
                            ?? homeController.Edit(new PracticeHomeControllerViewModel
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
