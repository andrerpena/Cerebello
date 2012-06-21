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
                Firestarter.CreateFakeUserAndPractice_1(this.db);
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
                    }
                });
            }

            // Verifying the ActionResult, and the DB.
            // - The controller ModelState must have no validation errors related to e-mail.

            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
        }


        /// <summary>
        /// Tests the visualization of a medic.
        /// </summary>
        [TestMethod]
        public void Details_1_ViewSecretary_HappyPath()
        {
            int userId;
            try
            {
                Firestarter.CreateFakeUserAndPractice_1(this.db);
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

                // Verifying the ActionResult, and the DB.
                // - The controller ModelState must have no validation errors related to e-mail.
                Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            }
        }
    }
}
