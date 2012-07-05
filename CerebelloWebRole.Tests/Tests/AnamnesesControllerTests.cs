using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Cerebello.Firestarter;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code.Controls;
using CerebelloWebRole.Code.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            this.db = new CerebelloEntities(string.Format("name={0}", Constants.CONNECTION_STRING_EF));

            Firestarter.ClearAllData(this.db);
            Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
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
        public void Edit_1_CreateDiagnosisIfItDoesNotExist()
        {
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db);
            this.db.SaveChanges();
            var patientId = this.db.Patients.First().Id;

            AnamneseViewModel formModel = new AnamneseViewModel()
            {
                PatientId = patientId,
                Text = "This is my anamnese",
                Diagnoses = new List<DiagnosisViewModel>() {
                        new DiagnosisViewModel() { Text = "Text", Cid10Code = "Q878" },
                        new DiagnosisViewModel() { Text = "Text2", Cid10Code = "Q879" }
                   }
            };

            var mr = new MockRepository();
            var controller = Mvc3TestHelper.CreateControllerForTesting<AnamnesesController>(this.db, mr);
            controller.Create(formModel);

            Assert.IsTrue(controller.ModelState.IsValid);

            var anamneses = this.db.Anamnese.ToList();
            var diagnoses = this.db.Diagnoses.ToList();

            Assert.AreEqual(1, anamneses.Count);
            Assert.AreEqual(2, diagnoses.Count);

            Assert.AreEqual(formModel.Diagnoses[0].Text, anamneses[0].Diagnoses.ElementAt(0).Cid10Name);
            Assert.AreEqual(formModel.Diagnoses[0].Cid10Code, anamneses[0].Diagnoses.ElementAt(0).Cid10Code);

            Assert.AreEqual(formModel.Diagnoses[1].Text, anamneses[0].Diagnoses.ElementAt(1).Cid10Name);
            Assert.AreEqual(formModel.Diagnoses[1].Cid10Code, anamneses[0].Diagnoses.ElementAt(1).Cid10Code);
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
                Diagnoses = new List<DiagnosisViewModel>() {
                        new DiagnosisViewModel() { Text = "Text", Cid10Code = "Q878" },
                        new DiagnosisViewModel() { Text = "Text2", Cid10Code = "Q879" }
                   }
            };

            var mr = new MockRepository();
            var controller = Mvc3TestHelper.CreateControllerForTesting<AnamnesesController>(this.db, mr);
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
            var mr = new MockRepository();
            var controller = Mvc3TestHelper.CreateControllerForTesting<AnamnesesController>(this.db, mr);

            // tries to delete the anamnese
            var result = controller.Delete(999);
            JsonDeleteMessage deleteMessage = (JsonDeleteMessage)result.Data;

            Assert.AreEqual(false, deleteMessage.success);
            Assert.IsNotNull(deleteMessage.text);
        }

        [TestMethod]
        public void LookupDiagnoses_1_ShouldReturnTheProperResult()
        {
            var mr = new MockRepository();
            var controller = Mvc3TestHelper.CreateControllerForTesting<AnamnesesController>(this.db, mr);

            // tries to delete the anamnese <== WTF??
            var result = controller.LookupDiagnoses("cefaléia", 20, 1);
            var lookupJsonResult = (LookupJsonResult)result.Data;

            Assert.AreEqual(9, lookupJsonResult.Count);
            foreach (CidLookupGridModel item in lookupJsonResult.Rows)
            {
                Assert.IsNotNull(item.Cid10Code);
                Assert.IsFalse(string.IsNullOrEmpty(item.Cid10Name));
            }
        }

        #endregion
    }
}
