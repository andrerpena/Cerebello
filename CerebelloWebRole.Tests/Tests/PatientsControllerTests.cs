using System;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Firestarter;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests
{
    [TestClass]
    public class PatientsControllerTests : DbTestBase
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
        #endregion

        [TestMethod]
        public void Search_ShouldReturnEverythingInEmptySearch()
        {
            PatientsController controller;

            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = mr.CreateController<PatientsController>();
                Firestarter.CreateFakePatients(doctor, this.db, 100);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
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
                controller = mr.CreateController<PatientsController>();
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
            Assert.IsTrue(model.Objects.Count >= Code.Constants.GRID_PAGE_SIZE);
        }

        [TestMethod]
        public void Delete_WhenTheresNoAssociation()
        {
            PatientsController controller;
            int patientId;
            Patient patient;

            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = mr.CreateController<PatientsController>();
                Firestarter.CreateFakePatients(doctor, this.db, 1);

                // we now have 1 patient
                patient = this.db.Patients.FirstOrDefault();
                Assert.IsNotNull(patient);
                patientId = patient.Id;
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            controller.Delete(patientId);

            // this patient must have been deleted
            patient = this.db.Patients.FirstOrDefault(p => p.Id == patientId);
            Assert.IsNull(patient);
        }

        [TestMethod]
        public void Delete_WhenTheresAnAnamnese()
        {
            PatientsController controller;
            int patientId;
            Patient patient;

            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = mr.CreateController<PatientsController>();
                Firestarter.CreateFakePatients(doctor, this.db, 1);

                // we now have 1 patient
                patient = this.db.Patients.FirstOrDefault();
                Assert.IsNotNull(patient);

                patientId = patient.Id;

                // now, let's add an anamnese
                var anamnese = new Anamnese()
                {
                    PatientId = patientId,
                    CreatedOn = DateTime.UtcNow,
                    Text = "This is my anamnese",
                    PracticeId = doctor.PracticeId,
                };

                anamnese.Symptoms.Add(new Symptom()
                {
                    Cid10Name = "Text",
                    Cid10Code = "Q878",
                    PracticeId = doctor.PracticeId,
                });

                patient.Anamneses.Add(anamnese);
                this.db.SaveChanges();
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            controller.Delete(patientId);

            // this patient must have been deleted
            patient = this.db.Patients.FirstOrDefault(p => p.Id == patientId);
            Assert.IsNull(patient);
        }

        [TestMethod]
        public void Delete_WhenTheresAReceipt()
        {
            PatientsController controller;
            int patientId;
            Patient patient;

            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = mr.CreateController<PatientsController>();
                Firestarter.CreateFakePatients(doctor, this.db, 1);

                // we now have 1 patient
                patient = this.db.Patients.FirstOrDefault();
                Assert.IsNotNull(patient);

                patientId = patient.Id;

                var medicine = new Medicine()
                    {
                        Laboratory = new Laboratory()
                        {
                            Name = "Lab1",
                            Doctor = doctor
                        },
                        Name = "Med1",
                        Doctor = doctor,
                        PracticeId = doctor.PracticeId,
                    };

                medicine.ActiveIngredients.Add(new MedicineActiveIngredient()
                    {
                        Name = "AI1",
                        PracticeId = doctor.PracticeId,
                    });

                this.db.Medicines.AddObject(medicine);

                // now, let's add an receipt
                var receipt = new Receipt()
                {
                    PatientId = patientId,
                    CreatedOn = DateTime.UtcNow,
                    PracticeId = doctor.PracticeId,
                };

                receipt.ReceiptMedicines.Add(new ReceiptMedicine()
                    {
                        Medicine = medicine,
                        Quantity = "1 caixa",
                        Prescription = "toma 1 de manha",
                        PracticeId = doctor.PracticeId,
                    });

                this.db.Receipts.AddObject(receipt);

                this.db.SaveChanges();
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            controller.Delete(patientId);

            // this patient must have been deleted
            patient = this.db.Patients.FirstOrDefault(p => p.Id == patientId);
            Assert.IsNull(patient);
        }

        [TestMethod]
        public void Delete_WhenTheresAnExamRequest()
        {
            PatientsController controller;
            int patientId;
            Patient patient;

            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = mr.CreateController<PatientsController>();
                Firestarter.CreateFakePatients(doctor, this.db, 1);

                // we now have 1 patient
                patient = this.db.Patients.FirstOrDefault();
                Assert.IsNotNull(patient);

                patientId = patient.Id;

                var examRequest = new ExaminationRequest()
                    {
                        MedicalProcedureCode = "mcode",
                        MedicalProcedureName = "mname",
                        PatientId = patientId,
                        CreatedOn = DateTime.UtcNow,
                        PracticeId = doctor.PracticeId,
                    };

                this.db.SYS_MedicalProcedure.AddObject(
                    new SYS_MedicalProcedure()
                        {
                            Code = "mcode",
                            Name = "mname"
                        });

                this.db.ExaminationRequests.AddObject(examRequest);

                this.db.SaveChanges();
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            controller.Delete(patientId);

            // this patient must have been deleted
            patient = this.db.Patients.FirstOrDefault(p => p.Id == patientId);
            Assert.IsNull(patient);
        }

        [TestMethod]
        public void Delete_WhenTheresAnExamResult()
        {
            PatientsController controller;
            int patientId;
            Patient patient;

            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = mr.CreateController<PatientsController>();
                Firestarter.CreateFakePatients(doctor, this.db, 1);

                // we now have 1 patient
                patient = this.db.Patients.FirstOrDefault();
                Assert.IsNotNull(patient);

                patientId = patient.Id;

                var examResult = new ExaminationResult()
                {
                    MedicalProcedureCode = "mcode",
                    MedicalProcedureName = "mname",
                    PatientId = patientId,
                    CreatedOn = DateTime.UtcNow,
                    Text = "tudo deu certo",
                    PracticeId = doctor.PracticeId,
                };

                this.db.SYS_MedicalProcedure.AddObject(
                    new SYS_MedicalProcedure()
                        {
                            Code = "mcode",
                            Name = "mname"
                        });

                this.db.ExaminationResults.AddObject(examResult);

                this.db.SaveChanges();
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

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
                controller = mr.CreateController<PatientsController>();
                Firestarter.CreateFakePatients(doctor, this.db, 1);

                // we now have 1 patient
                patient = this.db.Patients.FirstOrDefault();
                Assert.IsNotNull(patient);

                var certificateModel = new Cerebello.Model.ModelMedicalCertificate()
                {
                    DoctorId = doctor.Id,
                    Name = "model1",
                    Text = "model1",
                    PracticeId = doctor.PracticeId,
                };

                certificateModel.Fields.Add(new ModelMedicalCertificateField()
                {
                    Name = "field1",
                    PracticeId = doctor.PracticeId,
                });

                var certificate = new Cerebello.Model.MedicalCertificate()
                {
                    ModelMedicalCertificate = certificateModel,
                    Patient = patient,
                    Text = "text",
                    CreatedOn = DateTime.UtcNow,
                    PracticeId = doctor.PracticeId,
                };

                certificate.Fields.Add(new MedicalCertificateField()
                {
                    Name = "field1",
                    Value = "value",
                    PracticeId = doctor.PracticeId,
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
            Patient patient;

            try
            {
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = mr.CreateController<PatientsController>();
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
                    End = referenceTime + TimeSpan.FromMinutes(30),
                    PracticeId = docAndre.PracticeId,
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

        [TestMethod]
        public void Delete_WhenTheresADiagnosis()
        {
            PatientsController controller;
            Patient patient;

            try
            {
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = mr.CreateController<PatientsController>();
                Firestarter.CreateFakePatients(docAndre, this.db, 1);

                // we now have 1 patient
                patient = this.db.Patients.FirstOrDefault();
                Assert.IsNotNull(patient);
                var referenceTime = DateTime.UtcNow;

                var diagnosis = new Diagnosis()
                {
                    CreatedOn = referenceTime,
                    PatientId = patient.Id,
                    Cid10Code = "QAA",
                    Cid10Name = "Doença X", // x-men!
                    PracticeId = docAndre.PracticeId,
                };

                this.db.Diagnoses.AddObject(diagnosis);
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
    }
}
