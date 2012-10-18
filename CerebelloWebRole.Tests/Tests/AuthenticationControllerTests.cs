using System;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cerebello.Firestarter;
using CerebelloWebRole.Areas.Site.Controllers;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Mvc;
using CerebelloWebRole.Code.Security;
using CerebelloWebRole.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CerebelloWebRole.Tests
{
    [TestClass]
    public class AuthenticationControllerTests : DbTestBase
    {
        #region TEST_SETUP
        [TestInitialize]
        public void InitializeData()
        {
            Firestarter.ClearAllData(this.db);
            Firestarter.InitializeDatabaseWithSystemData(this.db);
        }
        #endregion

        #region Create
        /// <summary>
        /// Tests the creation of a new practice, with a valid user.
        /// This can be done, and should result in no errors or validation messages.
        /// NOTE: this is the most complete test in this class...
        /// all other tests should concentrate in deviations from this test.
        /// </summary>
        [TestMethod]
        public void CreateAccount_HappyPath()
        {
            using (var disposer = new Disposer())
            {
                AuthenticationController controller;
                bool hasBeenSaved = false;
                CreateAccountViewModel vm;

                bool wasEmailSent = false;
                string emailBody = null, emailSubject = null, emailToAddress = null;
                bool isEmailHtml = false;

                var utcNow = new DateTime(2012, 08, 31, 0, 0, 0, DateTimeKind.Utc);

                try
                {
                    var mr = new MockRepository();

                    var mve = mr.SetupViewEngine(disposer);
                    mve.SetViewContent(
                        "ConfirmationEmail",
                        vc => ((ConfirmationEmailViewModel)vc.ViewData.Model).ConvertToString("<div>{0}={1}</div>"));

                    var mhc = mr.SetupHttpContext(disposer);

                    controller = Mvc3TestHelper.CreateControllerForTesting<AuthenticationController>(this.db, mr);
                    this.db.SavingChanges += new EventHandler((s, e) => { hasBeenSaved = true; });

                    controller.UtcNowGetter = () => utcNow;

                    controller.EmailSender = mm =>
                    {
                        wasEmailSent = true;
                        emailBody = mm.Body;
                        emailSubject = mm.Subject;
                        emailToAddress = mm.To.Single().Address;
                        isEmailHtml = mm.IsBodyHtml;
                    };

                    // Creating ViewModel, and setting the ModelState of the controller.
                    vm = new CreateAccountViewModel
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
                    Mvc3TestHelper.SetModelStateErrors(controller, vm);
                }
                catch (Exception ex)
                {
                    Assert.Inconclusive(string.Format("Test initialization has failed.\n\n{0}", ex.FlattenMessages()));
                    return;
                }

                // Creating a new user without an e-mail.
                // This must be ok, no exceptions, no validation errors.
                ActionResult actionResult;

                {
                    actionResult = controller.CreateAccount(vm);
                }

                // Getting the user that was saved.
                var savedUser = this.db.Users.Where(u => u.UserName == "andré-01").Single();

                // Assertions.
                Assert.IsNotNull(actionResult, "The result of the controller method is null.");
                Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult));
                var redirectResult = (RedirectToRouteResult)actionResult;
                Assert.AreEqual(redirectResult.RouteValues["action"], "CreateAccountCompleted");
                Assert.IsTrue(controller.ModelState.IsValid, "ModelState should be valid.");
                Assert.IsTrue(hasBeenSaved, "The database should be changed, but it was not.");

                // Assert DB values.
                Assert.AreEqual(savedUser.UserNameNormalized, "andre01");
                Assert.AreEqual(32, savedUser.Practice.VerificationToken.Length);
                Assert.AreEqual(savedUser.Practice.VerificationDate, null);
                Assert.AreEqual(utcNow.AddDays(30), savedUser.Practice.VerificationExpirationDate);
                Assert.IsTrue(savedUser.IsOwner, "Saved user should be the owner of the practice.");
                Assert.AreEqual(savedUser.Id, savedUser.Practice.OwnerId, "Saved user should be the owner of the practice.");

                // Assert user is logged-in.
                Assert.IsTrue(
                    controller.HttpContext.Response.Cookies.Keys.Cast<string>().Contains(".ASPXAUTH"),
                    "Authentication cookie should be present in the Response.");
                var authCookie = controller.HttpContext.Response.Cookies[".ASPXAUTH"];
                var ticket = System.Web.Security.FormsAuthentication.Decrypt(authCookie.Value);
                Assert.AreEqual("andré-01", ticket.Name);
                var token = SecurityTokenHelper.FromString(ticket.UserData);
                Assert.AreEqual(savedUser.Id, token.UserData.Id);
                Assert.AreEqual("André", token.UserData.FullName);
                Assert.AreEqual("andre@gmail.com", token.UserData.Email);
                Assert.AreEqual(false, token.UserData.IsUsingDefaultPassword);

                // Assertion for email.
                Assert.IsTrue(wasEmailSent, "E-mail was not sent, but it should.");
                var emailViewModel = new ConfirmationEmailViewModel
                {
                    Token = savedUser.Practice.VerificationToken,
                    UserName = savedUser.UserName,
                    PersonName = savedUser.Person.FullName,
                    PracticeUrlIdentifier = savedUser.Practice.UrlIdentifier,
                };
                var emailExpected = emailViewModel.ConvertToString("<div>{0}={1}</div>");
                Assert.AreEqual(emailExpected, emailBody);
                Assert.AreEqual("Bem vindo ao Cerebello! Por favor, confirme sua conta.", emailSubject);
                Assert.AreEqual("andre@gmail.com", emailToAddress);
            }
        }

        /// <summary>
        /// Tests the creation of a new practice, with a valid user, that is a doctor of the practice.
        /// This can be done, and should result in no errors or validation messages.
        /// </summary>
        [TestMethod]
        public void CreateAccount_WithDoctor_HappyPath()
        {
            using (var disposer = new Disposer())
            {
                AuthenticationController controller;
                bool hasBeenSaved = false;
                CreateAccountViewModel vm;

                try
                {
                    var mr = new MockRepository();

                    var mve = mr.SetupViewEngine(disposer);
                    mve.SetViewContent(
                        "ConfirmationEmail",
                        vc => ((ConfirmationEmailViewModel)vc.ViewData.Model).ConvertToString("<div>{0}={1}</div>"));

                    var mhc = mr.SetupHttpContext(disposer);

                    controller = Mvc3TestHelper.CreateControllerForTesting<AuthenticationController>(this.db, mr);
                    this.db.SavingChanges += new EventHandler((s, e) => { hasBeenSaved = true; });

                    controller.EmailSender = mm =>
                    {
                        // Do nothing, instead of sending a REAL e-mail.
                        // Don't need to test if this has been called... another test already does this.
                    };

                    // Creating ViewModel, and setting the ModelState of the controller.
                    vm = new CreateAccountViewModel
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
                    Mvc3TestHelper.SetModelStateErrors(controller, vm);
                }
                catch (Exception ex)
                {
                    Assert.Inconclusive("Test initialization has failed.\n\n{0}", ex.FlattenMessages());
                    return;
                }

                // Creating a new user without an e-mail.
                // This must be ok, no exceptions, no validation errors.
                ActionResult actionResult;

                {
                    actionResult = controller.CreateAccount(vm);
                }

                // Getting the user that was saved.
                var savedUser = this.db.Users.Where(u => u.UserName == "andré-01").Single();

                // Assertions.
                Assert.IsNotNull(actionResult, "The result of the controller method is null.");
                Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult));
                var redirectResult = (RedirectToRouteResult)actionResult;
                Assert.AreEqual(redirectResult.RouteValues["action"], "CreateAccountCompleted");
                Assert.IsTrue(controller.ModelState.IsValid, "ModelState should be valid.");
                Assert.IsTrue(hasBeenSaved, "The database should be changed, but it was not.");
                Assert.AreEqual(savedUser.UserNameNormalized, "andre01");

                // Assert user is logged-in: this is already done in CreateAccount_HappyPath.

                // Assertion for email: this is already done in CreateAccount_HappyPath.
            }
        }

        /// <summary>
        /// Tests the creation of a new practice, using a practice name that already exists.
        /// This cannot be done, and should result in no changes to the database.
        /// Also a ModelState validation message must be returned.
        /// </summary>
        [TestMethod]
        public void CreateAccount_2_PracticeNameThatAlreadyExists()
        {
            using (var disposer = new Disposer())
            {
                AuthenticationController controller;
                string practiceName;
                bool hasBeenSaved = false;
                CreateAccountViewModel vm;

                try
                {
                    Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                    var mr = new MockRepository();

                    var mve = mr.SetupViewEngine(disposer);
                    mve.SetViewContent(
                        "ConfirmationEmail",
                        vc => ((ConfirmationEmailViewModel)vc.ViewData.Model).ConvertToString("<div>{0}={1}</div>"));

                    var mhc = mr.SetupHttpContext(disposer);

                    controller = Mvc3TestHelper.CreateControllerForTesting<AuthenticationController>(this.db, mr);
                    practiceName = this.db.Practices.Single().UrlIdentifier;
                    this.db.SavingChanges += new EventHandler((s, e) => { hasBeenSaved = true; });

                    controller.EmailSender = mm =>
                    {
                        // Do nothing, instead of sending a REAL e-mail.
                        // Don't need to test if this has been called... another test already does this.
                    };

                    vm = new CreateAccountViewModel
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
                    Mvc3TestHelper.SetModelStateErrors(controller, vm);
                }
                catch (Exception ex)
                {
                    Assert.Inconclusive(string.Format("Test initialization has failed.\n\n{0}", ex.FlattenMessages()));
                    return;
                }

                // Creating a new user without an e-mail.
                // This must be ok, no exceptions, no validation errors.
                ActionResult actionResult;

                {
                    actionResult = controller.CreateAccount(vm);
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
        }

        /// <summary>
        /// Tests the creation of a new practice, with a user-name that exists in another practice.
        /// This can be done, and should result in no errors or validation messages.
        /// </summary>
        [TestMethod]
        public void CreateAccount_3_UserNameExistsInAnotherPractice_HappyPath()
        {
            using (var disposer = new Disposer())
            {
                AuthenticationController controller;
                string userFullName;
                string userName;
                bool hasBeenSaved = false;
                CreateAccountViewModel vm;

                try
                {
                    Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                    var mr = new MockRepository();

                    var mve = mr.SetupViewEngine(disposer);
                    mve.SetViewContent(
                        "ConfirmationEmail",
                        vc => ((ConfirmationEmailViewModel)vc.ViewData.Model).ConvertToString("<div>{0}={1}</div>"));

                    var mhc = mr.SetupHttpContext(disposer);

                    controller = Mvc3TestHelper.CreateControllerForTesting<AuthenticationController>(this.db, mr);
                    userFullName = this.db.Users.Single().Person.FullName;
                    userName = this.db.Users.Single().UserName;
                    this.db.SavingChanges += new EventHandler((s, e) => { hasBeenSaved = true; });

                    controller.EmailSender = mm =>
                    {
                        // Do nothing, instead of sending a REAL e-mail.
                        // Don't need to test if this has been called... another test already does this.
                    };

                    vm = new CreateAccountViewModel
                    {
                        UserName = userName,
                        PracticeName = "consultoriodrhourse_0832986",
                        Password = "xpto",
                        ConfirmPassword = "xpto",
                        DateOfBirth = new DateTime(1984, 05, 04),
                        EMail = "masbicudo32784678@gmail.com",
                        FullName = userFullName,
                        Gender = (short)TypeGender.Male,
                    };
                    Mvc3TestHelper.SetModelStateErrors(controller, vm);
                }
                catch (Exception ex)
                {
                    Assert.Inconclusive(string.Format("Test initialization has failed.\n\n{0}", ex.FlattenMessages()));
                    return;
                }

                // Creating a new user without an e-mail.
                // This must be ok, no exceptions, no validation errors.
                ActionResult actionResult;

                {
                    actionResult = controller.CreateAccount(vm);
                }

                // Assertions.
                Assert.IsNotNull(actionResult, "The result of the controller method is null.");
                Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult));
                var redirectResult = (RedirectToRouteResult)actionResult;
                Assert.AreEqual(redirectResult.RouteValues["action"], "CreateAccountCompleted");
                Assert.IsTrue(controller.ModelState.IsValid, "ModelState should be valid.");
                Assert.IsTrue(hasBeenSaved, "The database should be changed, but it was not.");
            }
        }

        /// <summary>
        /// Tests the creation of a new practice, with an invalid user-name.
        /// This cannot be done, and should result in no changes to the database.
        /// Also a ModelState validation message must be set by the SetModelStateErrors.
        /// </summary>
        [TestMethod]
        public void CreateAccount_4_UserNameIsInvalid()
        {
            using (var disposer = new Disposer())
            {
                AuthenticationController controller;
                bool hasBeenSaved = false;
                CreateAccountViewModel vm;
                bool hasEmail = false;
                try
                {
                    var mr = new MockRepository();
                    mr.SetRouteData(typeof(AuthenticationController), "CreateAccount");

                    var mve = mr.SetupViewEngine(disposer);
                    mve.SetViewContent("ConfirmationEmail", vc => "<html>Test e-mail string.</html>");

                    controller = new AuthenticationController();
                    Mvc3TestHelper.SetupControllerForTesting(controller, this.db, mr);
                    controller.EmailSender = mm => { hasEmail = true; };

                    this.db.SavingChanges += new EventHandler((s, e) => { hasBeenSaved = true; });

                    // Creating ViewModel, and setting the ModelState of the controller.
                    vm = new CreateAccountViewModel
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
                    Mvc3TestHelper.SetModelStateErrors(controller, vm);
                }
                catch (Exception ex)
                {
                    Assert.Inconclusive(string.Format("Test initialization has failed.\n\n{0}", ex.FlattenMessages()));
                    return;
                }

                // Creating a new user without an e-mail.
                // This must be ok, no exceptions, no validation errors.
                ActionResult actionResult;

                {
                    actionResult = controller.CreateAccount(vm);
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
                Assert.IsFalse(hasEmail, "A confirmation e-mail has been sent. This was not supposed to happen.");
            }
        }
        #endregion

        #region VerifyPracticeAndEmail
        /// <summary>
        /// Verifies the token for a recently created account, and redirects the user to the welcome screen.
        /// This is a valid operation, and should not throw exceptions or return validation messages.
        /// </summary>
        [TestMethod]
        public void VerifyPracticeAndEmail_ValidToken_ValidPractice_HappyPath()
        {
            AuthenticationController controller;
            var utcNow = new DateTime(2012, 08, 31, 0, 0, 0, DateTimeKind.Utc);
            string practiceName;
            string token;

            try
            {
                // Simulating account creation.
                string password;
                var userId = CreateAccount_Helper(utcNow, out password, out token);

                var mr = new MockRepository();

                // Login-in the user that has just been created.
                using (var db = CreateNewCerebelloEntities())
                {
                    var user = db.Users.Single(u => u.Id == userId);
                    mr.SetCurrentUser(user, password);
                    mr.SetRouteData("Any", "Practice", null, user.Practice.UrlIdentifier);

                    practiceName = user.Practice.UrlIdentifier;
                }

                controller = new AuthenticationController();
                controller.UtcNowGetter = () => utcNow;
                Mvc3TestHelper.SetupControllerForTesting(controller, this.db, mr, callOnActionExecuting: true);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive(string.Format("Test initialization has failed.\n\n{0}", ex.FlattenMessages()));
                return;
            }

            ActionResult actionResult;
            {
                actionResult = controller.VerifyPracticeAndEmail(token, practiceName);
            }

            // Asserting.
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult));
            var redirectToRouteResult = (RedirectToRouteResult)actionResult;
            Assert.AreEqual("Welcome", redirectToRouteResult.RouteValues["action"]);
            Assert.AreEqual("PracticeHome", redirectToRouteResult.RouteValues["controller"]);
            Assert.AreEqual("App", redirectToRouteResult.RouteValues["area"]);
            Assert.AreEqual(practiceName, redirectToRouteResult.RouteValues["practice"]);
        }

        /// <summary>
        /// Returns a view that allows the user to enter the verification token manually.
        /// This is a valid operation, and should not throw exceptions or return validation messages.
        /// </summary>
        [TestMethod]
        public void VerifyPracticeAndEmail_EmptyToken_ValidPractice_HappyPath()
        {
            AuthenticationController controller;
            var utcNow = new DateTime(2012, 08, 31, 0, 0, 0, DateTimeKind.Utc);
            string practiceName;

            try
            {
                // Simulating account creation.
                string password;
                string token;
                var userId = CreateAccount_Helper(utcNow, out password, out token);

                var mr = new MockRepository();

                // Login-in the user that has just been created.
                using (var db = CreateNewCerebelloEntities())
                {
                    var user = db.Users.Where(u => u.Id == userId).Single();
                    mr.SetCurrentUser(user, password);
                    mr.SetRouteData("Any", "Practice", null, user.Practice.UrlIdentifier);

                    practiceName = user.Practice.UrlIdentifier;
                }

                controller = new AuthenticationController();
                Mvc3TestHelper.SetupControllerForTesting(controller, this.db, mr, callOnActionExecuting: true);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive(string.Format("Test initialization has failed.\n\n{0}", ex.FlattenMessages()));
                return;
            }

            ActionResult actionResult;
            {
                actionResult = controller.VerifyPracticeAndEmail(null, practiceName);
            }

            // Asserting.
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.IsInstanceOfType(viewResult.Model, typeof(VerifyPracticeAndEmailViewModel));
            var model = (VerifyPracticeAndEmailViewModel)viewResult.Model;

            // ATENTION: The value of the token must NEVER go out to a view.
            Assert.AreEqual(null, model.Token);

            Assert.AreEqual(practiceName, model.Practice);
        }

        /// <summary>
        /// Tries to verify a recently created account with an invalid token.
        /// This is not a valid operations, and must return a validation message.
        /// </summary>
        [TestMethod]
        public void VerifyPracticeAndEmail_InvalidToken_ValidPractice()
        {
            AuthenticationController controller;
            MockRepository mr;
            var utcNow = new DateTime(2012, 08, 31, 0, 0, 0, DateTimeKind.Utc);
            string practiceName;

            try
            {
                // Simulating account creation.
                string password;
                string token;
                var userId = CreateAccount_Helper(utcNow, out password, out token);

                mr = new MockRepository();

                // Login-in the user that has just been created.
                using (var db = CreateNewCerebelloEntities())
                {
                    var user = db.Users.Where(u => u.Id == userId).Single();
                    mr.SetCurrentUser(user, password);
                    mr.SetRouteData("Any", "Practice", null, user.Practice.UrlIdentifier);

                    practiceName = user.Practice.UrlIdentifier;
                }

                controller = new AuthenticationController();
                Mvc3TestHelper.SetupControllerForTesting(controller, this.db, mr, callOnActionExecuting: true);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive(string.Format("Test initialization has failed.\n\n{0}", ex.FlattenMessages()));
                return;
            }

            ActionResult actionResult;
            {
                actionResult = controller.VerifyPracticeAndEmail("Invalid-Token-Value", practiceName);
            }

            // Asserting.
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.IsInstanceOfType(viewResult.Model, typeof(VerifyPracticeAndEmailViewModel));
            var model = (VerifyPracticeAndEmailViewModel)viewResult.Model;

            // ATENTION: The value of the token must NEVER go out to a view.
            Assert.AreEqual(null, model.Token);

            Assert.AreEqual(practiceName, model.Practice);

            // Asserting ModelState.
            Assert.IsTrue(controller.ModelState.ContainsKey("Token"), "ModelState must containt an entry for 'Token'.");
            Assert.AreEqual(1, controller.ModelState["Token"].Errors.Count);
            Assert.AreEqual("Token is not valid.", controller.ModelState["Token"].Errors.First().ErrorMessage);
        }

        /// <summary>
        /// Tries to verify an created account that was created a long time ago,
        /// and remained a long time without verification.
        /// Verification of a new account must happen within 30 days from the account creation.
        /// This is not a valid operations, and must return a validation message.
        /// </summary>
        [TestMethod]
        public void VerifyPracticeAndEmail_ExpiredToken_ValidPractice()
        {
            AuthenticationController controller;
            MockRepository mr;
            var utcNow = new DateTime(2012, 08, 31, 0, 0, 0, DateTimeKind.Utc);
            string practiceName;
            string token;

            try
            {
                // Simulating account creation.
                string password;
                var userId = CreateAccount_Helper(utcNow.AddDays(-200), out password, out token);

                mr = new MockRepository();

                // Login-in the user that has just been created.
                using (var db = CreateNewCerebelloEntities())
                {
                    var user = db.Users.Where(u => u.Id == userId).Single();
                    mr.SetCurrentUser(user, password);
                    mr.SetRouteData("Any", "Practice", null, user.Practice.UrlIdentifier);

                    practiceName = user.Practice.UrlIdentifier;
                }

                controller = new AuthenticationController();
                Mvc3TestHelper.SetupControllerForTesting(controller, this.db, mr, callOnActionExecuting: true);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive(string.Format("Test initialization has failed.\n\n{0}", ex.FlattenMessages()));
                return;
            }

            ActionResult actionResult;
            {
                actionResult = controller.VerifyPracticeAndEmail(token, practiceName);
            }

            // Asserting.
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.IsInstanceOfType(viewResult.Model, typeof(VerifyPracticeAndEmailViewModel));
            var model = (VerifyPracticeAndEmailViewModel)viewResult.Model;

            // ATENTION: The value of the token must NEVER go out to a view.
            Assert.AreEqual(null, model.Token);

            Assert.AreEqual(practiceName, model.Practice);

            // Asserting ModelState.
            Assert.IsTrue(controller.ModelState.ContainsKey("Token"), "ModelState must containt an entry for 'Token'.");
            Assert.AreEqual(1, controller.ModelState["Token"].Errors.Count);
            Assert.AreEqual("Token has expired.", controller.ModelState["Token"].Errors.First().ErrorMessage);
        }
        #endregion

        #region After Create
        /// <summary>
        /// Though this is a valid operation, the user should be redirected
        /// to the EmailValidation screen, stating that the user must get a
        /// token to activate the account in his e-mail in-box.
        /// </summary>
        [TestMethod]
        public void TryAccessPractice_JustAfterCreateAccount()
        {
            PracticeController controller;
            MockRepository mr;
            var utcNow = new DateTime(2012, 08, 31, 0, 0, 0, DateTimeKind.Utc);
            string practiceName;

            try
            {
                // Simulating account creation.
                string password;
                string token;
                var userId = CreateAccount_Helper(utcNow, out password, out token);

                mr = new MockRepository();

                // Login-in the user that has just been created.
                using (var db = CreateNewCerebelloEntities())
                {
                    var user = db.Users.Where(u => u.Id == userId).Single();
                    mr.SetCurrentUser(user, password);
                    mr.SetRouteData("Any", "Practice", null, user.Practice.UrlIdentifier);

                    practiceName = user.Practice.UrlIdentifier;
                }

                var mockController = new Mock<PracticeController>() { CallBase = true };
                controller = mockController.Object;
                Mvc3TestHelper.SetupControllerForTesting(controller, this.db, mr, callOnActionExecuting: false);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive(string.Format("Test initialization has failed.\n\n{0}", ex.FlattenMessages()));
                return;
            }

            ActionResult actionResult;
            {
                actionResult = Mvc3TestHelper.ActionExecutingAndGetActionResult(controller, mr);
            }

            // Asserting.
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult));
            var redirectToRouteResult = (RedirectToRouteResult)actionResult;
            Assert.AreEqual("VerifyPracticeAndEmail", redirectToRouteResult.RouteValues["action"]);
            Assert.AreEqual("Authentication", redirectToRouteResult.RouteValues["controller"]);
            Assert.AreEqual("", redirectToRouteResult.RouteValues["area"]);
            Assert.AreEqual(practiceName, redirectToRouteResult.RouteValues["practice"]);
        }

        /// <summary>
        /// Though this is a valid operation, the user should be redirected
        /// to the Welcome screen, that has informations for the new user.
        /// </summary>
        [TestMethod]
        public void TryAccessPractice_AfterCreateAccountAndValidateEmail()
        {
            PracticeController controller;
            MockRepository mr;
            var utcNow = new DateTime(2012, 08, 31, 0, 0, 0, DateTimeKind.Utc);
            string practiceName;

            try
            {
                // Simulating account creation.
                string password;
                string token;
                var userId = CreateAccount_Helper(utcNow, out password, out token);

                mr = new MockRepository();

                // Login-in the user that has just been created.
                using (var db = CreateNewCerebelloEntities())
                {
                    var user = db.Users.Where(u => u.Id == userId).Single();
                    mr.SetCurrentUser(user, password);
                    mr.SetRouteData("Any", "Practice", null, user.Practice.UrlIdentifier);

                    practiceName = user.Practice.UrlIdentifier;
                }

                // Verifying the new account.
                // Note: the following AuthenticationController is being
                // setup with an invalid MockRepository for it,
                // however this does not prevent proper operation.
                var authController = new AuthenticationController();
                Mvc3TestHelper.SetupControllerForTesting(authController, CreateNewCerebelloEntities(), mr);
                authController.UtcNowGetter = () => utcNow.AddDays(15.0); // this is up to 30 days
                authController.VerifyPracticeAndEmail(token, practiceName);
                Assert.IsTrue(authController.ModelState.IsValid, "Could not validate email.");

                var mockController = new Mock<PracticeController>() { CallBase = true };
                controller = mockController.Object;
                Mvc3TestHelper.SetupControllerForTesting(controller, this.db, mr, callOnActionExecuting: false);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive(string.Format("Test initialization has failed.\n\n{0}", ex.FlattenMessages()));
                return;
            }

            ActionResult actionResult;
            {
                actionResult = Mvc3TestHelper.ActionExecutingAndGetActionResult(controller, mr);
            }

            // Asserting.
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult));
            var redirectToRouteResult = (RedirectToRouteResult)actionResult;
            Assert.AreEqual("Welcome", redirectToRouteResult.RouteValues["action"]);
            Assert.AreEqual("PracticeHome", redirectToRouteResult.RouteValues["controller"]);
            Assert.AreEqual("App", redirectToRouteResult.RouteValues["area"]);
            Assert.AreEqual(practiceName, redirectToRouteResult.RouteValues["practice"]);
        }

        /// <summary>
        /// Simulates the creation of a new account,
        /// by using the real controller,
        /// and mocking everything that is of no interest.
        /// </summary>
        /// <param name="utcNow"></param>
        private static int CreateAccount_Helper(DateTime utcNow, out string password, out string outToken)
        {
            using (var disposer = new Disposer())
            using (var db = CreateNewCerebelloEntities())
            {
                var mr = new MockRepository();

                string token = null;
                var mve = mr.SetupViewEngine(disposer);
                mve.SetViewContent(
                    "ConfirmationEmail",
                    vc =>
                    {
                        token = ((ConfirmationEmailViewModel)vc.ViewData.Model).Token;
                        return "Fake e-mail message.";
                    });

                var mhc = mr.SetupHttpContext(disposer);

                var controller = Mvc3TestHelper.CreateControllerForTesting<AuthenticationController>(db, mr);

                controller.UtcNowGetter = () => utcNow;

                controller.EmailSender = mm =>
                {
                    // Just don't send any REAL e-mail.
                };

                // Creating ViewModel, and setting the ModelState of the controller.
                password = "my_pwd";
                var vm = new CreateAccountViewModel
                {
                    UserName = "andré-01",
                    PracticeName = "consultoriodrhourse_08sd986",
                    Password = password,
                    ConfirmPassword = password,
                    DateOfBirth = new DateTime(1984, 05, 04),
                    EMail = "andre@gmail.com",
                    FullName = "André",
                    Gender = (short)TypeGender.Male,
                };
                Mvc3TestHelper.SetModelStateErrors(controller, vm);

                // Call the action on the controller to create the new account.
                // No assertions will be made to this, because this is not a test.
                // If you want to test any values, do it in a TEST METHOD.
                controller.CreateAccount(vm);

                outToken = token;

                // Getting the Id of the user that was created, and returning it.
                var authCookie = controller.HttpContext.Response.Cookies[".ASPXAUTH"];
                var ticket = System.Web.Security.FormsAuthentication.Decrypt(authCookie.Value);
                var securityToken = SecurityTokenHelper.FromString(ticket.UserData);

                return securityToken.UserData.Id;
            }
        }
        #endregion
    }
}
