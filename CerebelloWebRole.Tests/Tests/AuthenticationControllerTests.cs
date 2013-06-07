using System;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Firestarter;
using Cerebello.Model;
using CerebelloWebRole.Code;
using CerebelloWebRole.Controllers;
using CerebelloWebRole.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests
{
    [TestClass]
    public class AuthenticationControllerTests : DbTestBase
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
        public override void TestInitialize()
        {
            base.TestInitialize();
            Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
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
                var hasBeenSaved = false;
                CreateAccountViewModel vm;

                var wasEmailSent = false;
                string emailBody = null, emailSubject = null, emailToAddress = null;

                var utcNow = new DateTime(2012, 08, 31, 0, 0, 0, DateTimeKind.Utc);

                try
                {
                    var mr = new MockRepository();

                    var mve = mr.SetupViewEngine(disposer);
                    mve.SetViewContent(
                        "ConfirmationEmail",
                        vc => vc.ViewData.Model.ConvertObjectToString("<div>{0}={1}</div>"));
                    mr.SetRouteData_ControllerAndActionOnly("Home", "Index");

                    mr.SetupHttpContext(disposer);

                    controller = mr.CreateController<AuthenticationController>(
                        setupNewDb: db2 => db2.SavingChanges += (s, e) => { hasBeenSaved = true; });

                    controller.UtcNowGetter = () => utcNow;

                    controller.EmailSender = mm =>
                    {
                        wasEmailSent = true;
                        emailBody = mm.Body;
                        emailSubject = mm.Subject;
                        emailToAddress = mm.To.Single().Address;
                    };

                    // Creating ViewModel, and setting the ModelState of the controller.
                    vm = new CreateAccountViewModel
                    {
                        UserName = "andré-01",
                        PracticeName = "consultoriodrhouse_08sd986",
                        Password = "xpto",
                        ConfirmPassword = "xpto",
                        DateOfBirth = new DateTime(1984, 05, 04),
                        EMail = "andre@fakemail.com",
                        FullName = "André",
                        Gender = (short)TypeGender.Male,
                    };
                    Mvc3TestHelper.SetModelStateErrors(controller, vm);
                }
                catch (Exception ex)
                {
                    InconclusiveInit(ex);
                    return;
                }

                // Creating a new user without an e-mail.
                // This must be ok, no exceptions, no validation errors.
                ActionResult actionResult;

                {
                    actionResult = controller.CreateAccount(vm);
                }

                // Getting the user that was saved.
                var savedUser = this.db.Users.Single(u => u.UserName == "andré-01");
                var savedToken = this.db.GLB_Token.Single();

                // Assertions.
                Assert.IsNotNull(actionResult, "The result of the controller method is null.");
                Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult));
                var redirectResult = (RedirectToRouteResult)actionResult;
                Assert.AreEqual(redirectResult.RouteValues["action"], "CreateAccountCompleted");
                Assert.IsTrue(controller.ModelState.IsValid, "ModelState should be valid.");
                Assert.IsTrue(hasBeenSaved, "The database should be changed, but it was not.");

                // Assert DB values.
                Assert.AreEqual(savedUser.UserNameNormalized, "andre01");
                Assert.AreEqual(32, savedToken.Value.Length);
                Assert.AreEqual(savedUser.Practice.VerificationDate, null);
                Assert.AreEqual(utcNow.AddDays(30), savedToken.ExpirationDate);
                Assert.IsTrue(savedUser.IsOwner, "Saved user should be the owner of the practice.");
                Assert.AreEqual(savedUser.Id, savedUser.Practice.OwnerId, "Saved user should be the owner of the practice.");
                Assert.IsNotNull(savedUser.Administrator, "Practice owner must be administrator.");

                // Assert user is logged-in.
                Assert.IsTrue(
                    controller.HttpContext.Response.Cookies.Keys.Cast<string>().Contains(".ASPXAUTH"),
                    "Authentication cookie should be present in the Response.");
                var authCookie = controller.HttpContext.Response.Cookies[".ASPXAUTH"];
                Assert.IsNotNull(authCookie, @"Response.Cookies["".ASPXAUTH""] must not be null.");
                var ticket = System.Web.Security.FormsAuthentication.Decrypt(authCookie.Value);
                Assert.AreEqual("andré-01", ticket.Name);
                var token = SecurityTokenHelper.FromString(ticket.UserData);
                Assert.AreEqual(savedUser.Id, token.UserData.Id);
                Assert.AreEqual("André", token.UserData.FullName);
                Assert.AreEqual("andre@fakemail.com", token.UserData.Email);
                Assert.AreEqual(false, token.UserData.IsUsingDefaultPassword);

                // Assertion for email.
                Assert.IsTrue(wasEmailSent, "E-mail was not sent, but it should.");
                var emailViewModel = new UserEmailViewModel(savedUser)
                {
                    Token = new TokenId(savedToken.Id, savedToken.Value).ToString(),
                };
                var emailExpected = emailViewModel.ConvertObjectToString("<div>{0}={1}</div>");
                Assert.AreEqual(emailExpected, emailBody);
                Assert.AreEqual("Bem vindo ao Cerebello! Por favor, confirme a criação de sua conta.", emailSubject);
                Assert.AreEqual("andre@fakemail.com", emailToAddress);
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
                var hasBeenSaved = false;
                CreateAccountViewModel vm;

                try
                {
                    var mr = new MockRepository();

                    var mve = mr.SetupViewEngine(disposer);
                    mve.SetViewContent(
                        "ConfirmationEmail",
                        vc => vc.ViewData.Model.ConvertObjectToString("<div>{0}={1}</div>"));
                    mr.SetRouteData_ControllerAndActionOnly("Home", "Index");

                    mr.SetupHttpContext(disposer);

                    controller = mr.CreateController<AuthenticationController>(
                        setupNewDb: db2 => db2.SavingChanges += (s, e) => { hasBeenSaved = true; });
                    mr.SetupUrlHelper(controller);

                    controller.EmailSender = mm =>
                    {
                        // Do nothing, instead of sending a REAL e-mail.
                        // Don't need to test if this has been called... another test already does this.
                    };

                    // Creating ViewModel, and setting the ModelState of the controller.
                    var me = Firestarter.GetMedicalEntity_Psicologia(this.db);
                    var ms = Firestarter.GetMedicalSpecialty_Psiquiatra(this.db);
                    vm = new CreateAccountViewModel
                    {
                        UserName = "andré-01",
                        PracticeName = "consultoriodrhouse_08sd986",
                        Password = "xpto",
                        ConfirmPassword = "xpto",
                        DateOfBirth = new DateTime(1984, 05, 04),
                        EMail = "andre@fakemail.com",
                        FullName = "André",
                        Gender = (short)TypeGender.Male,
                        IsDoctor = true,
                        MedicCRM = "98237",
                        MedicalEntityId = me.Id,
                        MedicalSpecialtyId = ms.Id,
                        MedicalSpecialtyName = ms.Name,
                        MedicalEntityJurisdiction = (int)TypeEstadoBrasileiro.RJ,
                    };
                    Mvc3TestHelper.SetModelStateErrors(controller, vm);
                }
                catch (Exception ex)
                {
                    InconclusiveInit(ex);
                    return;
                }

                // Creating a new user without an e-mail.
                // This must be ok, no exceptions, no validation errors.
                ActionResult actionResult;

                {
                    actionResult = controller.CreateAccount(vm);
                }

                // Getting the user that was saved.
                var savedUser = this.db.Users.Single(u => u.UserName == "andré-01");

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
                var hasBeenSaved = false;
                CreateAccountViewModel vm;

                try
                {
                    var mr = new MockRepository();

                    var mve = mr.SetupViewEngine(disposer);
                    mve.SetViewContent(
                        "ConfirmationEmail",
                        vc => vc.ViewData.Model.ConvertObjectToString("<div>{0}={1}</div>"));

                    mr.SetupHttpContext(disposer);

                    controller = mr.CreateController<AuthenticationController>(
                        setupNewDb: db2 => db2.SavingChanges += (s, e) => { hasBeenSaved = true; });
                    var practiceName = this.db.Practices.Single().UrlIdentifier;

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
                        EMail = "masbicudo32784678@fakemail.com",
                        FullName = "Miguel Angelo Santos Bicudo",
                        Gender = (short)TypeGender.Male,
                    };
                    Mvc3TestHelper.SetModelStateErrors(controller, vm);
                }
                catch (Exception ex)
                {
                    InconclusiveInit(ex);
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
                var hasBeenSaved = false;
                CreateAccountViewModel vm;

                try
                {
                    var mr = new MockRepository();

                    var mve = mr.SetupViewEngine(disposer);
                    mve.SetViewContent(
                        "ConfirmationEmail",
                        vc => vc.ViewData.Model.ConvertObjectToString("<div>{0}={1}</div>"));
                    mr.SetRouteData_ControllerAndActionOnly("Home", "Index");

                    mr.SetupHttpContext(disposer);

                    controller = mr.CreateController<AuthenticationController>(
                        setupNewDb: db2 => db2.SavingChanges += (s, e) => { hasBeenSaved = true; });
                    var userFullName = this.db.Users.Single().Person.FullName;
                    var userName = this.db.Users.Single().UserName;

                    controller.EmailSender = mm =>
                    {
                        // Do nothing, instead of sending a REAL e-mail.
                        // Don't need to test if this has been called... another test already does this.
                    };

                    vm = new CreateAccountViewModel
                    {
                        UserName = userName,
                        PracticeName = "consultoriodrhouse_0832986",
                        Password = "xpto",
                        ConfirmPassword = "xpto",
                        DateOfBirth = new DateTime(1984, 05, 04),
                        EMail = "masbicudo32784678@fakemail.com",
                        FullName = userFullName,
                        Gender = (short)TypeGender.Male,
                    };
                    Mvc3TestHelper.SetModelStateErrors(controller, vm);
                }
                catch (Exception ex)
                {
                    InconclusiveInit(ex);
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
                var hasBeenSaved = false;
                CreateAccountViewModel vm;
                var hasEmail = false;
                try
                {
                    var mr = new MockRepository();
                    mr.SetRouteData(typeof(AuthenticationController), "CreateAccount");

                    var mve = mr.SetupViewEngine(disposer);
                    mve.SetViewContent("ConfirmationEmail", vc => "<html>Test e-mail string.</html>");

                    controller = mr.CreateController<AuthenticationController>(
                        setupNewDb: db2 => db2.SavingChanges += (s, e) => { hasBeenSaved = true; });

                    controller.EmailSender = mm => { hasEmail = true; };

                    // Creating ViewModel, and setting the ModelState of the controller.
                    vm = new CreateAccountViewModel
                    {
                        UserName = "André#Pena", // char # is invalid
                        PracticeName = "New Practice Name 4146",
                        Password = "xpto",
                        ConfirmPassword = "xpto",
                        DateOfBirth = new DateTime(1984, 09, 01),
                        EMail = "andrerpena32784678@fakemail.com",
                        FullName = "André Rodrigues Pena",
                        Gender = (short)TypeGender.Male,
                    };
                    Mvc3TestHelper.SetModelStateErrors(controller, vm);
                }
                catch (Exception ex)
                {
                    InconclusiveInit(ex);
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

        #region Login
        /// <summary>
        /// Tests the login with the remember-me checkbox set.
        /// </summary>
        [TestMethod]
        public void Login_RemeberMe_HappyPath()
        {
            AuthenticationController controller;
            var hasBeenSaved = false;
            LoginViewModel vm;

            var utcNow = new DateTime(2012, 08, 31, 0, 0, 0, DateTimeKind.Utc);

            User user;
            try
            {
                user = this.db.Users.Single(u => u.UserName == "andrerpena");

                var mr = new MockRepository();

                controller = mr.CreateController<AuthenticationController>(
                    setupNewDb: db2 => db2.SavingChanges += (s, e) => { hasBeenSaved = true; });

                controller.UtcNowGetter = () => utcNow;

                // Creating ViewModel, and setting the ModelState of the controller.
                vm = new LoginViewModel
                {
                    UserNameOrEmail = "andrerpena",
                    PracticeIdentifier = "consultoriodrhouse",
                    Password = "ph4r40h",
                    RememberMe = true,
                };
                Mvc3TestHelper.SetModelStateErrors(controller, vm);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Creating a new user without an e-mail.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Login(vm);
            }

            // Assertions.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)actionResult;
            Assert.AreEqual(redirectResult.RouteValues["action"], "Index");
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState should be valid.");
            Assert.IsTrue(hasBeenSaved, "The database should be changed, but it was not.");

            // Assert user is logged-in.
            Assert.IsTrue(
                controller.HttpContext.Response.Cookies.Keys.Cast<string>().Contains(".ASPXAUTH"),
                "Authentication cookie should be present in the Response.");

            var authCookie = controller.HttpContext.Response.Cookies[".ASPXAUTH"];
            Assert.IsNotNull(authCookie, @"Response.Cookies["".ASPXAUTH""] must not be null.");
            Assert.IsTrue(authCookie.Expires > utcNow, "Cookie expire date must be set to the future.");

            var ticket = System.Web.Security.FormsAuthentication.Decrypt(authCookie.Value);
            Assert.AreEqual("andrerpena", ticket.Name);
            Assert.IsTrue(ticket.Expiration > utcNow, "Ticket expire date must be set to the future.");
            Assert.IsTrue(ticket.IsPersistent, "Ticket must be persistent.");

            var token = SecurityTokenHelper.FromString(ticket.UserData);
            Assert.AreEqual(user.Id, token.UserData.Id);
            Assert.AreEqual("André Pena", token.UserData.FullName);
            Assert.AreEqual("andrerpena@gmail.com", token.UserData.Email);
            Assert.AreEqual("consultoriodrhouse", token.UserData.PracticeIdentifier);
            Assert.AreEqual(false, token.UserData.IsUsingDefaultPassword);
        }

        /// <summary>
        /// Tests the login with the remember-me checkbox unset.
        /// </summary>
        [TestMethod]
        public void Login_NotRemeberMe_HappyPath()
        {
            AuthenticationController controller;
            LoginViewModel vm;

            var utcNow = new DateTime(2012, 08, 31, 0, 0, 0, DateTimeKind.Utc);

            try
            {
                var mr = new MockRepository();
                controller = mr.CreateController<AuthenticationController>();
                controller.UtcNowGetter = () => utcNow;

                // Creating ViewModel, and setting the ModelState of the controller.
                vm = new LoginViewModel
                {
                    UserNameOrEmail = "andrerpena",
                    PracticeIdentifier = "consultoriodrhouse",
                    Password = "ph4r40h",
                    RememberMe = false,
                };
                Mvc3TestHelper.SetModelStateErrors(controller, vm);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Creating a new user without an e-mail.
            // This must be ok, no exceptions, no validation errors.

            {
                controller.Login(vm);
            }

            // Assert user is logged-in.
            Assert.IsTrue(
                controller.HttpContext.Response.Cookies.Keys.Cast<string>().Contains(".ASPXAUTH"),
                "Authentication cookie should be present in the Response.");

            var authCookie = controller.HttpContext.Response.Cookies[".ASPXAUTH"];
            Assert.IsNotNull(authCookie, @"Response.Cookies["".ASPXAUTH""] must not be null.");
            Assert.IsTrue(authCookie.Expires == DateTime.MinValue, "Cookie expire date must be set to DateTime.MinValue.");

            var ticket = System.Web.Security.FormsAuthentication.Decrypt(authCookie.Value);
            Assert.AreEqual("andrerpena", ticket.Name);
            Assert.IsTrue(ticket.Expiration > utcNow, "Ticket expire date must be set to the future.");
            Assert.IsFalse(ticket.IsPersistent, "Ticket must not be persistent.");
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
                using (var db2 = CreateNewCerebelloEntities())
                {
                    var user = db2.Users.Single(u => u.Id == userId);
                    mr.SetCurrentUser(user, password);
                    mr.SetRouteData("Any", "Practice", null, user.Practice.UrlIdentifier);

                    practiceName = user.Practice.UrlIdentifier;
                }

                controller = mr.CreateController<AuthenticationController>();
                controller.UtcNowGetter = () => utcNow;
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            ActionResult actionResult;
            {
                actionResult = controller.VerifyPracticeAndEmail(
                    new VerifyPracticeAndEmailViewModel { Token = token, PracticeIdentifier = practiceName, });
            }

            // Asserting.
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult));
            var redirectToRouteResult = (RedirectToRouteResult)actionResult;
            Assert.AreEqual("Welcome", redirectToRouteResult.RouteValues["action"]);
            Assert.AreEqual("Home", redirectToRouteResult.RouteValues["controller"]);
            Assert.AreEqual("", redirectToRouteResult.RouteValues["area"]);
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
                using (var db2 = CreateNewCerebelloEntities())
                {
                    var user = db2.Users.Single(u => u.Id == userId);
                    mr.SetCurrentUser(user, password);
                    mr.SetRouteData("Any", "Practice", null, user.Practice.UrlIdentifier);

                    practiceName = user.Practice.UrlIdentifier;
                }

                controller = mr.CreateController<AuthenticationController>();
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            ActionResult actionResult;
            {
                actionResult = controller.VerifyPracticeAndEmail(
                    new VerifyPracticeAndEmailViewModel { PracticeIdentifier = practiceName });
            }

            // Asserting.
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.IsInstanceOfType(viewResult.Model, typeof(VerifyPracticeAndEmailViewModel));
            var model = (VerifyPracticeAndEmailViewModel)viewResult.Model;

            //// ATENTION: The value of the token must NEVER go out to a view.
            //Assert.AreEqual(null, model.Token);

            // ATENTION: The value of the password must NEVER go out to a view.
            Assert.AreEqual(null, model.Password);

            Assert.AreEqual(practiceName, model.PracticeIdentifier);
        }

        /// <summary>
        /// Tries to verify a recently created account with an invalid token.
        /// This is not a valid operations, and must return a validation message.
        /// </summary>
        [TestMethod]
        public void VerifyPracticeAndEmail_InvalidToken_ValidPractice()
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
                using (var db2 = CreateNewCerebelloEntities())
                {
                    var user = db2.Users.Single(u => u.Id == userId);
                    mr.SetCurrentUser(user, password);
                    mr.SetRouteData("Any", "Practice", null, user.Practice.UrlIdentifier);

                    practiceName = user.Practice.UrlIdentifier;
                }

                controller = mr.CreateController<AuthenticationController>();
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            ActionResult actionResult;
            {
                actionResult = controller.VerifyPracticeAndEmail(
                    new VerifyPracticeAndEmailViewModel { Token = "Invalid-Token-Value", PracticeIdentifier = practiceName });
            }

            // Asserting.
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.IsInstanceOfType(viewResult.Model, typeof(VerifyPracticeAndEmailViewModel));
            var model = (VerifyPracticeAndEmailViewModel)viewResult.Model;

            //// ATENTION: The value of the token must NEVER go out to a view.
            //Assert.AreEqual(null, model.Token);

            // ATENTION: The value of the password must NEVER go out to a view.
            Assert.AreEqual(null, model.Password);

            Assert.AreEqual(practiceName, model.PracticeIdentifier);

            // Asserting ModelState.
            Assert.IsTrue(controller.ModelState.ContainsKey("Token"), "ModelState must containt an entry for 'Token'.");
            Assert.AreEqual(1, controller.ModelState["Token"].Errors.Count);
            Assert.AreEqual("Problema com o token.", controller.ModelState["Token"].Errors.First().ErrorMessage);
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
            var utcNow = new DateTime(2012, 08, 31, 0, 0, 0, DateTimeKind.Utc);
            string practiceName;
            string token;

            try
            {
                // Simulating account creation.
                string password;
                var userId = CreateAccount_Helper(utcNow.AddDays(-200), out password, out token);

                var mr = new MockRepository();

                // Login-in the user that has just been created.
                using (var db2 = CreateNewCerebelloEntities())
                {
                    var user = db2.Users.Single(u => u.Id == userId);
                    mr.SetCurrentUser(user, password);
                    mr.SetRouteData("Any", "Practice", null, user.Practice.UrlIdentifier);

                    practiceName = user.Practice.UrlIdentifier;
                }

                controller = mr.CreateController<AuthenticationController>();
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            ActionResult actionResult;
            {
                actionResult = controller.VerifyPracticeAndEmail(
                    new VerifyPracticeAndEmailViewModel { Token = token, PracticeIdentifier = practiceName });
            }

            // Asserting.
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.IsInstanceOfType(viewResult.Model, typeof(VerifyPracticeAndEmailViewModel));
            var model = (VerifyPracticeAndEmailViewModel)viewResult.Model;

            //// ATENTION: The value of the token must NEVER go out to a view.
            //Assert.AreEqual(null, model.Token);

            // ATENTION: The value of the password must NEVER go out to a view.
            Assert.AreEqual(null, model.Password);

            Assert.AreEqual(practiceName, model.PracticeIdentifier);

            // Asserting ModelState.
            Assert.IsTrue(controller.ModelState.ContainsKey("Token"), "ModelState must containt an entry for 'Token'.");
            Assert.AreEqual(1, controller.ModelState["Token"].Errors.Count);
            Assert.AreEqual("Problema com o token.", controller.ModelState["Token"].Errors.First().ErrorMessage);
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
                using (var db2 = CreateNewCerebelloEntities())
                {
                    var user = db2.Users.Single(u => u.Id == userId);
                    mr.SetCurrentUser(user, password);
                    mr.SetRouteData("Any", "Practice", null, user.Practice.UrlIdentifier);

                    practiceName = user.Practice.UrlIdentifier;
                }

                controller = mr.CreateController<PracticeController>(callOnActionExecuting: false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            ActionResult actionResult;
            {
                actionResult = Mvc3TestHelper.RunOnActionExecuting(controller, mr);
            }

            // Asserting.
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult));
            var redirectToRouteResult = (RedirectToRouteResult)actionResult;
            Assert.AreEqual("CreateAccountCompleted", redirectToRouteResult.RouteValues["action"]);
            Assert.AreEqual("Authentication", redirectToRouteResult.RouteValues["controller"]);
            Assert.AreEqual("", redirectToRouteResult.RouteValues["area"]);
            Assert.AreEqual(practiceName, redirectToRouteResult.RouteValues["practice"]);
            Assert.AreEqual(true, redirectToRouteResult.RouteValues["mustValidateEmail"]);
        }

        /// <summary>
        /// This is a valid operation, and the controller OnActionExecuting should not return an action result,
        /// and let the action being executed.
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
                using (var db2 = CreateNewCerebelloEntities())
                {
                    var user = db2.Users.Single(u => u.Id == userId);
                    mr.SetCurrentUser(user, password);
                    mr.SetRouteData("Any", "Practice", null, user.Practice.UrlIdentifier);

                    practiceName = user.Practice.UrlIdentifier;
                }

                // Verifying the new account.
                // Note: the following AuthenticationController is being
                // setup with an invalid MockRepository for it,
                // however this does not prevent proper operation.
                var authController = mr.CreateController<AuthenticationController>();
                authController.UtcNowGetter = () => utcNow.AddDays(15.0); // this is up to 30 days
                authController.VerifyPracticeAndEmail(
                    new VerifyPracticeAndEmailViewModel { Token = token, PracticeIdentifier = practiceName });

                Assert.IsTrue(authController.ModelState.IsValid, "Could not validate email.");

                controller = mr.CreateController<PracticeController>(callOnActionExecuting: false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            ActionResult actionResult;
            {
                // The controller self-filter should let the action being executed,
                // so the resulting actionResult from OnActionExecuting, must be null.
                actionResult = Mvc3TestHelper.RunOnActionExecuting(controller, mr);
            }

            // Asserting.
            Assert.IsNull(actionResult);
        }
        #endregion

        /// <summary>
        /// Simulates the creation of a new account,
        /// by using the real controller,
        /// and mocking everything that is of no interest.
        /// </summary>
        /// <param name="utcNow"></param>
        /// <param name="password"> </param>
        /// <param name="outToken"> </param>
        private static int CreateAccount_Helper(DateTime utcNow, out string password, out string outToken)
        {
            using (var disposer = new Disposer())
            {
                var mr = new MockRepository();

                string token = null;
                var mve = mr.SetupViewEngine(disposer);
                mve.SetViewContent(
                    "ConfirmationEmail",
                    vc =>
                    {
                        token = ((UserEmailViewModel)vc.ViewData.Model).Token;
                        return "Fake e-mail message.";
                    });

                mr.SetupHttpContext(disposer);

                var controller = mr.CreateController<AuthenticationController>();

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
                        PracticeName = "consultoriodrhouse_08sd986",
                        Password = password,
                        ConfirmPassword = password,
                        DateOfBirth = new DateTime(1984, 05, 04),
                        EMail = "andre@fakemail.com",
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
                Assert.IsNotNull(authCookie, @"Response.Cookies["".ASPXAUTH""] must not be null.");
                var ticket = System.Web.Security.FormsAuthentication.Decrypt(authCookie.Value);
                var securityToken = SecurityTokenHelper.FromString(ticket.UserData);

                return securityToken.UserData.Id;
            }
        }

        #region Reset passwrod
        /// <summary>
        /// Tests whether the reset password request is working.
        /// To request a password reset, the user needs to enter the practice-name and the user-name/e-mail.
        /// </summary>
        [TestMethod]
        public void ResetPasswordRequest_HappyPath()
        {
            using (var disposer = new Disposer())
            {
                AuthenticationController controller;
                var utcNow = new DateTime(2012, 10, 22, 23, 30, 0, DateTimeKind.Utc);
                var hasBeenSaved = false;
                var wasEmailSent = false;

                User user;
                string emailToken = null;
                string emailBody = null;
                string emailToAddress = null;
                string emailSubject = null;
                try
                {
                    user = this.db.Users.Single(x => x.UserName == "andrerpena");

                    var mr = new MockRepository();

                    var mve = mr.SetupViewEngine(disposer);
                    mve.SetViewContent(
                        "ResetPasswordEmail",
                        vc =>
                        {
                            emailToken = ((UserEmailViewModel)vc.ViewData.Model).Token;
                            return vc.ViewData.Model.ConvertObjectToString("<div>{0}={1}</div>");
                        });

                    controller = mr.CreateController<AuthenticationController>(
                        setupNewDb: db1 => { db1.SavingChanges += (s, e) => hasBeenSaved = true; });
                    controller.UtcNowGetter = () => utcNow;

                    controller.EmailSender += mm =>
                    {
                        wasEmailSent = true;
                        emailBody = mm.Body;
                        emailSubject = mm.Subject;
                        emailToAddress = mm.To.Single().Address;
                    };
                }
                catch (Exception ex)
                {
                    InconclusiveInit(ex);
                    return;
                }

                ActionResult actionResult;
                {
                    actionResult = controller.ResetPasswordRequest(
                        new IdentityViewModel
                            {
                                PracticeIdentifier = user.Practice.UrlIdentifier,
                                UserNameOrEmail = user.UserName,
                            });
                }

                using (var dbs = CreateNewCerebelloEntities())
                {
                    // Asserting.
                    Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

                    Assert.IsNotNull(actionResult);
                    Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult));
                    var redirectToRouteResult = (RedirectToRouteResult)actionResult;
                    Assert.AreEqual("ResetPasswordEmailSent", redirectToRouteResult.RouteValues["action"]);
                    Assert.AreEqual(null, redirectToRouteResult.RouteValues["controller"]);
                    Assert.AreEqual(null, redirectToRouteResult.RouteValues["area"]);

                    // Assert DB values.
                    Assert.IsTrue(hasBeenSaved, "The database has not been changed. This was supposed to happen.");
                    var tokenId = new TokenId(emailToken);
                    var savedToken = dbs.GLB_Token.Single(tk => tk.Id == tokenId.Id);
                    Assert.AreEqual(32, savedToken.Value.Length);
                    Assert.AreEqual(tokenId.Value, savedToken.Value);
                    Assert.AreEqual(utcNow.AddDays(30), savedToken.ExpirationDate);

                    // Assertion for email.
                    Assert.IsTrue(wasEmailSent, "E-mail was not sent, but it should.");
                    var emailViewModel = new UserEmailViewModel(user) { Token = emailToken, };
                    var emailExpected = emailViewModel.ConvertObjectToString("<div>{0}={1}</div>");
                    Assert.AreEqual(emailExpected, emailBody);
                    Assert.AreEqual("Redefinir senha da conta no Cerebello", emailSubject);
                    Assert.AreEqual("andrerpena@gmail.com", emailToAddress);
                }
            }
        }

        /// <summary>
        /// Tests whether the reset password command is working.
        /// This is going to test if the user can loggin after the password has been reset.
        /// The whole process will be simulated, from the passwrod reset request to the login.
        /// </summary>
        [TestMethod]
        public void ResetPassword_HappyPath()
        {
            using (var disposer = new Disposer())
            {
                var utcNow = new DateTime(2012, 10, 22, 23, 30, 0, DateTimeKind.Utc);

                string emailToken = null;
                User user;
                AuthenticationController controller;
                try
                {
                    user = this.db.Users.Single(x => x.UserName == "andrerpena");

                    var mr = new MockRepository();

                    var mve = mr.SetupViewEngine(disposer);
                    mve.SetViewContent("ResetPasswordEmail",
                                       vc =>
                                       {
                                           emailToken = ((UserEmailViewModel)vc.ViewData.Model).Token;
                                           return "Fake e-mail contents!";
                                       });

                    var controller0 = mr.CreateController<AuthenticationController>();
                    controller0.UtcNowGetter = () => utcNow;

                    // requesting password reset
                    controller0.ResetPasswordRequest(
                        new IdentityViewModel
                            {
                                PracticeIdentifier = user.Practice.UrlIdentifier,
                                UserNameOrEmail = user.UserName,
                            });

                    var mr1 = new MockRepository();
                    controller = mr1.CreateController<AuthenticationController>();
                    controller.UtcNowGetter = () => utcNow;
                }
                catch (Exception ex)
                {
                    InconclusiveInit(ex, "1st test initialization has failed");
                    return;
                }

                // reseting the password
                ActionResult actionResult;
                {
                    actionResult = controller.ResetPassword(
                        new ResetPasswordViewModel
                            {
                                Token = emailToken,
                                NewPassword = "pharaoh",
                                ConfirmNewPassword = "pharaoh",
                                PracticeIdentifier = user.Practice.UrlIdentifier,
                                UserNameOrEmail = user.UserName,
                            });
                }

                // Asserting action result.
                Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult));
                var redirectResult = (RedirectToRouteResult)actionResult;
                Assert.AreEqual("ResetPasswordSuccess", redirectResult.RouteValues["action"]);

                // Asserting user can login with the new password.
                ActionResult loginActionResult;
                try
                {
                    var mr2 = new MockRepository();
                    var controller2 = mr2.CreateController<AuthenticationController>();
                    controller2.UtcNowGetter = () => utcNow;
                    loginActionResult = controller2.Login(
                        new LoginViewModel
                            {
                                PracticeIdentifier = user.Practice.UrlIdentifier,
                                UserNameOrEmail = user.UserName,
                                Password = "pharaoh",
                            });
                }
                catch (Exception ex)
                {
                    InconclusiveInit(ex, "2nd test initialization has failed");
                    return;
                }

                Assert.IsInstanceOfType(loginActionResult, typeof(RedirectToRouteResult));
                var redirectToRouteResult = (RedirectToRouteResult)loginActionResult;
                Assert.AreEqual("Index", (string)redirectToRouteResult.RouteValues["action"], ignoreCase: true);
                Assert.AreEqual("PracticeHome", (string)redirectToRouteResult.RouteValues["controller"], ignoreCase: true);
                Assert.AreEqual("App", (string)redirectToRouteResult.RouteValues["area"], ignoreCase: true);
            }
        }

        #endregion
    }
}
