using System;
using System.Linq;
using Cerebello.Firestarter;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests
{
    [TestClass]
    public class DiagnosisControllerTests : DbTestBase
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
        public override void InitializeDb()
        {
            base.InitializeDb();
            Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
        }
        #endregion

        [TestMethod]
        public void Edit_WhenBothCid10AndNotesAreEmpty()
        {
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db, 1);
            this.db.SaveChanges();
            var patientId = this.db.Patients.First().Id;

            var formModel = new DiagnosisViewModel()
            {
                PatientId = patientId,
            };

            var mr = new MockRepository(true);
            var controller = Mvc3TestHelper.CreateControllerForTesting<DiagnosisController>(this.db, mr);
            controller.Edit(formModel);

            Assert.IsFalse(controller.ModelState.IsValid);
        }

        [TestMethod]
        public void Edit_HappyPath()
        {
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db, 1);
            this.db.SaveChanges();
            var patientId = this.db.Patients.First().Id;

            var formModel = new DiagnosisViewModel()
            {
                PatientId = patientId,
                Cid10Code = "CodeX",
                Cid10Name = "XDesease",
                Text = "Notes"
            };

            var mr = new MockRepository(true);
            var controller = Mvc3TestHelper.CreateControllerForTesting<DiagnosisController>(this.db, mr);

            var referenceTime = DateTime.UtcNow;
            controller.UtcNowGetter = () => referenceTime;

            controller.Edit(formModel);

            Assert.IsTrue(controller.ModelState.IsValid);

            var diagnosis = this.db.Diagnoses.First();
            Assert.AreEqual(referenceTime, diagnosis.CreatedOn);
            Assert.AreEqual(patientId, diagnosis.PatientId);
            Assert.AreEqual(formModel.Text, diagnosis.Observations);
            Assert.AreEqual(formModel.Cid10Code, diagnosis.Cid10Code);
            Assert.AreEqual(formModel.Cid10Name, diagnosis.Cid10Name);

        }

        [TestMethod]
        public void Delete_HappyPath()
        {
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db, 1);
            this.db.SaveChanges();
            var patientId = this.db.Patients.First().Id;

            var formModel = new DiagnosisViewModel()
            {
                PatientId = patientId,
                Text = "This is my diagnosis",
                Cid10Code = "Q878",
                Cid10Name = "Doença X"
            };

            var mr = new MockRepository(true);
            var controller = Mvc3TestHelper.CreateControllerForTesting<DiagnosisController>(this.db, mr);
            controller.Create(formModel);

            Assert.IsTrue(controller.ModelState.IsValid);

            // get's the newly created diagnosis
            var newlyCreatedDiagnosis = this.db.Diagnoses.First();

            // tries to delete the anamnese
            var result = controller.Delete(newlyCreatedDiagnosis.Id);
            JsonDeleteMessage deleteMessage = (JsonDeleteMessage)result.Data;

            Assert.AreEqual(true, deleteMessage.success);
            Assert.AreEqual(0, this.db.Anamnese.Count());
        }

        [TestMethod]
        public void Delete_ShouldReturnProperResultWhenNotExisting()
        {
            var mr = new MockRepository(true);
            var controller = Mvc3TestHelper.CreateControllerForTesting<DiagnosisController>(this.db, mr);

            // tries to delete the anamnese
            var result = controller.Delete(999);
            JsonDeleteMessage deleteMessage = (JsonDeleteMessage)result.Data;

            Assert.AreEqual(false, deleteMessage.success);
            Assert.IsNotNull(deleteMessage.text);
        }
    }
}
