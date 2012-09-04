using System;
using System.Configuration;
using System.Linq;
using Cerebello.Firestarter;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Controls;
using CerebelloWebRole.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests
{
    /// <summary>
    /// Summary description for AppControllerTests
    /// </summary>
    [TestClass]
    public class AppControllerTests
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
            Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
            this.db.SaveChanges();
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            this.db.Dispose();
        }
        #endregion

        [TestMethod]
        public void LookupEverything_1_ShouldSearchPatients()
        {
            // create some fake patients
            // patient 1
            Patient patient1 = new Patient()
            {
                Person = new Person()
                {
                    FullName = "Joao Manuel da Silva",
                    Gender = (int)TypeGender.Male,
                    DateOfBirth = Firestarter.ConvertFromDefaultToUtc(new DateTime(1982, 10, 12)),
                    MaritalStatus = (int)TypeMaritalStatus.Casado,
                    BirthPlace = "Brasileiro",
                    CPF = "87324128910",
                    CPFOwner = (int)TypeCPFOwner.PatientItself,
                    Profession = "Encarregado de Obras",
                    CreatedOn = DateTime.UtcNow
                },
                Doctor = this.db.Doctors.First()
            };
            patient1.Person.Email = "joao@gmail.com";
            patient1.Person.Address = new Address()
            {
                CEP = "602500330",
                StateProvince = "RJ",
                City = "Rio de Janeiro",
                Neighborhood = "Jacarepaguá",
                Street = "Rua Estrada do Pau Ferro 329",
                Complement = ""
            };

            db.Patients.AddObject(patient1);

            Patient patient2 = new Patient()
            {
                Person = new Person()
                {
                    FullName = "Manuela Moreira da Silva",
                    Gender = (int)TypeGender.Female,
                    DateOfBirth = Firestarter.ConvertFromDefaultToUtc(new DateTime(1982, 10, 12)),
                    MaritalStatus = (int)TypeMaritalStatus.Casado,
                    BirthPlace = "Brasileiro",
                    CPF = "87324128910",
                    CPFOwner = (int)TypeCPFOwner.PatientItself,
                    Profession = "Encarregado de Obras",
                    CreatedOn = DateTime.UtcNow
                },
                Doctor = this.db.Doctors.First()
            };
            patient1.Person.Email = "manuela@gmail.com";
            patient1.Person.Address = new Address()
            {
                CEP = "602500330",
                StateProvince = "RJ",
                City = "Rio de Janeiro",
                Neighborhood = "Jacarepaguá",
                Street = "Rua Estrada do Pau Ferro 329",
                Complement = ""
            };

            db.Patients.AddObject(patient2);

            this.db.SaveChanges();

            var mr = new MockRepository(true);
            var controller = Mvc3TestHelper.CreateControllerForTesting<AppController>(this.db, mr);
            var controllerResult = controller.LookupEverything("Joao", 20, 1, this.db.Doctors.First().Id);

            var controllerResultAsLookupResult = (LookupJsonResult)controllerResult.Data;

            Assert.AreEqual(1, controllerResultAsLookupResult.Rows.Count);
            Assert.IsInstanceOfType(controllerResultAsLookupResult.Rows[0], typeof(GlobalSearchViewModel));

            Assert.AreEqual(patient1.Person.FullName, ((GlobalSearchViewModel)controllerResultAsLookupResult.Rows[0]).Value);
            Assert.AreEqual(patient1.Id, ((GlobalSearchViewModel)controllerResultAsLookupResult.Rows[0]).Id);
        }
    }
}

