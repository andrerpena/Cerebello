﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Firestarter;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests
{
    [TestClass]
    public class UsersControllerTests
    {
        #region TEST_SETUP
        protected CerebelloEntities db = null;

        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
            DatabaseHelper.AttachCerebelloTestDatabase();
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            DatabaseHelper.DetachCerebelloTestDatabase();
        }

        [TestInitialize()]
        public void TestInitialize()
        {
            // Will clear all data and setup initial data again.
            DatabaseHelper.ClearAllData();
            this.db = new CerebelloEntities(ConfigurationManager.ConnectionStrings[Constants.CONNECTION_STRING_EF].ConnectionString);

            // Static information is stored in this class, so we must reset it.
            MockRepository.Reset();
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            this.db.Dispose();
        }
        #endregion

        #region Create
        /// <summary>
        /// Tests the creation of an user without e-mail.
        /// This is a valid operation and should complete without exceptions,
        /// and without validation errors.
        /// </summary>
        [TestMethod]
        public void Create_1_CreateUserWithoutEmail_HappyPath()
        {
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
            }
            catch
            {
                Assert.Inconclusive("Firestarter has failed.");
            }

            // Creating a new user without an e-mail.
            // This must be ok, no exceptions, no validation errors.
            UsersController controller;
            ActionResult actionResult;

            {
                controller = ControllersRepository.CreateControllerForTesting<UsersController>(this.db);
                actionResult = controller.Create(new UserViewModel
                {
                    UserName = "milena",
                    FullName = "Milena",
                    Gender = (int)TypeGender.Female,
                    DateOfBirth = new DateTime(1986, 01, 03),
                    MaritalStatus = (int)TypeMaritalStatus.Solteiro,
                    BirthPlace = "Juiz de Fora",
                    Addresses = new List<AddressViewModel>
                    {
                        new AddressViewModel
                        {
                            Street = "Nome rua",
                            CEP = "36030-060",
                            City = "Juiz de Fora",
                        }
                    },
                    IsSecretary = true,
                });
            }

            // Verifying the ActionResult, and the DB.
            // - The controller ModelState must have no validation errors related to e-mail.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
        }

        /// <summary>
        /// Tests the creation of an user with a repeated user name.
        /// This is an invalid operation, and should stay in the same View, with a ModelState validation message.
        /// </summary>
        [TestMethod]
        public void Create_2_RepeatedUserNameInSamePractice()
        {
            UsersController controller;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                controller = ControllersRepository.CreateControllerForTesting<UsersController>(this.db);
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // Creating a new user with the same UserName of another user in the same practice.
            ActionResult actionResult;

            {
                actionResult = controller.Create(new UserViewModel
                {
                    UserName = "andrerpena",
                    FullName = "André Junior",
                    Gender = (int)TypeGender.Male,
                    DateOfBirth = new DateTime(1970, 11, 11),
                    MaritalStatus = (int)TypeMaritalStatus.Solteiro,
                    BirthPlace = "Juiz de Fora",
                    Addresses = new List<AddressViewModel>
                    {
                        new AddressViewModel
                        {
                            Street = "Nome rua",
                            CEP = "36000-100",
                            City = "Juiz de Fora",
                        }
                    },
                    Emails = new List<EmailViewModel>
                    {
                        new EmailViewModel{ Address = "new_email_address@not_repeated.com.xpto.br", }
                    },
                    IsSecretary = true,
                });
            }

            // Verifying the ActionResult, and the DB.
            // - The result must be a ViewResult, with the name "Edit".
            // - The controller ModelState must have one validation message.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual(viewResult.ViewName, "Edit");
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState should not be valid.");
            Assert.AreEqual(controller.ModelState.Count, 1, "ModelState should contain one validation message.");
        }

        /// <summary>
        /// Tests the creation of an user with the same UserName of an user in another practice.
        /// This is a valid operation.
        /// </summary>
        [TestMethod]
        public void Create_3_RepeatedUserNameInAnotherPractice_HappyPath()
        {
            string userNameToRepeat;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var marta = Firestarter.Create_CrmMg_Psiquiatria_DraMarta_Marta(this.db);
                userNameToRepeat = marta.Users.First().UserName;
            }
            catch
            {
                Assert.Inconclusive("Firestarter has failed.");
                return;
            }

            // Creating a new user with the same UserName of another user in the same practice.
            UsersController controller;
            ActionResult actionResult;

            {
                controller = ControllersRepository.CreateControllerForTesting<UsersController>(this.db);
                actionResult = controller.Create(new UserViewModel
                {
                    UserName = userNameToRepeat,
                    FullName = "Marta Arta dos Campos",
                    Gender = (int)TypeGender.Female,
                    DateOfBirth = new DateTime(1979, 01, 15),
                    MaritalStatus = (int)TypeMaritalStatus.Casado,
                    BirthPlace = "Juiz de Fora",
                    Addresses = new List<AddressViewModel>
                    {
                        new AddressViewModel
                        {
                            Street = "Nome rua",
                            CEP = "37000-200",
                            City = "Juiz de Fora",
                        }
                    },
                    Emails = new List<EmailViewModel>
                    {
                        new EmailViewModel{ Address = "new_email_address@not_repeated.com.xpto.br", }
                    },
                    IsSecretary = true,
                });
            }

            // Verifying the ActionResult, and the DB.
            // - The result must be a ViewResult, with the name "Edit".
            // - The controller ModelState must have one validation message.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
        }

        /// <summary>
        /// Tests the creation of an user without a UserName.
        /// This is an invalid operation, and should stay in the same View, with a ModelState validation message.
        /// </summary>
        [TestMethod]
        public void Create_4_NoUserName()
        {
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
            }
            catch
            {
                Assert.Inconclusive("Firestarter has failed.");
                return;
            }

            // Creating a new user with the same UserName of another user in the same practice.
            UsersController controller;
            ActionResult actionResult;

            {
                controller = ControllersRepository.CreateControllerForTesting<UsersController>(this.db);
                actionResult = controller.Create(new UserViewModel
                {
                    UserName = "", // No user name.
                    FullName = "Milena",
                    Gender = (int)TypeGender.Female,
                    DateOfBirth = new DateTime(1986, 01, 03),
                    MaritalStatus = (int)TypeMaritalStatus.Solteiro,
                    BirthPlace = "Juiz de Fora",
                    Addresses = new List<AddressViewModel>
                    {
                        new AddressViewModel
                        {
                            Street = "Nome rua",
                            CEP = "36030-060",
                            City = "Juiz de Fora",
                        }
                    },
                    Emails = new List<EmailViewModel>
                    {
                        new EmailViewModel{ Address = "new_email_address@not_repeated.com.xpto.br", }
                    },
                    IsSecretary = true,
                });
            }

            // Verifying the ActionResult, and the DB.
            // - The result must be a ViewResult, with the name "Edit".
            // - The controller ModelState must have one validation message.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual(viewResult.ViewName, "Edit");
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState should not be valid.");
            Assert.AreEqual(controller.ModelState.Count, 1, "ModelState should contain one validation message.");
        }

        /// <summary>
        /// Tests the creation of an user without a function: not admin, not medic, not secretary... phantom user!
        /// This is an invalid operation, and should stay in the same View, with a ModelState validation message.
        /// </summary>
        [TestMethod]
        public void Create_5_MustHaveAtLeastOneFunction()
        {
            // Initializing test environment.
            UsersController controller;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                MockRepository.SetCurrentUser_Andre_CorrectPassword();
                controller = ControllersRepository.CreateControllerForTesting<UsersController>(this.db);
            }
            catch
            {
                Assert.Inconclusive("Test initialization failed.");
                return;
            }

            // Creating a new user with the same UserName of another user in the same practice.
            ActionResult actionResult;

            {
                actionResult = controller.Create(new UserViewModel
                {
                    UserName = "milena",
                    FullName = "Milena",
                    Gender = (int)TypeGender.Female,
                    DateOfBirth = new DateTime(1986, 01, 03),
                    MaritalStatus = (int)TypeMaritalStatus.Solteiro,
                    BirthPlace = "Juiz de Fora",
                    Addresses = new List<AddressViewModel>
                    {
                        new AddressViewModel
                        {
                            Street = "Nome rua",
                            CEP = "36030-060",
                            City = "Juiz de Fora",
                        }
                    },
                    Emails = new List<EmailViewModel>
                    {
                        new EmailViewModel{ Address = "new_email_address@not_repeated.com.xpto.br", }
                    }
                });
            }

            // Verifying the ActionResult, and the DB.
            // - The result must be a ViewResult, with the name "Edit".
            // - The controller ModelState must have one validation message.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual(viewResult.ViewName, "Edit");
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState should not be valid.");
            Assert.AreEqual(controller.ModelState.Count, 1, "ModelState should contain one validation message.");
        }

        /// <summary>
        /// Tests the creation of an user that is a medic, without the CRM.
        /// This is an invalid operation, and should stay in the same View, with a ModelState validation message.
        /// </summary>
        [TestMethod]
        public void Create_6_MedicWithoutCRM()
        {
            // Initializing test environment.
            UsersController controller;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                MockRepository.SetCurrentUser_Andre_CorrectPassword();
                controller = ControllersRepository.CreateControllerForTesting<UsersController>(this.db);
            }
            catch
            {
                Assert.Inconclusive("Test initialization failed.");
                return;
            }

            // Creating a new user with the same UserName of another user in the same practice.
            ActionResult actionResult;

            {
                actionResult = controller.Create(new UserViewModel
                {
                    UserName = "milena",
                    FullName = "Milena",
                    Gender = (int)TypeGender.Female,
                    DateOfBirth = new DateTime(1986, 01, 03),
                    MaritalStatus = (int)TypeMaritalStatus.Solteiro,
                    BirthPlace = "Juiz de Fora",
                    Addresses = new List<AddressViewModel>
                    {
                        new AddressViewModel
                        {
                            Street = "Nome rua",
                            CEP = "36030-060",
                            City = "Juiz de Fora",
                        }
                    },
                    Emails = new List<EmailViewModel>
                    {
                        new EmailViewModel{ Address = "new_email_address@not_repeated.com.xpto.br", }
                    },
                    IsMedic = true,
                    MedicCRM = "", // Missing CRM.
                });
            }

            // Verifying the ActionResult, and the DB.
            // - The result must be a ViewResult, with the name "Edit".
            // - The controller ModelState must have one validation message.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual(viewResult.ViewName, "Edit");
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState should not be valid.");
            Assert.AreEqual(controller.ModelState.Count, 1, "ModelState should contain one validation message.");
        }

        /// <summary>
        /// Tests the creation of an user that is a medic.
        /// This is a valid operation.
        /// </summary>
        [TestMethod]
        public void Create_7_Medic_HappyPath()
        {
            // Initializing test environment.
            UsersController controller;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                MockRepository.SetCurrentUser_Andre_CorrectPassword();
                controller = ControllersRepository.CreateControllerForTesting<UsersController>(this.db);
            }
            catch
            {
                Assert.Inconclusive("Test initialization failed.");
                return;
            }

            // Creating a new user with the same UserName of another user in the same practice.
            ActionResult actionResult;

            {
                actionResult = controller.Create(new UserViewModel
                {
                    UserName = "milena",
                    FullName = "Milena",
                    Gender = (int)TypeGender.Female,
                    DateOfBirth = new DateTime(1986, 01, 03),
                    MaritalStatus = (int)TypeMaritalStatus.Solteiro,
                    BirthPlace = "Juiz de Fora",
                    Addresses = new List<AddressViewModel>
                    {
                        new AddressViewModel
                        {
                            Street = "Nome rua",
                            CEP = "36030-060",
                            City = "Juiz de Fora",
                        }
                    },
                    Emails = new List<EmailViewModel>
                    {
                        new EmailViewModel{ Address = "new_email_address@not_repeated.com.xpto.br", }
                    },
                    IsMedic = true,
                    MedicCRM = "98237",
                    MedicalEntity = "CRMMG",
                    MedicalSpecialty = "Psiquiatria",
                });
            }

            // Verifying the ActionResult, and ModelState.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState should be valid.");
        }
        #endregion

        #region Details
        /// <summary>
        /// Tests the visualization of a secretary.
        /// </summary>
        [TestMethod]
        public void Details_1_ViewSecretary_HappyPath()
        {
            UsersController controller;
            int userId;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var s = Firestarter.CreateSecretary_Milena(this.db, this.db.Practices.ToList().Last());
                userId = s.Users.Single().Id;
                controller = ControllersRepository.CreateControllerForTesting<UsersController>(this.db);
            }
            catch
            {
                Assert.Inconclusive("Firestarter has failed.");
                return;
            }

            // Creating a new user without an e-mail.
            // This must be ok, no exceptions, no validation errors.
            {
                ActionResult actionResult = controller.Details(userId);

                // Verifying the ActionResult, ModelState and Model.
                Assert.IsNotNull(actionResult, "The result of the controller method is null.");
                Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
                var viewResult = (ViewResult)actionResult;
                Assert.IsInstanceOfType(viewResult.Model, typeof(UserViewModel));
                var model = (UserViewModel)viewResult.Model;
                Assert.IsTrue(model.IsSecretary, "User is not secretary.");
                Assert.IsTrue(viewResult.ViewName == "Details" || viewResult.ViewName == "", "Wrong view name.");
                Assert.IsTrue(controller.ModelState.IsValid);
            }
        }

        /// <summary>
        /// Tests the visualization of a medic.
        /// </summary>
        [TestMethod]
        public void Details_2_ViewMedic_HappyPath()
        {
            // Initializing test.
            int userId;
            UsersController controller;
            try
            {
                var medic = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                userId = medic.Users.Single().Id;
                controller = ControllersRepository.CreateControllerForTesting<UsersController>(this.db);
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // This must be ok, no exceptions, no validation errors.
            {
                ActionResult actionResult = controller.Details(userId);

                // Verifying the ActionResult and ModelState.
                Assert.IsNotNull(actionResult, "The result of the controller method is null.");
                Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
                var viewResult = (ViewResult)actionResult;
                Assert.IsInstanceOfType(viewResult.Model, typeof(UserViewModel));
                var model = (UserViewModel)viewResult.Model;
                Assert.IsTrue(model.IsMedic, "User is not medic.");
                Assert.IsTrue(viewResult.ViewName == "Details" || viewResult.ViewName == "", "Wrong view name.");
                Assert.IsTrue(controller.ModelState.IsValid);
            }
        }

        /// <summary>
        /// Tests the visualization of an administrator.
        /// </summary>
        [TestMethod]
        public void Details_3_ViewAdministrator_HappyPath()
        {
            // Initializing test.
            int userId;
            UsersController controller;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre_Miguel(this.db);
                var admin = this.db.Users.Where(m => m.AdministratorId != null).First();
                controller = ControllersRepository.CreateControllerForTesting<UsersController>(this.db);
                userId = admin.Id;
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // This must be ok, no exceptions, no validation errors.
            {
                ActionResult actionResult = controller.Details(userId);

                // Verifying the ActionResult and ModelState.
                Assert.IsNotNull(actionResult, "The result of the controller method is null.");
                Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
                var viewResult = (ViewResult)actionResult;
                Assert.IsInstanceOfType(viewResult.Model, typeof(UserViewModel));
                var model = (UserViewModel)viewResult.Model;
                Assert.IsTrue(model.IsAdministrador, "User is not administrator.");
                Assert.IsTrue(viewResult.ViewName == "Details" || viewResult.ViewName == "", "Wrong view name.");
                Assert.IsTrue(controller.ModelState.IsValid);
            }
        }
        #endregion

        #region ChangePassword
        /// <summary>
        /// Tests the access to the ChangePassword action, when using the default password.
        /// This is a valid action, and shoud go straight to the "ChangePassword" view.
        /// </summary>
        [TestMethod]
        public void ChangePassword_1_UserWithDefaultPassword_HappyPath()
        {
            // Initializing.
            UsersController controller;
            int userId;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var practice = this.db.Practices.FirstOrDefault();
                var s = Firestarter.CreateSecretary_Milena(this.db, practice, useDefaultPassword: true);
                var user = s.Users.Single();
                userId = user.Id;
                MockRepository.SetCurrentUser_WithDefaultPassword(user, loginWithUserName: true);
                MockRepository.SetRouteData<UsersController>(practice, null, "changepassword");

                controller = ControllersRepository.CreateControllerForTesting<UsersController>(this.db, callOnActionExecuting: false);
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // Testing.
            ActionResult actionResult;

            {
                actionResult =
                    ControllersRepository.ActionExecutingAndGetActionResult(controller)
                    ?? controller.ChangePassword();
            }

            // Asserting.
            Assert.IsNotNull(actionResult, "ActionResult must not be null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            ViewResult viewResult = (ViewResult)actionResult;
            Assert.AreEqual("", viewResult.ViewName);
            Assert.IsTrue(viewResult.ViewBag.IsDefaultPassword == true);
        }

        /// <summary>
        /// Tests the access to the ChangePassword action, by a normal user.
        /// This is a valid action, and shoud go straight to the "ChangePassword" view.
        /// </summary>
        [TestMethod]
        public void ChangePassword_2_UserWantsToChangePassword_HappyPath()
        {
            // Initializing.
            UsersController controller;
            int userId;
            try
            {
                var d = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var practice = this.db.Practices.FirstOrDefault();
                var user = d.Users.Single();
                userId = user.Id;
                MockRepository.SetCurrentUser_Andre_CorrectPassword(userId);
                MockRepository.SetRouteData<UsersController>(practice, null, "changepassword");

                controller = ControllersRepository.CreateControllerForTesting<UsersController>(this.db, callOnActionExecuting: false);
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // Testing.
            ActionResult actionResult;

            {
                actionResult =
                    ControllersRepository.ActionExecutingAndGetActionResult(controller)
                    ?? controller.ChangePassword();
            }

            // Asserting.
            Assert.IsNotNull(actionResult, "ActionResult must not be null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            ViewResult viewResult = (ViewResult)actionResult;
            Assert.AreEqual("", viewResult.ViewName);
            Assert.IsTrue(viewResult.ViewBag.IsDefaultPassword == false);
        }

        /// <summary>
        /// Tests some cases of the user trying to access the software using the default password.
        /// These are invalid actions, they must redirect the user back to the "ChangePassword" view.
        /// </summary>
        [TestMethod]
        public void ChangePassword_3_AllActionsMustRedirectToChangePasswordWhenUserIsUsingDefaultPassword()
        {
            // Initializing.
            Practice practice;
            Doctor docToView;
            try
            {
                docToView = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                practice = this.db.Practices.FirstOrDefault();
                var d = Firestarter.CreateAdministratorDoctor_Miguel(
                    this.db,
                    this.db.MedicalEntities.FirstOrDefault(),
                    this.db.MedicalSpecialties.FirstOrDefault(),
                    practice,
                    useDefaultPassword: true);
                var user = d.Users.Single();
                var userId = user.Id;
                MockRepository.SetCurrentUser_WithDefaultPassword(user, loginWithUserName: true);
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            bool dbChanged = false;
            this.db.SavingChanges += new EventHandler((s, e) => { dbChanged = true; });

            // Testing PracticeHomeController.
            TestChangePassword_3_Helper<PracticeHomeController>(practice, docToView, "index", c => c.Index());

            // Testing UserController.
            TestChangePassword_3_Helper<UsersController>(practice, docToView, "index", c => c.Index());
            TestChangePassword_3_Helper<UsersController>(practice, docToView, "details", c => c.Details(1));
            TestChangePassword_3_Helper<UsersController>(practice, docToView, "create", c => c.Create());
            TestChangePassword_3_Helper<UsersController>(practice, docToView, "edit", c => c.Edit(1));
            TestChangePassword_3_Helper<UsersController>(practice, docToView, "delete", c => c.Delete(1));

            // Testing DoctorsController.
            TestChangePassword_3_Helper<DoctorsController>(practice, docToView, "index", c => c.Index());

            // Testing DoctorHomeController.
            TestChangePassword_3_Helper<DoctorHomeController>(practice, docToView, "index", c => c.Index());

            // Testing PatientsController.
            TestChangePassword_3_Helper<PatientsController>(practice, docToView, "index", c => c.Index());
            TestChangePassword_3_Helper<PatientsController>(practice, docToView, "details", c => c.Details(1));
            TestChangePassword_3_Helper<PatientsController>(practice, docToView, "create", c => c.Create());
            TestChangePassword_3_Helper<PatientsController>(practice, docToView, "edit", c => c.Edit(1));
            TestChangePassword_3_Helper<PatientsController>(practice, docToView, "delete", c => c.Delete(1));

            // Testing AppController.
            TestChangePassword_3_Helper<AppController>(practice, docToView, "lookupeverything", c => c.LookupEverything("term", 10, 1, 1));

            // Testing AnamnesesController.
            TestChangePassword_3_Helper<AnamnesesController>(practice, docToView, "details", c => c.Details(1));
            TestChangePassword_3_Helper<AnamnesesController>(practice, docToView, "create", c => c.Create(1));
            TestChangePassword_3_Helper<AnamnesesController>(practice, docToView, "edit", c => c.Edit(1, 1));
            TestChangePassword_3_Helper<AnamnesesController>(practice, docToView, "delete", c => c.Delete(1));

            Assert.IsFalse(dbChanged, "Database should not be changed.");
        }

        private void TestChangePassword_3_Helper<T>(Practice practice, Doctor docToView, string action, Func<T, object> exec) where T : Controller, new()
        {
            T controller;

            var counts1 = GetCounts();

            try
            {
                MockRepository.SetRouteData<T>(practice, docToView, action);
                controller = ControllersRepository.CreateControllerForTesting<T>(this.db, callOnActionExecuting: false);
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // Testing.
            var result =
                (object)ControllersRepository.ActionExecutingAndGetActionResult(controller)
                ?? exec(controller);

            var counts2 = GetCounts();

            // Asserting.
            // todo: maybe there is some other way to know if the DB changed.
            for (int it = 0; it < counts1.Length; it++)
                Assert.AreEqual(counts1[it], counts2[it], "Database should not be changed.");

            Assert.IsNotNull(result, "ActionResult must not be null.");
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            RedirectToRouteResult viewResult = (RedirectToRouteResult)result;
            Assert.AreEqual(3, viewResult.RouteValues.Count);
            Assert.AreEqual("app", string.Format("{0}", viewResult.RouteValues["area"]));
            Assert.AreEqual("users", string.Format("{0}", viewResult.RouteValues["controller"]));
            Assert.AreEqual("changepassword", string.Format("{0}", viewResult.RouteValues["action"]));
        }

        private int[] GetCounts()
        {
            int[] counts = new int[]
            {
                this.db.ActiveIngredients.Count(),
                this.db.Addresses.Count(),
                this.db.Administrators.Count(),
                this.db.Anamnese.Count(),
                this.db.Appointments.Count(),
                this.db.CFG_Documents.Count(),
                this.db.CFG_Schedule.Count(),
                this.db.Coverages.Count(),
                this.db.Diagnoses.Count(),
                this.db.Doctors.Count(),
                this.db.Emails.Count(),
                this.db.Laboratories.Count(),
                this.db.Leaflets.Count(),
                this.db.MedicalCertificateFields.Count(),
                this.db.MedicalCertificates.Count(),
                this.db.MedicalEntities.Count(),
                this.db.MedicalSpecialties.Count(),
                this.db.Medicines.Count(),
                this.db.ModelMedicalCertificateFields.Count(),
                this.db.ModelMedicalCertificates.Count(),
                this.db.Patients.Count(),
                this.db.People.Count(),
                this.db.Phones.Count(),
                this.db.Practices.Count(),
                this.db.ReceiptMedicines.Count(),
                this.db.Receipts.Count(),
                this.db.Secretaries.Count(),
                this.db.SYS_ActiveIngredient.Count(),
                this.db.SYS_Laboratory.Count(),
                this.db.SYS_Leaflet.Count(),
                this.db.SYS_Medicine.Count(),
                this.db.Users.Count(),
            };

            return counts;
        }
        #endregion
    }
}
