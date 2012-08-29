using System;
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
using Cerebello;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Areas.Site.Controllers;
using CerebelloWebRole.Code.Mvc;

namespace CerebelloWebRole.Tests
{
    [TestClass]
    public class AuthenticationControllerTests
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
            this.db = new CerebelloEntities(string.Format("name={0}", Constants.CONNECTION_STRING_EF));

            Firestarter.ClearAllData(this.db);
            Firestarter.InitializeDatabaseWithSystemData(this.db);
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            this.db.Dispose();
        }
        #endregion

        #region Create
        /// <summary>
        /// Tests the creation of a new practice, with a valid user.
        /// This can be done, and should result in no errors or validation messages.
        /// </summary>
        [TestMethod]
        public void CreateAccount_1_HappyPath()
        {
            AuthenticationController controller;
            string userFullName;
            bool hasBeenSaved = false;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository();
                controller = Mvc3TestHelper.CreateControllerForTesting<AuthenticationController>(this.db, mr);
                userFullName = this.db.Users.Single().Person.FullName;
                this.db.SavingChanges += new EventHandler((s, e) => { hasBeenSaved = true; });
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // Creating a new user without an e-mail.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                var data = new CreateAccountViewModel
                {
                    UserName = "andré-01",
                    PracticeName = "consultoriodrhourse_08sd986",
                    Password = "xpto",
                    ConfirmPassword = "xpto",
                    DateOfBirth = new DateTime(1984, 05, 04),
                    EMail = "andre@gmail.com",
                    FullName = "André",
                    Gender = (short)TypeGender.Male,
                };
                Mvc3TestHelper.SetModelStateErrors(controller, data);
                actionResult = controller.CreateAccount(data);
            }

            // Getting the user that was saved.
            var savedUser = this.db.Users.Where(u => u.UserName == "andré-01").Single();

            // Assertions.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)actionResult;
            Assert.AreEqual(redirectResult.RouteValues["action"], "createaccountcompleted");
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState should be valid.");
            Assert.IsTrue(hasBeenSaved, "The database should be changed, but it was not.");
            Assert.AreEqual(savedUser.UserNameNormalized, "andre01");
        }

        /// <summary>
        /// Tests the creation of a new practice, with a valid user, that is a doctor of the practice.
        /// This can be done, and should result in no errors or validation messages.
        /// </summary>
        [TestMethod]
        public void CreateAccount_WithDoctor_HappyPath()
        {
            AuthenticationController controller;
            string userFullName;
            bool hasBeenSaved = false;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository();
                controller = Mvc3TestHelper.CreateControllerForTesting<AuthenticationController>(this.db, mr);
                userFullName = this.db.Users.Single().Person.FullName;
                this.db.SavingChanges += new EventHandler((s, e) => { hasBeenSaved = true; });
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // Creating a new user without an e-mail.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                var data = new CreateAccountViewModel
                {
                    UserName = "andré-01",
                    PracticeName = "consultoriodrhourse_08sd986",
                    Password = "xpto",
                    ConfirmPassword = "xpto",
                    DateOfBirth = new DateTime(1984, 05, 04),
                    EMail = "andre@gmail.com",
                    FullName = "André",
                    Gender = (short)TypeGender.Male,
                    IsDoctor = true,
                    MedicCRM = "98237",
                    MedicalEntity = this.db.SYS_MedicalEntity.First().Id,
                    MedicalSpecialty = this.db.SYS_MedicalSpecialty.First().Id,
                };
                Mvc3TestHelper.SetModelStateErrors(controller, data);
                actionResult = controller.CreateAccount(data);
            }

            // Getting the user that was saved.
            var savedUser = this.db.Users.Where(u => u.UserName == "andré-01").Single();

            // Assertions.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)actionResult;
            Assert.AreEqual(redirectResult.RouteValues["action"], "createaccountcompleted");
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState should be valid.");
            Assert.IsTrue(hasBeenSaved, "The database should be changed, but it was not.");
            Assert.AreEqual(savedUser.UserNameNormalized, "andre01");
        }

        /// <summary>
        /// Tests the creation of a new practice, using a practice name that already exists.
        /// This cannot be done, and should result in no changes to the database.
        /// Also a ModelState validation message must be returned.
        /// </summary>
        [TestMethod]
        public void CreateAccount_2_PracticeNameThatAlreadyExists()
        {
            AuthenticationController controller;
            string practiceName;
            bool hasBeenSaved = false;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository();
                controller = Mvc3TestHelper.CreateControllerForTesting<AuthenticationController>(this.db, mr);
                practiceName = this.db.Practices.Single().UrlIdentifier;
                this.db.SavingChanges += new EventHandler((s, e) => { hasBeenSaved = true; });
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // Creating a new user without an e-mail.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                var data = new CreateAccountViewModel
                {
                    UserName = "masbicudo_1238236",
                    PracticeName = practiceName,
                    Password = "xpto",
                    ConfirmPassword = "xpto",
                    DateOfBirth = new DateTime(1984, 05, 04),
                    EMail = "masbicudo32784678@gmail.com",
                    FullName = "Miguel Angelo Santos Bicudo",
                    Gender = (short)TypeGender.Male,
                };
                Mvc3TestHelper.SetModelStateErrors(controller, data);
                actionResult = controller.CreateAccount(data);
            }

            // Assertions.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual(viewResult.ViewName, "");
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState should not be valid.");
            Assert.AreEqual(1, controller.ModelState.GetAllErrors().Count, "ModelState should contain one validation message.");
            Assert.IsTrue(
                controller.ModelState.ContainsKey("PracticeName"),
                "ModelState must contain validation message for 'PracticeName'.");
            Assert.IsFalse(hasBeenSaved, "The database has been changed. This was not supposed to happen.");
        }

        /// <summary>
        /// Tests the creation of a new practice, with a user-name that exists in another practice.
        /// This can be done, and should result in no errors or validation messages.
        /// </summary>
        [TestMethod]
        public void CreateAccount_3_UserNameExistsInAnotherPractice_HappyPath()
        {
            AuthenticationController controller;
            string userFullName;
            bool hasBeenSaved = false;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository();
                controller = Mvc3TestHelper.CreateControllerForTesting<AuthenticationController>(this.db, mr);
                userFullName = this.db.Users.Single().Person.FullName;
                this.db.SavingChanges += new EventHandler((s, e) => { hasBeenSaved = true; });
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // Creating a new user without an e-mail.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                var data = new CreateAccountViewModel
                {
                    UserName = "masbicudo_1238236",
                    PracticeName = "consultoriodrhourse_0832986",
                    Password = "xpto",
                    ConfirmPassword = "xpto",
                    DateOfBirth = new DateTime(1984, 05, 04),
                    EMail = "masbicudo32784678@gmail.com",
                    FullName = userFullName,
                    Gender = (short)TypeGender.Male,
                };
                Mvc3TestHelper.SetModelStateErrors(controller, data);
                actionResult = controller.CreateAccount(data);
            }

            // Assertions.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)actionResult;
            Assert.AreEqual(redirectResult.RouteValues["action"], "createaccountcompleted");
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState should be valid.");
            Assert.IsTrue(hasBeenSaved, "The database should be changed, but it was not.");
        }

        /// <summary>
        /// Tests the creation of a new practice, with an invalid user-name.
        /// This cannot be done, and should result in no changes to the database.
        /// Also a ModelState validation message must be set by the SetModelStateErrors.
        /// </summary>
        [TestMethod]
        public void CreateAccount_4_UserNameIsInvalid()
        {
            AuthenticationController controller;
            string practiceName;
            bool hasBeenSaved = false;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository();
                controller = Mvc3TestHelper.CreateControllerForTesting<AuthenticationController>(this.db, mr);
                practiceName = this.db.Practices.Single().UrlIdentifier;
                this.db.SavingChanges += new EventHandler((s, e) => { hasBeenSaved = true; });
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // Creating a new user without an e-mail.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                var data = new CreateAccountViewModel
                {
                    UserName = "André#Pena", // char # is invalid
                    PracticeName = "New Practice Name 4146",
                    Password = "xpto",
                    ConfirmPassword = "xpto",
                    DateOfBirth = new DateTime(1984, 09, 01),
                    EMail = "andrerpena32784678@gmail.com",
                    FullName = "André Rodrigues Pena",
                    Gender = (short)TypeGender.Male,
                };
                Mvc3TestHelper.SetModelStateErrors(controller, data);
                actionResult = controller.CreateAccount(data);
            }

            // Assertions.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual(viewResult.ViewName, "");
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState should not be valid.");
            Assert.AreEqual(1, controller.ModelState.GetAllErrors().Count, "ModelState should contain one validation message.");
            Assert.IsTrue(
                controller.ModelState.ContainsKey("UserName"),
                "ModelState must contain validation message for 'PracticeName'.");
            Assert.IsFalse(hasBeenSaved, "The database has been changed. This was not supposed to happen.");
        }
        #endregion
    }
}
