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

namespace CerebelloWebRole.Tests
{
    [TestClass]
    public class ScheduleControllerTests
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
            Firestarter.InitializeDatabaseWithSystemData(this.db);
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            this.db.Dispose();
        }
        #endregion

        #region Create Visualization
        [TestMethod]
        public void CreateView_1_ViewNew_HappyPath()
        {
            ScheduleController controller;
            bool isDbChanged = false;
            try
            {
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);
                var mr = new MockRepository();
                mr.SetRouteData_App_ConsultorioDrHourse_GregoryHouse();
                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr);
                this.db.SavingChanges += new EventHandler((s, e) => { isDbChanged = true; });
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // View new appointment.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Create((DateTime?)null, "10:00", "10:30", (int?)null);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.AreEqual(controller.ViewBag.IsEditing, false);
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
            Assert.IsFalse(isDbChanged, "View actions cannot change DB.");
        }

        [TestMethod]
        public void CreateView_ViewNewWithWeirdTimes_HappyPath()
        {
            ScheduleController controller;
            bool isDbChanged = false;
            try
            {
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);
                var mr = new MockRepository();
                mr.SetRouteData_App_ConsultorioDrHourse_GregoryHouse();
                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr);
                this.db.SavingChanges += new EventHandler((s, e) => { isDbChanged = true; });
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // View new appointment.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Create((DateTime?)null, "10:07", "12:42", (int?)null);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");

            // Verify view-model.
            var viewResult = (ViewResult)actionResult;
            var viewModel = (AppointmentViewModel)viewResult.Model;
            Assert.AreEqual("10:07", viewModel.Start);
            Assert.AreEqual("12:42", viewModel.End);

            Assert.AreEqual(controller.ViewBag.IsEditing, false);
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
            Assert.IsFalse(isDbChanged, "View actions cannot change DB.");
        }

        [TestMethod]
        public void CreateView_ViewNewOutOfWorkTime_HappyPath()
        {
            ScheduleController controller;
            bool isDbChanged = false;
            try
            {
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);
                var mr = new MockRepository();
                mr.SetRouteData_App_ConsultorioDrHourse_GregoryHouse();
                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr);
                this.db.SavingChanges += new EventHandler((s, e) => { isDbChanged = true; });
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // View new appointment.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Create((DateTime?)null, "00:00", "00:30", (int?)null);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");

            // Verify view-model.
            var viewResult = (ViewResult)actionResult;
            var viewModel = (AppointmentViewModel)viewResult.Model;
            Assert.AreEqual("00:00", viewModel.Start);
            Assert.AreEqual("00:30", viewModel.End);

            Assert.AreEqual(controller.ViewBag.IsEditing, false);
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
            Assert.IsFalse(isDbChanged, "View actions cannot change DB.");
        }

        [TestMethod]
        public void CreateView_2_ViewNewPredefinedPatient_HappyPath()
        {
            ScheduleController controller;
            bool isDbChanged = false;
            Patient patient;
            try
            {
                // Creating DB entries.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);
                patient = Firestarter.CreateFakePatients(docAndre, this.db).First();

                // Creating test objects.
                var mr = new MockRepository();
                mr.SetRouteData_App_ConsultorioDrHourse_GregoryHouse();
                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr);

                // Associating DB event.
                this.db.SavingChanges += new EventHandler((s, e) => { isDbChanged = true; });
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // View new appointment.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Create((DateTime?)null, "10:00", "10:30", (int?)patient.Id);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.IsInstanceOfType(viewResult.Model, typeof(AppointmentViewModel));
            var resultViewModel = (AppointmentViewModel)viewResult.Model;
            Assert.AreEqual(patient.Person.FullName, resultViewModel.PatientNameLookup);
            Assert.AreEqual(controller.ViewBag.IsEditing, false);
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
            Assert.IsFalse(isDbChanged, "View actions cannot change DB.");
        }

        [TestMethod]
        public void CreateView_3_ViewNewPredefinedPatientFromAnotherPractice()
        {
            ScheduleController controller;
            bool isDbChanged = false;
            Patient patient;
            try
            {
                // Creating DB entries.
                var docMarta = Firestarter.Create_CrmMg_Psiquiatria_DraMarta_Marta(this.db);
                Firestarter.SetupDoctor(docMarta, this.db);
                patient = Firestarter.CreateFakePatients(docMarta, this.db).First();
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);

                // Creating test objects.
                var mr = new MockRepository();
                mr.SetRouteData_App_ConsultorioDrHourse_GregoryHouse();
                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr);

                // Associating DB event.
                this.db.SavingChanges += new EventHandler((s, e) => { isDbChanged = true; });
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            // View new appointment.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Create((DateTime?)null, "10:00", "10:30", (int?)patient.Id);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.IsInstanceOfType(viewResult.Model, typeof(AppointmentViewModel));
            var resultViewModel = (AppointmentViewModel)viewResult.Model;
            Assert.AreNotEqual("Pedro Paulo Machado", resultViewModel.PatientNameLookup);
            Assert.AreEqual(controller.ViewBag.IsEditing, false);
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
            Assert.IsFalse(isDbChanged, "View actions cannot change DB.");
        }
        #endregion

        #region Create
        /// <summary>
        /// This test consists of creating an appointment that conflicts in time with another appointment that
        /// already exists in the DB.
        /// This is a valid operation and should result in two appointments registered at the same time.
        /// </summary>
        [TestMethod]
        public void Create_SaveAppointmentConflictingWithAnother_HappyPath()
        {
            ScheduleController controller;
            bool isDbChanged = false;
            AppointmentViewModel vm;

            // Dates that will be used by this test.
            // - utcNow and userNow: used to mock Now values from Utc and User point of view.
            // - start and end: start and end time of the appointments that will be created.
            var utcNow = new DateTime(2012, 07, 19, 12, 00, 00, 000, DateTimeKind.Utc);
            var userNow = new DateTime(2012, 07, 19, 12, 00, 00, 000, DateTimeKind.Local);

            // Setting Now to be on an wednesday, mid day.
            // We know that Dr. House works only after 13:00, so we need to set appointments after that.
            // 28 days from now in the future.
            var start = userNow.Date.AddDays(28).AddHours(13); // 2012-07-19 13:00
            var end = start.AddMinutes(30); // 2012-07-19 13:30

            try
            {
                // Creating practice and doctor.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);

                // Creating an appointment.
                var appointment = this.db.Appointments.CreateObject();
                appointment.CreatedBy = docAndre.Users.Single();
                appointment.CreatedOn = DateTime.Now;
                appointment.Description = "This is a generic appointment.";
                appointment.Doctor = docAndre;
                appointment.Start = start;
                appointment.End = end;
                appointment.Type = (int)TypeAppointment.GenericAppointment;
                this.db.SaveChanges();

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository();
                mr.SetRouteData_App_ConsultorioDrHourse_GregoryHouse();
                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr);

                controller.UserNowGetter = () => userNow;
                controller.UtcNowGetter = () => utcNow;

                // Setting view-model values to create a new appointment.
                // - this view-model must be valid for this test... if some day it becomes invalid,
                //   then it must be made valid again.
                vm = new AppointmentViewModel
                {
                    Description = "Another generic appointment.",
                    Date = start.Date,
                    DoctorId = docAndre.Id,
                    Start = start.ToString("HH:mm"),
                    End = end.ToString("HH:mm"),
                    IsGenericAppointment = true,
                };

                Mvc3TestHelper.SetModelStateErrors(controller, vm);

                if (!controller.ModelState.IsValid)
                    throw new Exception("The given view-model must be valid for this test.");

                // Events to know if database was changed or not.
                this.db.SavingChanges += new EventHandler((s, e) => { isDbChanged = true; });
            }
            catch (Exception ex)
            {
                Assert.Inconclusive(string.Format("Test initialization has failed.\n\n{0}", ex.FlattenMessages()));
                return;
            }

            // View new appointment.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Create(vm);
            }

            // Verifying the ActionResult.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(JsonResult));
            var jsonResult = (JsonResult)actionResult;
            Assert.AreEqual("success", ((dynamic)jsonResult.Data).status);

            // Verifying the view-model.
            // todo: this should verify a mocked message, but cannot mock messages until languages are implemented.
            Assert.AreEqual(
                "A data e hora já está marcada para outro compromisso.",
                vm.TimeValidationMessage);
            Assert.AreEqual(false, vm.IsTimeValid);

            // Verifying the controller.
            Assert.AreEqual(controller.ViewBag.IsEditing, null); // when JsonResult there must be no ViewBag
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            // Verifying the DB.
            Assert.IsTrue(isDbChanged, "View actions must change DB.");
            using (var db2 = new CerebelloEntities(this.db.Connection.ConnectionString))
            {
                int appointmentsCountAtSameTime = db2.Appointments
                    .Where(a => a.Start == start)
                    .Where(a => a.End == end)
                    .Count();

                Assert.AreEqual(2, appointmentsCountAtSameTime);
            }
        }

        /// <summary>
        /// This test consists of creating an appointment in the past.
        /// This is a valid operation.
        /// </summary>
        [TestMethod]
        public void Create_SaveAppointmentInThePast_HappyPath()
        {
            ScheduleController controller;
            bool isDbChanged = false;
            AppointmentViewModel vm;

            // Dates that will be used by this test.
            // - utcNow and userNow: used to mock Now values from Utc and User point of view.
            // - start and end: start and end time of the appointments that will be created.
            var utcNow = new DateTime(2012, 07, 19, 12, 00, 00, 000, DateTimeKind.Utc);
            var userNow = new DateTime(2012, 07, 19, 12, 00, 00, 000, DateTimeKind.Local);

            // We know that Dr. House works only after 13:00, so we need to set appointments after that.
            // Setting Now to be on an wednesday, mid day.
            // 28 days ago. (we are going to create an appointment in the past)
            var start = userNow.Date.AddDays(-28).AddHours(13);
            var end = start.AddMinutes(30);

            try
            {
                // Creating practice and doctor.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository();
                mr.SetRouteData_App_ConsultorioDrHourse_GregoryHouse();
                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr);

                // Mocking 'Now' values.
                controller.UserNowGetter = () => userNow;
                controller.UtcNowGetter = () => utcNow;

                // Setting view-model values to create a new appointment.
                // - this view-model must be valid for this test... if some day it becomes invalid,
                //   then it must be made valid again.
                vm = new AppointmentViewModel
                {
                    Description = "Generic appointment.",
                    Date = start.Date,
                    DoctorId = docAndre.Id,
                    Start = start.ToString("HH:mm"),
                    End = end.ToString("HH:mm"),
                    IsGenericAppointment = true,
                };

                Mvc3TestHelper.SetModelStateErrors(controller, vm);

                if (!controller.ModelState.IsValid)
                    throw new Exception("The given view-model must be valid for this test.");

                // Events to know if database was changed or not.
                this.db.SavingChanges += new EventHandler((s, e) => { isDbChanged = true; });
            }
            catch (Exception ex)
            {
                Assert.Inconclusive(string.Format("Test initialization has failed.\n\n{0}", ex.FlattenMessages()));
                return;
            }

            // View new appointment.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Create(vm);
            }

            // Verifying the ActionResult.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(JsonResult));
            var jsonResult = (JsonResult)actionResult;
            Assert.AreEqual("success", ((dynamic)jsonResult.Data).status);

            // Verifying the view-model.
            // todo: this should verify a mocked message, but cannot mock messages until languages are implemented.
            Assert.AreEqual(
                "A data e hora indicadas estão no passado.",
                vm.TimeValidationMessage);
            Assert.AreEqual(false, vm.IsTimeValid);

            // Verifying the controller.
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            // Verifying the DB.
            Assert.IsTrue(isDbChanged, "View actions must change DB.");
            using (var db2 = new CerebelloEntities(this.db.Connection.ConnectionString))
            {
                int appointmentsCountAtSameTime = db2.Appointments
                    .Where(a => a.Start == start)
                    .Where(a => a.End == end)
                    .Count();

                Assert.AreEqual(1, appointmentsCountAtSameTime);
            }
        }

        /// <summary>
        /// This test consists of creating an appointment in the past.
        /// This is a valid operation.
        /// </summary>
        [TestMethod]
        public void Create_SaveAppointmentOnHoliday_HappyPath()
        {
            ScheduleController controller;
            bool isDbChanged = false;
            AppointmentViewModel vm;

            // Dates that will be used by this test.
            // - utcNow and userNow: used to mock Now values from Utc and User point of view.
            // - start and end: start and end time of the appointments that will be created.
            var utcNow = new DateTime(2012, 07, 19, 12, 00, 00, 000, DateTimeKind.Utc);
            var userNow = new DateTime(2012, 07, 19, 12, 00, 00, 000, DateTimeKind.Local);

            // We know that Dr. House works only after 13:00, so we need to set appointments after that.
            // Setting Now to be on an wednesday, mid day.
            // 28 days ago. (we are going to create an appointment in the past)
            var start = userNow.Date.AddDays(28).AddHours(13);
            var end = start.AddMinutes(30);

            try
            {
                // Creating practice and doctor.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);

                // Creating a holiday in the day we will create the appointment.
                var holiday = this.db.SYS_Holiday.CreateObject();
                holiday.MonthAndDay = start.Month * 100 + start.Day;
                holiday.Name = "Holiday!";
                this.db.SYS_Holiday.AddObject(holiday);
                this.db.SaveChanges();

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository();
                mr.SetRouteData_App_ConsultorioDrHourse_GregoryHouse();
                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr);

                // Mocking 'Now' values.
                controller.UserNowGetter = () => userNow;
                controller.UtcNowGetter = () => utcNow;

                // Setting view-model values to create a new appointment.
                // - this view-model must be valid for this test... if some day it becomes invalid,
                //   then it must be made valid again.
                vm = new AppointmentViewModel
                {
                    Description = "Generic appointment.",
                    Date = start.Date,
                    DoctorId = docAndre.Id,
                    Start = start.ToString("HH:mm"),
                    End = end.ToString("HH:mm"),
                    IsGenericAppointment = true,
                };

                Mvc3TestHelper.SetModelStateErrors(controller, vm);

                if (!controller.ModelState.IsValid)
                    throw new Exception("The given view-model must be valid for this test.");

                // Events to know if database was changed or not.
                this.db.SavingChanges += new EventHandler((s, e) => { isDbChanged = true; });
            }
            catch (Exception ex)
            {
                Assert.Inconclusive(string.Format("Test initialization has failed.\n\n{0}", ex.FlattenMessages()));
                return;
            }

            // View new appointment.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Create(vm);
            }

            // Verifying the ActionResult.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(JsonResult));
            var jsonResult = (JsonResult)actionResult;
            Assert.AreEqual("success", ((dynamic)jsonResult.Data).status);

            // Verifying the view-model.
            // todo: this should verify a mocked message, but cannot mock messages until languages are implemented.
            Assert.AreEqual(
                "O campo 'Data da consulta' é inválido. Este dia é um feriado.",
                vm.TimeValidationMessage);
            Assert.AreEqual(false, vm.IsTimeValid);

            // Verifying the controller.
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            // Verifying the DB.
            Assert.IsTrue(isDbChanged, "View actions must change DB.");
            using (var db2 = new CerebelloEntities(this.db.Connection.ConnectionString))
            {
                int appointmentsCountAtSameTime = db2.Appointments
                    .Where(a => a.Start == start)
                    .Where(a => a.End == end)
                    .Count();

                Assert.AreEqual(1, appointmentsCountAtSameTime);
            }
        }

        /// <summary>
        /// This test consists of creating an appointment on lunch interval.
        /// This is a valid operation, and should result in no validation messages.
        /// </summary>
        [TestMethod]
        public void Create_SaveAppointmentOnLunch_HappyPath()
        {
            ScheduleController controller;
            bool isDbChanged = false;
            AppointmentViewModel vm;

            // Dates that will be used by this test.
            // - utcNow and userNow: used to mock Now values from Utc and User point of view.
            // - start and end: start and end time of the appointments that will be created.
            var utcNow = new DateTime(2012, 07, 19, 12, 00, 00, 000, DateTimeKind.Utc);
            var userNow = new DateTime(2012, 07, 19, 12, 00, 00, 000, DateTimeKind.Local);

            // Setting Now to be on an wednesday, mid day.
            // We know that Dr. House lunch time is from 12:00 until 13:00.
            var start = userNow.Date.AddDays(7).AddHours(12); // 2012-07-19 13:00
            var end = start.AddMinutes(30); // 2012-07-19 13:30

            try
            {
                // Creating practice and doctor.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository();
                mr.SetRouteData_App_ConsultorioDrHourse_GregoryHouse();
                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr);

                controller.UserNowGetter = () => userNow;
                controller.UtcNowGetter = () => utcNow;

                // Setting view-model values to create a new appointment.
                // - this view-model must be valid for this test... if some day it becomes invalid,
                //   then it must be made valid again.
                vm = new AppointmentViewModel
                {
                    Description = "Generic appointment on lunch time.",
                    Date = start.Date,
                    DoctorId = docAndre.Id,
                    Start = start.ToString("HH:mm"),
                    End = end.ToString("HH:mm"),
                    IsGenericAppointment = true,
                };

                Mvc3TestHelper.SetModelStateErrors(controller, vm);

                if (!controller.ModelState.IsValid)
                    throw new Exception("The given view-model must be valid for this test.");

                // Events to know if database was changed or not.
                this.db.SavingChanges += new EventHandler((s, e) => { isDbChanged = true; });
            }
            catch (Exception ex)
            {
                Assert.Inconclusive(string.Format("Test initialization has failed.\n\n{0}", ex.FlattenMessages()));
                return;
            }

            // View new appointment.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Create(vm);
            }

            // Verifying the ActionResult.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(JsonResult));
            var jsonResult = (JsonResult)actionResult;
            Assert.AreEqual("success", ((dynamic)jsonResult.Data).status);

            // Verifying the view-model.
            // todo: this should verify a mocked message, but cannot mock messages until languages are implemented.
            Assert.AreEqual(
                "A data e hora marcada está no horário de almoço do médico.",
                vm.TimeValidationMessage);
            Assert.AreEqual(false, vm.IsTimeValid);

            // Verifying the controller.
            Assert.AreEqual(controller.ViewBag.IsEditing, null); // when JsonResult there must be no ViewBag
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            // Verifying the DB.
            Assert.IsTrue(isDbChanged, "View actions must change DB.");
            using (var db2 = new CerebelloEntities(this.db.Connection.ConnectionString))
            {
                int appointmentsCountAtSameTime = db2.Appointments
                    .Where(a => a.Start == start)
                    .Where(a => a.End == end)
                    .Count();

                Assert.AreEqual(1, appointmentsCountAtSameTime);
            }
        }

        #endregion
    }
}
