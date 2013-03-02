using System.Collections.Generic;
using System.Linq;
using Cerebello.Firestarter;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code.Controls;
using CerebelloWebRole.Code.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests
{
    [TestClass]
    public class AnamnesesControllerTests : DbTestBase
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

        #region Edit

        [TestMethod]
        public void Edit_HappyPath()
        {
            AnamnesesController controller;
            AnamneseViewModel formModel;
            try
            {
                // obtains a valid patient
                Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db, 1);
                this.db.SaveChanges();
                var patientId = this.db.Patients.First().Id;

                formModel = new AnamneseViewModel()
                    {
                        PatientId = patientId,
                        Conclusion = "This is my anamnese",
                        DiagnosticHypotheses = new List<DiagnosticHypothesisViewModel>()
                            {
                                new DiagnosticHypothesisViewModel() {Text = "Text", Cid10Code = "Q878"},
                                new DiagnosticHypothesisViewModel() {Text = "Text2", Cid10Code = "Q879"}
                            }
                    };

                var mr = new MockRepository(true);
                controller = mr.CreateController<AnamnesesController>();
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // executing the test
            controller.Create(formModel);

            Assert.IsTrue(controller.ModelState.IsValid);

            var anamneses = this.db.Anamnese.ToList();
            var diagnosticHypotheses = this.db.DiagnosticHypotheses.ToList();

            Assert.AreEqual(1, anamneses.Count);
            Assert.AreEqual(2, diagnosticHypotheses.Count);

            Assert.AreEqual(formModel.DiagnosticHypotheses[0].Text, anamneses[0].DiagnosticHypotheses.ElementAt(0).Cid10Name);
            Assert.AreEqual(formModel.DiagnosticHypotheses[0].Cid10Code, anamneses[0].DiagnosticHypotheses.ElementAt(0).Cid10Code);

            Assert.AreEqual(formModel.DiagnosticHypotheses[1].Text, anamneses[0].DiagnosticHypotheses.ElementAt(1).Cid10Name);
            Assert.AreEqual(formModel.DiagnosticHypotheses[1].Cid10Code, anamneses[0].DiagnosticHypotheses.ElementAt(1).Cid10Code);
        }

        #endregion

        #region Delete

        [TestMethod]
        public void Delete_HappyPath()
        {
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db);
            this.db.SaveChanges();
            var patientId = this.db.Patients.First().Id;

            var formModel = new AnamneseViewModel()
            {
                PatientId = patientId,
                Conclusion = "This is my anamnese",
                DiagnosticHypotheses = new List<DiagnosticHypothesisViewModel>() {
                        new DiagnosticHypothesisViewModel() { Text = "Text", Cid10Code = "Q878" },
                        new DiagnosticHypothesisViewModel() { Text = "Text2", Cid10Code = "Q879" }
                   }
            };

            var mr = new MockRepository(true);
            var controller = mr.CreateController<AnamnesesController>();
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
        public void Delete_ShouldReturnProperResultWhenNotExisting()
        {
            var mr = new MockRepository(true);
            var controller = mr.CreateController<AnamnesesController>();

            // tries to delete the anamnese
            var result = controller.Delete(999);
            JsonDeleteMessage deleteMessage = (JsonDeleteMessage)result.Data;

            Assert.AreEqual(false, deleteMessage.success);
            Assert.IsNotNull(deleteMessage.text);
        }

        [TestMethod]
        public void LookupDiagnoses_1_ShouldReturnTheProperResult()
        {
            var mr = new MockRepository(true);
            var controller = mr.CreateController<AnamnesesController>();

            var result = controller.AutocompleteDiagnoses("cefaléia", 20, 1);
            var lookupJsonResult = (AutocompleteJsonResult)result.Data;

            Assert.AreEqual(9, lookupJsonResult.Count);
            foreach (CidAutocompleteGridModel item in lookupJsonResult.Rows)
            {
                Assert.IsNotNull(item.Cid10Code);
                Assert.IsFalse(string.IsNullOrEmpty(item.Cid10Name));
            }
        }

        #endregion
    }
}
