using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cerebello.Model;
using Cerebello.Firestarter;
using CerebelloWebRole.Areas.App.Controllers;
using System.Web.Mvc;
using CerebelloWebRole.Areas.App.Models;

namespace CerebelloWebRole.Tests.Tests
{
    [TestClass]
    public class PatientsControllerTests
    {
        #region TEST_SETUP
        protected CerebelloEntities db = null;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            DatabaseHelper.AttachCerebelloTestDatabase();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            DatabaseHelper.DetachCerebelloTestDatabase();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            this.db = new CerebelloEntities(string.Format("name={0}", Constants.CONNECTION_STRING_EF));

            Firestarter.ClearAllData(this.db);
            Firestarter.InitializeDatabaseWithSystemData(this.db);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            this.db.Dispose();
        }
        #endregion

        #region Search

        [TestMethod]
        public void Search_ShouldReturnEverythingInEmptySearch()
        {
            PatientsController controller;

            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = Mvc3TestHelper.CreateControllerForTesting<PatientsController>(this.db, mr);
                Firestarter.CreateFakePatients(doctor, this.db, 100);
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            var patientsCount = this.db.Patients.Count();

            // making an empty search
            var result = controller.Search(new Areas.App.Models.SearchModel()
            {
                Term = "",
                Page = 1
            });

            var resultAsView = result as ViewResult;
            Assert.IsNotNull(resultAsView);

            var model = resultAsView.Model as SearchViewModel<PatientViewModel>;
            Assert.IsNotNull(model);

            Assert.AreEqual(100, model.Count);
            Assert.AreEqual(Code.Constants.GRID_PAGE_SIZE, model.Objects.Count);
        }

        [TestMethod]
        public void Search_ShouldRespectTheSearchTermWhenItsPresent()
        {
            PatientsController controller = null;

            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = Mvc3TestHelper.CreateControllerForTesting<PatientsController>(this.db, mr);
                Firestarter.CreateFakePatients(doctor, this.db, 200);
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
            }

            var searchTerm = "an";
            var matchingPatientsCount = this.db.Patients.Count(p => p.Person.FullName.Contains(searchTerm));

            // making an empty search
            var result = controller.Search(new Areas.App.Models.SearchModel()
            {
                Term = searchTerm,
                Page = 1
            });

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var resultAsView = result as ViewResult;

            Assert.IsInstanceOfType(resultAsView.Model, typeof(SearchViewModel<PatientViewModel>));
            var model = resultAsView.Model as SearchViewModel<PatientViewModel>;

            Assert.AreEqual(matchingPatientsCount, model.Count);
            Assert.IsTrue(model.Objects.Count >= CerebelloWebRole.Code.Constants.GRID_PAGE_SIZE);
        }

        [TestMethod]
        public void Delete_HappyPath_WhenTheresNoAssociation()
        {
            PatientsController controller;

            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = Mvc3TestHelper.CreateControllerForTesting<PatientsController>(this.db, mr);
                Firestarter.CreateFakePatients(doctor, this.db, 1);
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // we now have 1 patient
            var patient = this.db.Patients.FirstOrDefault();
            Assert.IsNotNull(patient);
            var patientId = patient.Id;
            controller.Delete(patientId);

            // this patient must have been deleted
            patient = this.db.Patients.FirstOrDefault(p => p.Id == patientId);
            Assert.IsNull(patient);
        }

        [TestMethod]
        public void Delete_HappyPath_WhenTheresAnAnamnese()
        {
            PatientsController controller;

            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = Mvc3TestHelper.CreateControllerForTesting<PatientsController>(this.db, mr);
                Firestarter.CreateFakePatients(doctor, this.db, 1);
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // we now have 1 patient
            var patient = this.db.Patients.FirstOrDefault();
            Assert.IsNotNull(patient);

            var patientId = patient.Id;

            // now, let's add an anamnese
            var anamnese = new Anamnese()
                {
                    PatientId = patientId,
                    CreatedOn = DateTime.UtcNow,
                    Text = "This is my anamnese"
                };

            anamnese.Diagnoses.Add(new Diagnosis()
            {
                Cid10Name = "Text",
                Cid10Code = "Q878"
            });

            patient.Anamneses.Add(anamnese);
            this.db.SaveChanges();

            controller.Delete(patientId);

            // this patient must have been deleted
            patient = this.db.Patients.FirstOrDefault(p => p.Id == patientId);
            Assert.IsNull(patient);
        }

        [TestMethod]
        public void Delete_WhenTheresAMedicalCertificate()
        {
            PatientsController controller;
            Doctor doctor;
            Patient patient;

            try
            {
                doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = Mvc3TestHelper.CreateControllerForTesting<PatientsController>(this.db, mr);
                Firestarter.CreateFakePatients(doctor, this.db, 1);

                // we now have 1 patient
                patient = this.db.Patients.FirstOrDefault();
                Assert.IsNotNull(patient);

                var certificateModel = new Cerebello.Model.ModelMedicalCertificate()
                {
                    DoctorId = doctor.Id,
                    Name = "model1",
                    Text = "model1"
                };

                certificateModel.Fields.Add(new ModelMedicalCertificateField()
                {
                    Name = "field1"
                });

                var certificate = new Cerebello.Model.MedicalCertificate()
                {
                    ModelMedicalCertificate = certificateModel,
                    Patient = patient,
                    Text = "text",
                    CreatedOn = DateTime.UtcNow
                };

                certificate.Fields.Add(new MedicalCertificateField()
                {
                    Name = "field1",
                    Value = "value"
                });

                this.db.MedicalCertificates.AddObject(certificate);
                this.db.SaveChanges();
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            controller.Delete(patient.Id);

            // this patient must have been deleted
            patient = this.db.Patients.FirstOrDefault(p => p.Id == patient.Id);
            Assert.IsNull(patient);

        }

        [TestMethod]
        public void Delete_WhenTheresAnAppointment()
        {
            PatientsController controller;
            Doctor docAndre;
            Patient patient;

            try
            {
                docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = Mvc3TestHelper.CreateControllerForTesting<PatientsController>(this.db, mr);
                Firestarter.CreateFakePatients(docAndre, this.db, 1);

                // we now have 1 patient
                patient = this.db.Patients.FirstOrDefault();
                Assert.IsNotNull(patient);
                var referenceTime = DateTime.UtcNow;

                var appointment = new Appointment()
                {
                    Doctor = docAndre,
                    CreatedBy = docAndre.Users.First(),
                    CreatedOn = referenceTime,
                    PatientId = patient.Id,
                    Start = referenceTime,
                    End = referenceTime + TimeSpan.FromMinutes(30)
                };

                this.db.Appointments.AddObject(appointment);
                this.db.SaveChanges();
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            controller.Delete(patient.Id);

            // this patient must have been deleted
            patient = this.db.Patients.FirstOrDefault(p => p.Id == patient.Id);
            Assert.IsNull(patient);

        }

        #endregion
    }
}
