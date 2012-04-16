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

namespace CerebelloWebRole.Tests
{
    [TestClass]
    public class AnamnesesControllerTests
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

        [TestMethod]
        public void Edit_1_CreateIfItDoesntExist()
        {
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db);
            this.db.SaveChanges();
            var patientId = this.db.Patients.First().Id;

            AnamneseViewModel formModel = new AnamneseViewModel()
            {
                PatientId = patientId,
                Text = "This is my anamnese",
                AnamneseSymptoms = new List<AnamneseSymptomViewModel>() {
                        new AnamneseSymptomViewModel() { Text = "NonExistingSymptom1" },
                        new AnamneseSymptomViewModel() { Text = "NonExistingSymptom2" }
                   }
            };

            var controller = ControllersRepository.CreateControllerForTesting<AnamnesesController>(this.db);
            controller.Create(formModel);

            Assert.IsTrue(controller.ModelState.IsValid);

            var anamneses = this.db.Anamnese.ToList();
            var symptoms = this.db.Symptoms.ToList();

            Assert.AreEqual(1, anamneses.Count);
            Assert.AreEqual(2, symptoms.Count);

            Assert.AreEqual(formModel.AnamneseSymptoms[0].Text, anamneses[0].AnamneseSymptoms.ElementAt(0).Symptom.Name);
            Assert.AreEqual(formModel.AnamneseSymptoms[1].Text, anamneses[0].AnamneseSymptoms.ElementAt(1).Symptom.Name);
        }

        [TestMethod]
        public void Edit_2_SymptomsThatExistMustBeReferenced()
        {
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db);
            this.db.SaveChanges();
            var patientId = this.db.Patients.First().Id;

            AnamneseViewModel formModel = new AnamneseViewModel()
            {
                PatientId = patientId,
                Text = "This is my anamnese",
                AnamneseSymptoms = new List<AnamneseSymptomViewModel>() {
                        new AnamneseSymptomViewModel() { Text = "Symptom1" }
                   }
            };

            var controller = ControllersRepository.CreateControllerForTesting<AnamnesesController>(this.db);
            controller.Create(formModel);

            formModel = new AnamneseViewModel()
            {
                PatientId = patientId,
                Text = "This is my OTHER anamnese",
                AnamneseSymptoms = new List<AnamneseSymptomViewModel>() {
                        new AnamneseSymptomViewModel() { Text = "Symptom1" }
                   }
            };

            controller.Edit(formModel);

            Assert.AreEqual(1, this.db.Symptoms.Count());
            Assert.AreEqual("Symptom1", this.db.Symptoms.First().Name);
        }

        #endregion

        #region Delete

        [TestMethod]
        public void Delete_1_HappyPath()
        {
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db);
            this.db.SaveChanges();
            var patientId = this.db.Patients.First().Id;

            AnamneseViewModel formModel = new AnamneseViewModel()
            {
                PatientId = patientId,
                Text = "This is my anamnese",
                AnamneseSymptoms = new List<AnamneseSymptomViewModel>() {
                        new AnamneseSymptomViewModel() { Text = "NonExistingSymptom1" },
                        new AnamneseSymptomViewModel() { Text = "NonExistingSymptom2" }
                   }
            };

            var controller = ControllersRepository.CreateControllerForTesting<AnamnesesController>(this.db);
            controller.Create(formModel);

            Assert.IsTrue(controller.ModelState.IsValid);

            // get's the newly created anamnese
            var newlyCreatedAnamnese = this.db.Anamnese.First();

            // tries to delete the anamnese
            var result = controller.Delete(newlyCreatedAnamnese.Id);
            JsonDeleteMessage deleteMessage = (JsonDeleteMessage)result.Data;

            Assert.AreEqual(true, deleteMessage.success);
            Assert.AreEqual(0, this.db.Anamnese.Count());
        }

        [TestMethod]
        public void Delete_2_ShouldReturnProperResultWhenNotExisting()
        {
            var controller = ControllersRepository.CreateControllerForTesting<AnamnesesController>(this.db);

            // tries to delete the anamnese
            var result = controller.Delete(999);
            JsonDeleteMessage deleteMessage = (JsonDeleteMessage)result.Data;

            Assert.AreEqual(false, deleteMessage.success);
            Assert.IsNotNull(deleteMessage.text);
        }

        #endregion
    }
}
