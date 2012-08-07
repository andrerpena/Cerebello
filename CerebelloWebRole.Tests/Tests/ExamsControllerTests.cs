using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Firestarter;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cerebello;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Mvc;
using CerebelloWebRole.Code.Json;

namespace CerebelloWebRole.Tests
{
    [TestClass]
    public class ExamsControllerTests
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

            this.MedicalProcedures = Firestarter.CreateFakeMedicalProcedures(this.db);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            this.db.Dispose();
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
                var mr = new MockRepository();
                controller = Mvc3TestHelper.CreateControllerForTesting<ExamsController>(this.db, mr);
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // Creating a new examination request.
            ActionResult actionResult;

            {
                var viewModel = new ExaminationRequestViewModel
                {
                    PatientId = patient.Id,
                    Notes = "Any text",
                    MedicalProcedureId = this.MedicalProcedures[0].Id,
                    MedicalProcedureText = this.MedicalProcedures[0].Name,
                };

                actionResult = controller.Create(viewModel);
            }

            // Verifying the ActionResult.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");

            // Verifying the controller model-state.
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            // Verifying the database.
            Assert.IsTrue(this.db.ExaminationRequests.Any(x => x.PatientId == patient.Id), "Database record was not saved.");
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
                var mr = new MockRepository();
                controller = Mvc3TestHelper.CreateControllerForTesting<ExamsController>(this.db, mr);
                this.db.SavingChanges += new EventHandler((s, e) => { isDbChangesSaved = true; });
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
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

                actionResult = controller.Create(viewModel);
            }

            // Verifying the ActionResult, and the DB.
            // - The result must be a ViewResult, with the name "Edit".
            // - The controller ModelState must have one validation message.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual("edit", viewResult.ViewName);
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState should not be valid.");
            Assert.AreEqual(
                1,
                controller.ModelState.GetPropertyErrors(() => viewModel.MedicalProcedureText).Count(),
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
            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                patient = Firestarter.CreateFakePatients(doctor, this.db).First();
                var mr = new MockRepository();
                controller = Mvc3TestHelper.CreateControllerForTesting<ExamsController>(this.db, mr);

                // saving the object that will be edited
                examRequest = new ExaminationRequest
                {
                    CreatedOn = DateTime.Now,
                    PatientId = patient.Id,
                    Text = "Old text",
                    MedicalProcedureId = this.MedicalProcedures[0].Id,
                };
                this.db.ExaminationRequests.AddObject(examRequest);
                this.db.SaveChanges();
            }
            catch (Exception ex)
            {
                Assert.Inconclusive(string.Format("Test initialization has failed. {0}", ex.Message));
                return;
            }

            // Creating a new examination request.
            ActionResult actionResult;

            {
                var viewModel = new ExaminationRequestViewModel
                {
                    Id = examRequest.Id,
                    PatientId = patient.Id,
                    Notes = "Any text",
                    MedicalProcedureId = this.MedicalProcedures[1].Id, // editing value: old = 0; new = 1
                    MedicalProcedureText = this.MedicalProcedures[1].Name,
                };

                Mvc3TestHelper.SetModelStateErrors(controller, viewModel);

                actionResult = controller.Edit(viewModel);
            }

            // Verifying the ActionResult.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");

            // Verifying the controller model-state.
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            // Verifying the database.
            using (var db2 = new CerebelloEntities(this.db.Connection.ConnectionString))
            {
                var obj = db2.ExaminationRequests.Where(x => x.PatientId == patient.Id).FirstOrDefault();
                Assert.IsNotNull(obj, "Database record was not saved.");
                Assert.AreEqual("Any text", obj.Text);
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
            bool isDbChangesSaved = false;
            try
            {
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                patient = Firestarter.CreateFakePatients(doctor, this.db).First();
                var mr = new MockRepository();
                controller = Mvc3TestHelper.CreateControllerForTesting<ExamsController>(this.db, mr);

                // saving the object that will be edited
                examRequest = new ExaminationRequest
                {
                    CreatedOn = DateTime.Now,
                    PatientId = patient.Id,
                    Text = "Old text",
                    MedicalProcedureId = this.MedicalProcedures[0].Id,
                };
                this.db.ExaminationRequests.AddObject(examRequest);
                this.db.SaveChanges();

                // this must come last... after database preparation.
                this.db.SavingChanges += new EventHandler((s, e) => { isDbChangesSaved = true; });
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
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

                actionResult = controller.Edit(viewModel);
            }

            // Verifying the ActionResult, and the DB.
            // - The result must be a ViewResult, with the name "Edit".
            // - The controller ModelState must have one validation message.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual("edit", viewResult.ViewName);
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState should not be valid.");
            Assert.AreEqual(
                1,
                controller.ModelState.GetPropertyErrors(() => viewModel.MedicalProcedureText).Count(),
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
            Patient patientDraMarta;
            ExaminationRequest examRequest;
            ExaminationRequestViewModel viewModel;
            bool isDbChangesSaved = false;
            try
            {
                var drandre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var dramarta = Firestarter.Create_CrmMg_Psiquiatria_DraMarta_Marta(this.db);
                patientDraMarta = Firestarter.CreateFakePatients(dramarta, this.db).First();

                var mr = new MockRepository();
                controller = Mvc3TestHelper.CreateControllerForTesting<ExamsController>(this.db, mr);

                // saving the object that will be edited
                examRequest = new ExaminationRequest
                {
                    CreatedOn = DateTime.Now,
                    PatientId = patientDraMarta.Id,
                    Text = "Old text",
                    MedicalProcedureId = this.MedicalProcedures[0].Id,
                };
                this.db.ExaminationRequests.AddObject(examRequest);
                this.db.SaveChanges();

                // This must come last... after database preparation.
                this.db.SavingChanges += new EventHandler((s, e) => { isDbChangesSaved = true; });

                // Define André as the logged user, he cannot edit Marta's patients.
                mr.SetCurrentUser_Andre_CorrectPassword();

                // Creating view-model and setting up controller ModelState based on the view-model.
                viewModel = new ExaminationRequestViewModel
                {
                    Id = examRequest.Id,
                    PatientId = patientDraMarta.Id,
                    Notes = "New text",
                    MedicalProcedureId = this.MedicalProcedures[2].Id,
                    MedicalProcedureText = this.MedicalProcedures[2].Name,
                };

                Mvc3TestHelper.SetModelStateErrors(controller, viewModel);
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // Editing an examination request that does not belong to the current user's practice.
            // This is not allowed and must throw an exception.
            // note: this is not a validation error, this is a malicious attack...
            ActionResult actionResult = controller.Edit(viewModel);

            // Verifying the ActionResult, and the DB.
            // - The result must be a ViewResult, with the name "Edit".
            // - The controller ModelState must have one validation message.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual("NotFound", viewResult.ViewName);
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState should be valid.");

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
            Patient patient;
            ExaminationRequest examRequest;
            ExaminationRequestViewModel viewModel;
            bool isDbChangesSaved = false;
            try
            {
                var drandre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                patient = Firestarter.CreateFakePatients(drandre, this.db).First();

                var mr = new MockRepository();
                controller = Mvc3TestHelper.CreateControllerForTesting<ExamsController>(this.db, mr);

                // saving the object that will be edited
                examRequest = new ExaminationRequest
                {
                    CreatedOn = DateTime.Now,
                    PatientId = patient.Id,
                    Text = "Old text",
                    MedicalProcedureId = this.MedicalProcedures[0].Id,
                };
                this.db.ExaminationRequests.AddObject(examRequest);
                this.db.SaveChanges();

                // This must come last... after database preparation.
                this.db.SavingChanges += new EventHandler((s, e) => { isDbChangesSaved = true; });

                // Define André as the logged user.
                mr.SetCurrentUser_Andre_CorrectPassword();

                // Creating view-model and setting up controller ModelState based on the view-model.
                viewModel = new ExaminationRequestViewModel
                {
                    Id = 19837,
                    PatientId = patient.Id,
                    Notes = "New text",
                    MedicalProcedureId = this.MedicalProcedures[1].Id,
                    MedicalProcedureText = this.MedicalProcedures[1].Name,
                };

                Mvc3TestHelper.SetModelStateErrors(controller, viewModel);
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // Editing an examination request that does not belong to the current user's practice.
            // This is not allowed and must throw an exception.
            // note: this is not a validation error, this is a malicious attack...
            ActionResult actionResult = controller.Edit(viewModel);

            // Verifying the ActionResult, and the DB.
            // - The result must be a ViewResult, with the name "Edit".
            // - The controller ModelState must have one validation message.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual("NotFound", viewResult.ViewName);
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState should be valid.");

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
            bool isDbChangesSaved = false;
            try
            {
                using (var db2 = new CerebelloEntities(this.db.Connection.ConnectionString))
                {
                    var drandre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(db2);
                    patient = Firestarter.CreateFakePatients(drandre, db2).First();

                    var mr = new MockRepository();
                    controller = Mvc3TestHelper.CreateControllerForTesting<ExamsController>(this.db, mr);

                    // saving the object that will be edited
                    examRequest = db2.ExaminationRequests.CreateObject();

                    examRequest.CreatedOn = DateTime.Now;
                    examRequest.PatientId = patient.Id;
                    examRequest.Text = "Old text";
                    examRequest.MedicalProcedureId = this.MedicalProcedures[2].Id;

                    db2.ExaminationRequests.AddObject(examRequest);
                    db2.SaveChanges();

                    // This must come last... after database preparation.
                    this.db.SavingChanges += new EventHandler((s, e) => { isDbChangesSaved = true; });

                    // Define André as the logged user, he cannot edit Marta's patients.
                    mr.SetCurrentUser_Andre_CorrectPassword();
                }
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
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
                var obj = db2.ExaminationRequests.Where(x => x.PatientId == patient.Id).FirstOrDefault();
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

                    var mr = new MockRepository();
                    controller = Mvc3TestHelper.CreateControllerForTesting<ExamsController>(this.db, mr);

                    // This must come after database preparation.
                    this.db.SavingChanges += new EventHandler((s, e) => { isDbChangesSaved = true; });

                    // Define André as the logged user, he cannot edit Marta's patients.
                    mr.SetCurrentUser_Andre_CorrectPassword();
                }
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
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
            Patient patientDraMarta;
            ExaminationRequest examRequest;
            bool isDbChangesSaved = false;
            try
            {
                var drandre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var dramarta = Firestarter.Create_CrmMg_Psiquiatria_DraMarta_Marta(this.db);
                patientDraMarta = Firestarter.CreateFakePatients(dramarta, this.db).First();

                var mr = new MockRepository();
                controller = Mvc3TestHelper.CreateControllerForTesting<ExamsController>(this.db, mr);

                // saving the object that will be edited
                examRequest = new ExaminationRequest
                {
                    CreatedOn = DateTime.Now,
                    PatientId = patientDraMarta.Id,
                    Text = "Old text",
                    MedicalProcedureId = this.MedicalProcedures[0].Id,
                };
                this.db.ExaminationRequests.AddObject(examRequest);
                this.db.SaveChanges();

                // This must come last... after database preparation.
                this.db.SavingChanges += new EventHandler((s, e) => { isDbChangesSaved = true; });

                // Define André as the logged user, he cannot edit Marta's patients.
                mr.SetCurrentUser_Andre_CorrectPassword();
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
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

        public SYS_MedicalProcedure[] MedicalProcedures { get; set; }
    }
}
