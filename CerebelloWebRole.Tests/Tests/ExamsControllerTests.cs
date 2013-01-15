using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Firestarter;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Json;
using CerebelloWebRole.Code.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests
{
    [TestClass]
    public class ExamsControllerTests : DbTestBase
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

        #region Create
        /// <summary>
        /// Tests the creation of an examination request.
        /// This is a valid operation and should complete without exceptions,
        /// and without validation errors.
        /// </summary>
        [TestMethod]
        public void Create_1_HappyPath()
        {
            ExamsController controller;
            Patient patient;
            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                patient = Firestarter.CreateFakePatients(doctor, this.db).First();
                var mr = new MockRepository(true);
                controller = mr.CreateController<ExamsController>();
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Creating a new examination request.
            ActionResult actionResult;

            {
                var medicalProc = this.db.SYS_MedicalProcedure.Single(x => x.Code == "4.03.04.36-1");
                var viewModel = new ExaminationRequestViewModel
                {
                    PatientId = patient.Id,
                    Notes = "Any text",
                    MedicalProcedureId = medicalProc.Id,
                    MedicalProcedureName = "Hemograma com contagem de plaquetas ou frações (eritrograma, leucograma, plaquetas)",
                };

                actionResult = controller.Create(new[] { viewModel });
            }

            // Verifying the ActionResult.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");

            // Verifying the controller model-state.
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            // Verifying the database.
            Assert.IsTrue(this.db.ExaminationRequests.Any(x => x.PatientId == patient.Id), "Database record was not saved.");

            // Verifying the database.
            using (var db2 = new CerebelloEntities(this.db.Connection.ConnectionString))
            {
                var obj = db2.ExaminationRequests.FirstOrDefault(x => x.PatientId == patient.Id);
                Assert.IsNotNull(obj, "Database record was not saved.");
                Assert.AreEqual("Any text", obj.Text);
                Assert.AreEqual("4.03.04.36-1", obj.MedicalProcedureCode);
                Assert.AreEqual("Hemograma com contagem de plaquetas ou frações (eritrograma, leucograma, plaquetas)", obj.MedicalProcedureName);
            }
        }

        /// <summary>
        /// Tests the creation of an examination request without a text.
        /// This is an invalid operation, and should stay in the same View, with a ModelState validation message.
        /// </summary>
        [TestMethod]
        public void Create_WithoutMedicalProcedure()
        {
            ExamsController controller;
            Patient patient;
            bool isDbChangesSaved = false;
            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                patient = Firestarter.CreateFakePatients(doctor, this.db).First();
                var mr = new MockRepository(true);
                controller = mr.CreateController<ExamsController>(
                        setupNewDb: db => db.SavingChanges += (s, e) => { isDbChangesSaved = true; });
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Creating a new examination request without the text.
            // This is not allowed and must generate a model state validation message.
            ActionResult actionResult;
            ExaminationRequestViewModel viewModel;

            {
                viewModel = new ExaminationRequestViewModel
                {
                    PatientId = patient.Id,
                };

                Mvc3TestHelper.SetModelStateErrors(controller, viewModel);

                actionResult = controller.Create(new[] { viewModel });
            }

            // Verifying the ActionResult, and the DB.
            // - The result must be a ViewResult, with the name "Edit".
            // - The controller ModelState must have one validation message.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual("edit", viewResult.ViewName, ignoreCase: true);
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState should not be valid.");
            Assert.AreEqual(
                1,
                controller.ModelState.GetPropertyErrors(() => viewModel.MedicalProcedureName).Count(),
                "ModelState should contain one validation message.");

            // Verifying the database: cannot save the changes.
            Assert.IsFalse(isDbChangesSaved, "Database changes were saved, but they should not.");
        }
        #endregion

        #region Edit
        /// <summary>
        /// Tests the edition of an examination request.
        /// This is a valid operation.
        /// </summary>
        [TestMethod]
        public void Edit_1_HappyPath()
        {
            ExamsController controller;
            Patient patient;
            ExaminationRequest examRequest;
            DateTime utcNow;
            var localNow = new DateTime(2012, 08, 16);
            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                patient = Firestarter.CreateFakePatients(doctor, this.db).First();

                var mr = new MockRepository(true);
                controller = mr.CreateController<ExamsController>();
                Debug.Assert(doctor != null, "doctor must not be null");
                utcNow = PracticeController.ConvertToUtcDateTime(doctor.Users.First().Practice, localNow);
                DateTimeHelper.SetUtcNow(utcNow);

                // saving the object that will be edited
                var medicalProc = this.db.SYS_MedicalProcedure.Single(x => x.Code == "4.03.04.36-1");
                examRequest = new ExaminationRequest
                {
                    CreatedOn = utcNow,
                    PatientId = patient.Id,
                    Text = "Old text",
                    MedicalProcedureCode = medicalProc.Code,
                    MedicalProcedureName = medicalProc.Name,
                    PracticeId = doctor.PracticeId,
                };
                this.db.ExaminationRequests.AddObject(examRequest);
                this.db.SaveChanges();
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Creating a new examination request.
            ActionResult actionResult;

            {
                var medicalProc = this.db.SYS_MedicalProcedure.Single(x => x.Code == "4.01.03.23-4");
                var viewModel = new ExaminationRequestViewModel
                {
                    Id = examRequest.Id,
                    PatientId = patient.Id,
                    Notes = "Any text",
                    MedicalProcedureId = medicalProc.Id, // editing value: old = "4.03.04.36-1"; new = "4.01.03.23-4"
                    MedicalProcedureName = "Eletrencefalograma em vigília, e sono espontâneo ou induzido",
                };

                Mvc3TestHelper.SetModelStateErrors(controller, viewModel);

                actionResult = controller.Edit(new[] { viewModel });
            }

            // Verifying the ActionResult.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");

            // Verifying the controller model-state.
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            // Verifying the database.
            using (var db2 = new CerebelloEntities(this.db.Connection.ConnectionString))
            {
                var obj = db2.ExaminationRequests.FirstOrDefault(x => x.PatientId == patient.Id);
                Assert.IsNotNull(obj, "Database record was not saved.");
                Assert.AreEqual("Any text", obj.Text);
                Assert.AreEqual(utcNow, obj.CreatedOn);
                Assert.AreEqual("4.01.03.23-4", obj.MedicalProcedureCode);
                Assert.AreEqual("Eletrencefalograma em vigília, e sono espontâneo ou induzido", obj.MedicalProcedureName);
            }
        }

        /// <summary>
        /// Tests the edition of an examination request, by removing the text.
        /// This is an invalid operation, and should stay in the same View, with a ModelState validation message.
        /// </summary>
        [TestMethod]
        public void Edit_2_WithoutMedicalProcedure()
        {
            ExamsController controller;
            Patient patient;
            ExaminationRequest examRequest;
            var isDbChangesSaved = false;
            var localNow = new DateTime(2012, 08, 16);
            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                patient = Firestarter.CreateFakePatients(doctor, this.db).First();
                var mr = new MockRepository(true);
                controller = mr.CreateController<ExamsController>(
                        setupNewDb: db => db.SavingChanges += (s, e) => { isDbChangesSaved = true; });
                Debug.Assert(doctor != null, "doctor must not be null");
                var utcNow = PracticeController.ConvertToUtcDateTime(doctor.Users.First().Practice, localNow);

                DateTimeHelper.SetUtcNow(utcNow);

                // saving the object that will be edited
                examRequest = new ExaminationRequest
                {
                    CreatedOn = utcNow,
                    PatientId = patient.Id,
                    Text = "Old text",
                    PracticeId = doctor.PracticeId,
                    MedicalProcedureName = "Hemoglobina (eletroforese ou HPLC)",
                    MedicalProcedureCode = "4.03.04.35-3",
                };
                this.db.ExaminationRequests.AddObject(examRequest);
                this.db.SaveChanges();
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Creating a new examination request without the text.
            // This is not allowed and must generate a model state validation message.
            ActionResult actionResult;
            ExaminationRequestViewModel viewModel;

            {
                viewModel = new ExaminationRequestViewModel
                {
                    Id = examRequest.Id,
                    PatientId = patient.Id,
                };

                Mvc3TestHelper.SetModelStateErrors(controller, viewModel);

                actionResult = controller.Edit(new[] { viewModel });
            }

            // Verifying the ActionResult, and the DB.
            // - The result must be a ViewResult, with the name "Edit".
            // - The controller ModelState must have one validation message.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual("edit", viewResult.ViewName, true);
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState should not be valid.");
            Assert.AreEqual(
                1,
                controller.ModelState.GetPropertyErrors(() => viewModel.MedicalProcedureName).Count(),
                "ModelState should contain one validation message.");

            // Verifying the database: cannot save the changes.
            Assert.IsFalse(isDbChangesSaved, "Database changes were saved, but they should not.");
        }

        /// <summary>
        /// Tests the edition of an examination request, that belongs to another practice.
        /// This is an error and an exception should be thrown.
        /// </summary>
        [TestMethod]
        public void Edit_3_EditExamFromAnotherPractice()
        {
            ExamsController controller;
            ExaminationRequestViewModel viewModel;
            var isDbChangesSaved = false;
            var localNow = new DateTime(2012, 08, 16);
            try
            {
                var drandre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var dramarta = Firestarter.Create_CrmMg_Psiquiatria_DraMarta_Marta(this.db);
                var patientDraMarta = Firestarter.CreateFakePatients(dramarta, this.db).First();

                var mr = new MockRepository(true);
                controller = mr.CreateController<ExamsController>(
                        setupNewDb: db => db.SavingChanges += (s, e) => { isDbChangesSaved = true; });
                Debug.Assert(drandre != null, "drandre must not be null");
                var utcNow = PracticeController.ConvertToUtcDateTime(drandre.Users.First().Practice, localNow);

                DateTimeHelper.SetUtcNow(utcNow);

                // saving the object that will be edited
                var medicalProc0 = this.db.SYS_MedicalProcedure.Single(x => x.Code == "4.03.04.36-1");
                var examRequest = new ExaminationRequest
                                      {
                                          CreatedOn = utcNow,
                                          PatientId = patientDraMarta.Id,
                                          Text = "Old text",
                                          MedicalProcedureCode = medicalProc0.Code,
                                          MedicalProcedureName = medicalProc0.Name,
                                          PracticeId = dramarta.PracticeId,
                                      };
                this.db.ExaminationRequests.AddObject(examRequest);
                this.db.SaveChanges();

                // Define André as the logged user, he cannot edit Marta's patients.
                mr.SetCurrentUser_Andre_CorrectPassword();

                // Creating view-model and setting up controller ModelState based on the view-model.
                var medicalProc1 = this.db.SYS_MedicalProcedure.Single(x => x.Code == "4.01.03.55-2");
                viewModel = new ExaminationRequestViewModel
                {
                    Id = examRequest.Id,
                    PatientId = patientDraMarta.Id,
                    Notes = "New text",
                    MedicalProcedureCode = medicalProc1.Code,
                    MedicalProcedureName = medicalProc1.Name,
                };

                Mvc3TestHelper.SetModelStateErrors(controller, viewModel);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Editing an examination request that does not belong to the current user's practice.
            // This is not allowed and must throw an exception.
            // note: this is not a validation error, this is a malicious attack...
            ActionResult actionResult = controller.Edit(new[] { viewModel });

            // Verifying the ActionResult, and the DB.
            // - The result must be a ViewResult, with the name "Edit".
            // - The controller ModelState must have one validation message.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual("NotFound", viewResult.ViewName);

            // Verifying the database: cannot save the changes.
            Assert.IsFalse(isDbChangesSaved, "Database changes were saved, but they should not.");
        }

        /// <summary>
        /// Tests the edition of an examination request that doen not exist.
        /// This is an error and an exception should be thrown.
        /// </summary>
        [TestMethod]
        public void Edit_4_EditExamThatDoesNotExist()
        {
            ExamsController controller;
            ExaminationRequestViewModel viewModel;
            var isDbChangesSaved = false;
            var localNow = new DateTime(2012, 08, 16);
            try
            {
                var drandre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var patient = Firestarter.CreateFakePatients(drandre, this.db).First();

                var mr = new MockRepository(true);
                controller = mr.CreateController<ExamsController>(
                        setupNewDb: db => db.SavingChanges += (s, e) => { isDbChangesSaved = true; });
                Debug.Assert(drandre != null, "drandre must not be null");
                var utcNow = PracticeController.ConvertToUtcDateTime(drandre.Users.First().Practice, localNow);

                DateTimeHelper.SetUtcNow(utcNow);

                // saving the object that will be edited
                var medicalProc0 = this.db.SYS_MedicalProcedure.Single(x => x.Code == "4.03.04.36-1");
                var examRequest = new ExaminationRequest
                                      {
                                          CreatedOn = utcNow,
                                          PatientId = patient.Id,
                                          Text = "Old text",
                                          MedicalProcedureCode = medicalProc0.Code,
                                          MedicalProcedureName = medicalProc0.Name,
                                          PracticeId = drandre.PracticeId,
                                      };
                this.db.ExaminationRequests.AddObject(examRequest);
                this.db.SaveChanges();

                // Define André as the logged user.
                mr.SetCurrentUser_Andre_CorrectPassword();

                // Creating view-model and setting up controller ModelState based on the view-model.
                var medicalProc1 = this.db.SYS_MedicalProcedure.Single(x => x.Code == "4.01.03.23-4");
                viewModel = new ExaminationRequestViewModel
                {
                    Id = 19837,
                    PatientId = patient.Id,
                    Notes = "New text",
                    MedicalProcedureCode = medicalProc1.Code,
                    MedicalProcedureName = medicalProc1.Name,
                };

                Mvc3TestHelper.SetModelStateErrors(controller, viewModel);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Editing an examination request that does not belong to the current user's practice.
            // This is not allowed and must throw an exception.
            // note: this is not a validation error, this is a malicious attack...
            ActionResult actionResult = controller.Edit(new[] { viewModel });

            // Verifying the ActionResult, and the DB.
            // - The result must be a ViewResult, with the name "Edit".
            // - The controller ModelState must have one validation message.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual("NotFound", viewResult.ViewName);

            // Verifying the database: cannot save the changes.
            Assert.IsFalse(isDbChangesSaved, "Database changes were saved, but they should not.");
        }
        #endregion

        #region Delete
        /// <summary>
        /// Tests the deletion of an examination request.
        /// This is a valid operation.
        /// </summary>
        [TestMethod]
        public void Delete_1_HappyPath()
        {
            ExamsController controller;
            Patient patient;
            ExaminationRequest examRequest;
            var isDbChangesSaved = false;
            var localNow = new DateTime(2012, 08, 16);
            try
            {
                using (var db2 = new CerebelloEntities(this.db.Connection.ConnectionString))
                {
                    var drandre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(db2);
                    patient = Firestarter.CreateFakePatients(drandre, db2).First();

                    var mr = new MockRepository(true);
                    controller = mr.CreateController<ExamsController>(
                        setupNewDb: db => db.SavingChanges += (s, e) => { isDbChangesSaved = true; });
                    Debug.Assert(drandre != null, "drandre must not be null");
                    var utcNow = PracticeController.ConvertToUtcDateTime(drandre.Users.First().Practice, localNow);

                    DateTimeHelper.SetUtcNow(utcNow);

                    // saving the object that will be edited
                    var medicalProc1 = this.db.SYS_MedicalProcedure.Single(x => x.Code == "4.01.03.55-2");

                    examRequest = new ExaminationRequest
                                      {
                                          PracticeId = patient.PracticeId,
                                          CreatedOn = utcNow,
                                          PatientId = patient.Id,
                                          Text = "Old text",
                                          MedicalProcedureCode = medicalProc1.Code,
                                          MedicalProcedureName = medicalProc1.Name
                                      };

                    db2.ExaminationRequests.AddObject(examRequest);
                    db2.SaveChanges();

                    // Define André as the logged user, he cannot edit Marta's patients.
                    mr.SetCurrentUser_Andre_CorrectPassword();
                }
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Editing an examination request that does not belong to the current user's practice.
            // This is not allowed and must throw an exception.
            // note: this is not a validation error, this is a malicious attack...
            ActionResult actionResult = controller.Delete(examRequest.Id);

            // Verifying the ActionResult.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");

            // Verifying the controller model-state.
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            // Verifying the database: cannot save the changes.
            Assert.IsTrue(isDbChangesSaved, "Database changes were not saved, but they should.");

            // Verifying the database.
            using (var db2 = new CerebelloEntities(this.db.Connection.ConnectionString))
            {
                var obj = db2.ExaminationRequests.FirstOrDefault(x => x.PatientId == patient.Id);
                Assert.IsNull(obj, "Database record was not deleted.");
            }
        }

        /// <summary>
        /// Tests the deletion of an examination request that does not exist.
        /// This is an invalid operation and should return a fail message.
        /// </summary>
        [TestMethod]
        public void Delete_2_ExamThatDoesNotExist()
        {
            ExamsController controller;
            bool isDbChangesSaved = false;
            try
            {
                using (var db2 = new CerebelloEntities(this.db.Connection.ConnectionString))
                {
                    Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(db2);

                    var mr = new MockRepository(true);
                    controller = mr.CreateController<ExamsController>(
                        setupNewDb: db => db.SavingChanges += (s, e) => { isDbChangesSaved = true; });

                    // Define André as the logged user, he cannot edit Marta's patients.
                    mr.SetCurrentUser_Andre_CorrectPassword();
                }
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Editing an examination request that does not belong to the current user's practice.
            // This is not allowed and must throw an exception.
            // note: this is not a validation error, this is a malicious attack...
            var jsonResult = controller.Delete(6327);

            // Verifying the ActionResult.
            Assert.IsNotNull(jsonResult, "The result of the controller method is null.");
            var jsonDelete = (JsonDeleteMessage)jsonResult.Data;
            Assert.IsFalse(jsonDelete.success, "Deletion should not succed.");
            Assert.IsNotNull(jsonDelete.text, "Deletion should fail with a message.");

            // Verifying the controller model-state.
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            // Verifying the database: cannot save the changes.
            Assert.IsFalse(isDbChangesSaved, "Database changes were saved, but they should not.");
        }

        /// <summary>
        /// Tests the deletion of an examination request that does not exist.
        /// This is an invalid operation and should return a fail message.
        /// </summary>
        [TestMethod]
        public void Delete_3_ExamFromAnotherPractice()
        {
            ExamsController controller;
            ExaminationRequest examRequest;
            var isDbChangesSaved = false;
            var localNow = new DateTime(2012, 08, 16);
            try
            {
                var drandre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var dramarta = Firestarter.Create_CrmMg_Psiquiatria_DraMarta_Marta(this.db);
                var patientDraMarta = Firestarter.CreateFakePatients(dramarta, this.db).First();

                var mr = new MockRepository(true);
                controller = mr.CreateController<ExamsController>(
                        setupNewDb: db => db.SavingChanges += (s, e) => { isDbChangesSaved = true; });
                Debug.Assert(drandre != null, "drandre must not be null");
                var utcNow = PracticeController.ConvertToUtcDateTime(drandre.Users.First().Practice, localNow);

                DateTimeHelper.SetUtcNow(utcNow);

                // saving the object that will be edited
                var medicalProc0 = this.db.SYS_MedicalProcedure.Single(x => x.Code == "4.03.04.36-1");
                examRequest = new ExaminationRequest
                {
                    CreatedOn = utcNow,
                    PatientId = patientDraMarta.Id,
                    Text = "Old text",
                    MedicalProcedureCode = medicalProc0.Code,
                    MedicalProcedureName = medicalProc0.Name,
                    PracticeId = dramarta.PracticeId,
                };
                this.db.ExaminationRequests.AddObject(examRequest);
                this.db.SaveChanges();

                // Define André as the logged user, he cannot edit Marta's patients.
                mr.SetCurrentUser_Andre_CorrectPassword();
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Editing an examination request that does not belong to the current user's practice.
            // This is not allowed and must throw an exception.
            // note: this is not a validation error, this is a malicious attack...
            var jsonResult = controller.Delete(examRequest.Id);

            // Verifying the ActionResult.
            Assert.IsNotNull(jsonResult, "The result of the controller method is null.");
            var jsonDelete = (JsonDeleteMessage)jsonResult.Data;
            Assert.IsFalse(jsonDelete.success, "Deletion should not succed.");
            Assert.IsNotNull(jsonDelete.text, "Deletion should fail with a message.");

            // Verifying the controller model-state.
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            // Verifying the database: cannot save the changes.
            Assert.IsFalse(isDbChangesSaved, "Database changes were saved, but they should not.");
        }
        #endregion
    }
}
