using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cerebello.Model;
using System.Configuration;
using Cerebello.Firestarter;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Code.Controls;
using System.Web.Mvc;
using CerebelloWebRole.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Areas.App.Models;

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
                    DateOfBirth = new DateTime(1982, 10, 12),
                    MaritalStatus = (int)TypeMaritalStatus.Casado,
                    BirthPlace = "Brasileiro",
                    CPF = "87324128910",
                    CPFOwner = (int)TypeCPFOwner.PatientItself,
                    Profession = "Encarregado de Obras",
                    UrlIdentifier = StringHelper.GenerateUrlIdentifier("Joao Manuel da Silva"),
                    CreatedOn = DateTime.UtcNow
                },
                Doctor = this.db.Doctors.First()
            };
            patient1.Person.Emails.Add(new Email() { Address = "joao@gmail.com" });
            patient1.Person.Addresses.Add(new Address()
            {
                CEP = "602500330",
                StateProvince = "RJ",
                City = "Rio de Janeiro",
                Neighborhood = "Jacarepaguá",
                Street = "Rua Estrada do Pau Ferro 329",
                Complement = ""
            });

            db.Patients.AddObject(patient1);

            Patient patient2 = new Patient()
            {
                Person = new Person()
                {
                    FullName = "Manuela Moreira da Silva",
                    Gender = (int)TypeGender.Female,
                    DateOfBirth = new DateTime(1982, 10, 12),
                    MaritalStatus = (int)TypeMaritalStatus.Casado,
                    BirthPlace = "Brasileiro",
                    CPF = "87324128910",
                    CPFOwner = (int)TypeCPFOwner.PatientItself,
                    Profession = "Encarregado de Obras",
                    UrlIdentifier = StringHelper.GenerateUrlIdentifier("Manuela Moreira da Silva"),
                    CreatedOn = DateTime.UtcNow
                },
                Doctor = this.db.Doctors.First()
            };
            patient1.Person.Emails.Add(new Email() { Address = "manuela@gmail.com" });
            patient1.Person.Addresses.Add(new Address()
            {
                CEP = "602500330",
                StateProvince = "RJ",
                City = "Rio de Janeiro",
                Neighborhood = "Jacarepaguá",
                Street = "Rua Estrada do Pau Ferro 329",
                Complement = ""
            });

            db.Patients.AddObject(patient2);

            this.db.SaveChanges();

            var controller = ControllersRepository.CreateControllerForTesting<AppController>(this.db);
            var controllerResult = controller.LookupEverything("Joao", 20, 1, this.db.Doctors.First().Id);

            var controllerResultAsLookupResult = (LookupJsonResult)controllerResult.Data;

            Assert.AreEqual(1, controllerResultAsLookupResult.Rows.Count);
            Assert.IsInstanceOfType(controllerResultAsLookupResult.Rows[0], typeof(GlobalSearchViewModel));

            Assert.AreEqual(patient1.Person.FullName, ((GlobalSearchViewModel)controllerResultAsLookupResult.Rows[0]).Value);
            Assert.AreEqual(patient1.Id, ((GlobalSearchViewModel)controllerResultAsLookupResult.Rows[0]).Id);
        }
    }
}

