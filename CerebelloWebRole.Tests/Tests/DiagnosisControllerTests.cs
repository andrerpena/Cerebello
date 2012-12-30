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
        public override void TestInitialize()
        {
            base.TestInitialize();
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

            var formModel = new DiagnosisViewModel
                                {
                                    PatientId = patientId,
                                };

            var mr = new MockRepository(true);
            var controller = mr.CreateController<DiagnosisController>();
            controller.Edit(new[] { formModel });

            Assert.IsFalse(controller.ModelState.IsValid);
        }

        [TestMethod]
        public void Edit_HappyPath()
        {
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db, 1);
            this.db.SaveChanges();
            var patientId = this.db.Patients.First().Id;

            var formModel = new DiagnosisViewModel
                                {
                                    PatientId = patientId,
                                    Cid10Code = "CodeX",
                                    Cid10Name = "XDesease",
                                    Text = "Notes"
                                };

            var mr = new MockRepository(true);
            var controller = mr.CreateController<DiagnosisController>();

            var referenceTime = DateTime.UtcNow;
            controller.UtcNowGetter = () => referenceTime;

            controller.Edit(new[] { formModel });

            Assert.IsTrue(controller.ModelState.IsValid);

            var diagnosis = this.db.Diagnoses.First();
            Assert.AreEqual(0, (referenceTime.Ticks - diagnosis.CreatedOn.Ticks) / 100000);
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

            var formModel = new DiagnosisViewModel
                                {
                                    PatientId = patientId,
                                    Text = "This is my diagnosis",
                                    Cid10Code = "Q878",
                                    Cid10Name = "Doença X"
                                };

            var mr = new MockRepository(true);
            var controller = mr.CreateController<DiagnosisController>();
            controller.Create(new[] { formModel });

            Assert.IsTrue(controller.ModelState.IsValid);

            // get's the newly created diagnosis
            var newlyCreatedDiagnosis = this.db.Diagnoses.First();

            // tries to delete the anamnese
            var result = controller.Delete(newlyCreatedDiagnosis.Id);
            var deleteMessage = (JsonDeleteMessage)result.Data;

            Assert.AreEqual(true, deleteMessage.success);
            Assert.AreEqual(0, this.db.Anamnese.Count());
        }

        [TestMethod]
        public void Delete_ShouldReturnProperResultWhenNotExisting()
        {
            var mr = new MockRepository(true);
            var controller = mr.CreateController<DiagnosisController>();

            // tries to delete the anamnese
            var result = controller.Delete(999);
            var deleteMessage = (JsonDeleteMessage)result.Data;

            Assert.AreEqual(false, deleteMessage.success);
            Assert.IsNotNull(deleteMessage.text);
        }
    }
}
