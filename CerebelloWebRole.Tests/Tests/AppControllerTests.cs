using System;
using System.Linq;
using Cerebello.Firestarter;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests
{
    /// <summary>
    /// Summary description for AppControllerTests
    /// </summary>
    [TestClass]
    public class AppControllerTests : DbTestBase
    {
        #region TEST_SETUP
        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
            AttachCerebelloTestDatabase();
        }

        [ClassCleanup()]
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

        [TestMethod]
        public void LookupEverything_1_ShouldSearchPatients()
        {
            var doctor = this.db.Doctors.First();

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
                    CPFOwner = (int)TypeCpfOwner.PatientItself,
                    Profession = "Encarregado de Obras",
                    CreatedOn = DateTime.UtcNow,
                    PracticeId = doctor.PracticeId,
                },
                Doctor = doctor,
                PracticeId = doctor.PracticeId,
            };
            patient1.Person.Email = "joao@fakemail.com";
            patient1.Person.Addresses.Add(
                new Address
                    {
                        CEP = "602500330",
                        StateProvince = "RJ",
                        City = "Rio de Janeiro",
                        Neighborhood = "Jacarepaguá",
                        Street = "Rua Estrada do Pau Ferro 329",
                        Complement = "",
                        PracticeId = doctor.PracticeId,
                    });

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
                    CPFOwner = (int)TypeCpfOwner.PatientItself,
                    Profession = "Encarregado de Obras",
                    CreatedOn = DateTime.UtcNow,
                    PracticeId = doctor.PracticeId,
                },
                Doctor = doctor
            };
            patient1.Person.Email = "manuela@fakemail.com";
            patient1.Person.Addresses.Add(
                new Address
                    {
                        CEP = "602500330",
                        StateProvince = "RJ",
                        City = "Rio de Janeiro",
                        Neighborhood = "Jacarepaguá",
                        Street = "Rua Estrada do Pau Ferro 329",
                        Complement = "",
                        PracticeId = doctor.PracticeId,
                    });

            db.Patients.AddObject(patient2);

            this.db.SaveChanges();

            var mr = new MockRepository(true);
            var controller = mr.CreateController<AppController>();
            var controllerResult = controller.LookupEverything("Joao", 20, 1, this.db.Doctors.First().Id);

            var controllerResultAsLookupResult = (AutocompleteJsonResult)controllerResult.Data;

            Assert.AreEqual(1, controllerResultAsLookupResult.Rows.Count);
            Assert.IsInstanceOfType(controllerResultAsLookupResult.Rows[0], typeof(GlobalSearchViewModel));

            Assert.AreEqual(patient1.Person.FullName, ((GlobalSearchViewModel)controllerResultAsLookupResult.Rows[0]).Value);
            Assert.AreEqual(patient1.Id, ((GlobalSearchViewModel)controllerResultAsLookupResult.Rows[0]).Id);
        }
    }
}
