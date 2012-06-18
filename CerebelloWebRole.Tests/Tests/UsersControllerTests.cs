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
            // will clear all data and setup initial data again
            DatabaseHelper.ClearAllData();
            this.db = new CerebelloEntities(ConfigurationManager.ConnectionStrings[Constants.CONNECTION_STRING_EF].ConnectionString);

            Firestarter.CreateFakeUserAndPractice(this.db);
            this.db.SaveChanges();
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            this.db.Dispose();
        }
        #endregion

        #region Edit

        public void View_1_HappyPath()
        {
            // obtains a valid user
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db);
        }

        [TestMethod]
        public void Edit_1_CreateDiagnosisIfItDoesNotExist()
        {
        }
        
        #endregion

        #region Delete

        [TestMethod]
        public void Delete_1_HappyPath()
        {
        }

        [TestMethod]
        public void Delete_2_ShouldReturnProperResultWhenNotExisting()
        {
        }

        [TestMethod]
        public void LookupDiagnoses_1_ShouldReturnTheProperResult()
        {
        }

        #endregion
    }
}
