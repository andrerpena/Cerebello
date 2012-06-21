using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cerebello.Model;
using System.Configuration;
using Test1;
using Cerebello.Firestarter;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Code.Json;
using CerebelloWebRole.Code.Controls;
using CerebelloWebRole.Models;
using System.Web.Mvc;

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
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
            }
            catch
            {
                Assert.Inconclusive("Firestarter has failed.");
            }

            // Creating a new user with the same UserName of another user in the same practice.
            UsersController controller;
            ActionResult actionResult;

            {
                controller = ControllersRepository.CreateControllerForTesting<UsersController>(this.db);
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
                MockRepository.SetCurrentUser_Andre_Valid();
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
                MockRepository.SetCurrentUser_Andre_Valid();
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
                MockRepository.SetCurrentUser_Andre_Valid();
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
            int userId;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var s = Firestarter.CreateSecretary_Milena(this.db, this.db.Practices.ToList().Last());
                userId = s.Users.Single().Id;
            }
            catch
            {
                Assert.Inconclusive("Firestarter has failed.");
                return;
            }

            // Creating a new user without an e-mail.
            // This must be ok, no exceptions, no validation errors.
            {
                UsersController controller = ControllersRepository.CreateControllerForTesting<UsersController>(this.db);
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
    }
}
