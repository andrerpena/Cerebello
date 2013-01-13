using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Firestarter;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests
{
    [TestClass]
    public class ScheduleControllerTests : DbTestBase
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

        #region Create [Get]

        /// <summary>
        /// This test a graceful degradation,
        /// for a bug that is caused by an URL rendered in a view without all the needed params,
        /// that should never happen if the application is functioning correctly.
        /// But in this case, it ensures that app will degrade instead of being destroyed.
        /// Issue #54.
        /// </summary>
        [TestMethod]
        public void CreateView_ViewNewEmpty()
        {
            ScheduleController controller;
            bool isDbChanged = false;

            // Dates that will be used by this test.
            // - utcNow and localNow: used to mock Now values from Utc and User point of view.
            // - start and end: start and end time of the appointments that will be created.
            var localNow = new DateTime(2012, 07, 26, 12, 33, 00, 000);

            try
            {
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.Single().Practice.WindowsTimeZoneId);
                DateTime utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);

                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = mr.CreateController<ScheduleController>(
                    setupNewDb: db2 => db2.SavingChanges += (s, e) => { isDbChanged = true; });

                DateTimeHelper.SetUtcNow(utcNow);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // View new appointment.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Create(null, null, null, null, null);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");

            // Verify view-model.
            var viewResult = (ViewResult)actionResult;
            var viewModel = (AppointmentViewModel)viewResult.Model;
            Assert.AreEqual("12:33", viewModel.Start);
            Assert.AreEqual("13:03", viewModel.End);

            // Asserts related to the view bag.
            Assert.AreEqual(controller.ViewBag.IsEditingOrCreating, 'C');
            Assert.IsInstanceOfType(controller.ViewBag.HealthInsuranceSelectItems, typeof(List<SelectListItem>));
            Assert.AreEqual(3, ((List<SelectListItem>)controller.ViewBag.HealthInsuranceSelectItems).Count);

            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
            Assert.IsFalse(isDbChanged, "View actions cannot change DB.");
        }

        [TestMethod]
        public void CreateView_ViewNew_SpecificTime_HappyPath()
        {
            ScheduleController controller;
            bool isDbChanged = false;
            try
            {
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = mr.CreateController<ScheduleController>(
                    setupNewDb: db2 => db2.SavingChanges += (s, e) => { isDbChanged = true; });
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // View new appointment.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Create(null, "10:00", "10:30", null, false);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.AreEqual(controller.ViewBag.IsEditingOrCreating, 'C');
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
            Assert.IsFalse(isDbChanged, "View actions cannot change DB.");

            // Asserts related to view-model.
            var vm = (AppointmentViewModel)((ViewResult)actionResult).Model;
            Assert.AreEqual(null, vm.PatientId);
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
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = mr.CreateController<ScheduleController>(
                    setupNewDb: db2 => db2.SavingChanges += (s, e) => { isDbChanged = true; });
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // View new appointment.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Create(null, "10:07", "12:42", null, false);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");

            // Verify view-model.
            var viewResult = (ViewResult)actionResult;
            var viewModel = (AppointmentViewModel)viewResult.Model;
            Assert.AreEqual("10:07", viewModel.Start);
            Assert.AreEqual("12:42", viewModel.End);

            Assert.AreEqual(controller.ViewBag.IsEditingOrCreating, 'C');
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
            Assert.IsFalse(isDbChanged, "View actions cannot change DB.");
        }

        [TestMethod]
        public void CreateView_ViewNewAndFindNextAvailableTimeSlot_Now_HappyPath()
        {
            ScheduleController controller;
            bool isDbChanged = false;

            // Dates that will be used by this test.
            // - utcNow and localNow: used to mock Now values from Utc and User point of view.
            // - start and end: start and end time of the appointments that will be created.
            var localNow = new DateTime(2012, 07, 25, 12, 00, 00, 000);

            try
            {
                // Creating DB entries.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.Single().Practice.WindowsTimeZoneId);
                DateTime utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);

                // Filling current day, so that the next available time slot is on the next day.
                var start1 = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2012, 07, 25, 9, 00, 00, 000), timeZoneInfo);
                Firestarter.CreateFakeAppointment(this.db, utcNow, docAndre, start1, TimeSpan.FromHours(3), "Before mid-day.");
                var start2 = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2012, 07, 25, 13, 00, 00, 000), timeZoneInfo);
                Firestarter.CreateFakeAppointment(this.db, utcNow, docAndre, start2, TimeSpan.FromHours(5), "After mid-day.");

                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = mr.CreateController<ScheduleController>(
                    setupNewDb: db2 => db2.SavingChanges += (s, e) => { isDbChanged = true; });

                DateTimeHelper.SetUtcNow(utcNow);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // View new appointment.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Create(null, "", "", null, true);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");

            // Verify view-model.
            var viewResult = (ViewResult)actionResult;
            var viewModel = (AppointmentViewModel)viewResult.Model;
            Assert.AreEqual(new DateTime(2012, 07, 26), viewModel.LocalDateTime);
            Assert.AreEqual("09:00", viewModel.Start);
            Assert.AreEqual("09:30", viewModel.End);

            Assert.AreEqual(controller.ViewBag.IsEditingOrCreating, 'C');
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
            Assert.IsFalse(isDbChanged, "View actions cannot change DB.");
        }

        [TestMethod]
        public void CreateView_ViewNewAndFindNextAvailableTimeSlot_30DaysAfter_HappyPath()
        {
            ScheduleController controller;
            bool isDbChanged = false;

            // Dates that will be used by this test.
            // - utcNow and localNow: used to mock Now values from Utc and User point of view.
            // - start and end: start and end time of the appointments that will be created.
            var localNow = new DateTime(2012, 07, 25, 12, 00, 00, 000);

            try
            {
                // Creating DB entries.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.Single().Practice.WindowsTimeZoneId);
                DateTime utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);

                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = mr.CreateController<ScheduleController>(
                    setupNewDb: db2 => db2.SavingChanges += (s, e) => { isDbChanged = true; });

                DateTimeHelper.SetUtcNow(utcNow);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // View new appointment.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Create(localNow.AddDays(30).Date, "", "", null, true);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");

            // Verify view-model.
            var viewResult = (ViewResult)actionResult;
            var viewModel = (AppointmentViewModel)viewResult.Model;
            Assert.AreEqual(new DateTime(2012, 08, 24), viewModel.LocalDateTime);
            Assert.AreEqual("09:00", viewModel.Start);
            Assert.AreEqual("09:30", viewModel.End);

            Assert.AreEqual(controller.ViewBag.IsEditingOrCreating, 'C');
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
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = mr.CreateController<ScheduleController>(
                    setupNewDb: db2 => db2.SavingChanges += (s, e) => { isDbChanged = true; });
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // View new appointment.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Create(null, "00:00", "00:30", null, false);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");

            // Verify view-model.
            var viewResult = (ViewResult)actionResult;
            var viewModel = (AppointmentViewModel)viewResult.Model;
            Assert.AreEqual("00:00", viewModel.Start);
            Assert.AreEqual("00:30", viewModel.End);

            Assert.AreEqual(controller.ViewBag.IsEditingOrCreating, 'C');
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
            Assert.IsFalse(isDbChanged, "View actions cannot change DB.");
        }

        [TestMethod]
        public void CreateView_ViewNewPredefinedPatient_HappyPath()
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
                patient.LastUsedHealthInsuranceId = docAndre.HealthInsurances.First().Id;
                this.db.SaveChanges();

                // Creating test objects.
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = mr.CreateController<ScheduleController>(
                    setupNewDb: db2 => db2.SavingChanges += (s, e) => { isDbChanged = true; });
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // View new appointment.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Create(null, "10:00", "10:30", patient.Id, false);
            }

            // Asserts related to ActionResult.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;

            // Asserts related to view-model.
            Assert.IsInstanceOfType(viewResult.Model, typeof(AppointmentViewModel));
            var resultViewModel = (AppointmentViewModel)viewResult.Model;
            Assert.AreEqual(patient.Id, resultViewModel.PatientId);
            Assert.AreEqual(patient.Person.FullName, resultViewModel.PatientNameLookup);
            Assert.AreEqual(patient.LastUsedHealthInsuranceId, resultViewModel.HealthInsuranceId);

            // Asserts related to ViewBag.
            Assert.AreEqual(controller.ViewBag.IsEditingOrCreating, 'C');

            // Asserts related to ModelState.
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            // Asserts related to data base.
            Assert.IsFalse(isDbChanged, "View actions cannot change DB.");
        }

        /// <summary>
        /// Tries to get information about a patient from another practice.
        /// </summary>
        [TestMethod]
        public void CreateView_ViewNewPredefinedPatientFromAnotherPractice()
        {
            ScheduleController controller;
            bool isDbChanged = false;
            Patient patientFromAnotherPractice;
            try
            {
                // Creating DB entries.
                var docMarta = Firestarter.Create_CrmMg_Psiquiatria_DraMarta_Marta(this.db);
                Firestarter.SetupDoctor(docMarta, this.db);
                patientFromAnotherPractice = Firestarter.CreateFakePatients(docMarta, this.db, 1)[0];

                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);

                // Creating test objects.
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = mr.CreateController<ScheduleController>(
                    setupNewDb: db2 => db2.SavingChanges += (s, e) => { isDbChanged = true; });
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // View new appointment.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Create(null, "10:00", "10:30", patientFromAnotherPractice.Id, false);
            }

            // Asserts related to ActionResult.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;

            // Asserts related to view-model.
            Assert.IsInstanceOfType(viewResult.Model, typeof(AppointmentViewModel));
            var resultViewModel = (AppointmentViewModel)viewResult.Model;
            Assert.AreEqual(null, resultViewModel.PatientId);
            Assert.AreEqual(null, resultViewModel.PatientNameLookup);
            Assert.AreEqual(null, resultViewModel.HealthInsuranceId);

            // Asserts related to ViewBag.
            Assert.AreEqual(controller.ViewBag.IsEditingOrCreating, 'C');

            // Asserts related to ModelState.
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            // Asserts related to data base.
            Assert.IsFalse(isDbChanged, "View actions cannot change DB.");
        }

        #endregion

        #region Create [Post]

        /// <summary>
        /// Creates a medical appointment, in a free and valid time.
        /// </summary>
        [TestMethod]
        public void Create_SaveAppointment_HappyPath()
        {
            ScheduleController controller;
            bool isDbChanged = false;
            AppointmentViewModel vm;

            // Dates that will be used by this test.
            // - utcNow and localNow: used to mock Now values from Utc and User point of view.
            // - start and end: start and end time of the appointments that will be created.
            DateTime utcStart, utcEnd;
            var localNow = new DateTime(2012, 11, 09, 13, 00, 00, 000);

            // We know that Dr. House works only after 13:00, so we need to set appointments after that.
            var start = localNow.Date.AddDays(+7).AddHours(13);
            var end = start.AddMinutes(30);

            Patient patient;
            try
            {
                // Creating practice, doctor and patient.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);
                patient = Firestarter.CreateFakePatients(docAndre, this.db, 1)[0];
                patient.LastUsedHealthInsuranceId = null;
                this.db.SaveChanges();

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.Single().Practice.WindowsTimeZoneId);
                DateTime utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);
                utcStart = TimeZoneInfo.ConvertTimeToUtc(start, timeZoneInfo);
                utcEnd = TimeZoneInfo.ConvertTimeToUtc(end, timeZoneInfo);

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = mr.CreateController<ScheduleController>(
                    setupNewDb: db2 => db2.SavingChanges += (s, e) => { isDbChanged = true; });

                // Mocking 'Now' values.
                DateTimeHelper.SetUtcNow(utcNow);

                // Setting view-model values to create a new appointment.
                // - this view-model must be valid for this test... if some day it becomes invalid,
                //   then it must be made valid again.
                vm = new AppointmentViewModel
                {
                    PatientId = patient.Id,
                    PatientNameLookup = patient.Person.FullName,
                    HealthInsuranceId = docAndre.HealthInsurances.First(hi => hi.IsActive).Id,
                    LocalDateTime = start.Date,
                    DoctorId = docAndre.Id,
                    Start = start.ToString("HH:mm"),
                    End = end.ToString("HH:mm"),
                    IsGenericAppointment = false,
                    Status = (int)TypeAppointmentStatus.Undefined,
                };

                Mvc3TestHelper.SetModelStateErrors(controller, vm);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
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
                null,
                vm.TimeValidationMessage);
            Assert.AreEqual(DateAndTimeValidationState.Passed, vm.DateAndTimeValidationState);

            // Verifying the controller.
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid, but should be.");

            // Verifying the DB.
            Assert.IsTrue(isDbChanged, "Create actions must change DB.");
            using (var db2 = new CerebelloEntities(this.db.Connection.ConnectionString))
            {
                int appointmentsCountAtSameTime = db2.Appointments
                    .Count(a => a.Start == utcStart && a.End == utcEnd);

                Assert.AreEqual(1, appointmentsCountAtSameTime);

                var savedPatient = db2.Patients.Single(p => p.Id == patient.Id);
                Assert.AreEqual(vm.HealthInsuranceId, savedPatient.LastUsedHealthInsuranceId);

                var savedAppointment = savedPatient.Appointments.Single();
                Assert.AreEqual(vm.HealthInsuranceId, savedAppointment.HealthInsuranceId);
            }
        }

        /// <summary>
        /// This test consists of creating an appointment that conflicts in time with another appointment that
        /// already exists in the DB.
        /// This is a valid operation and should result in two appointments registered at the same time.
        /// </summary>
        [TestMethod]
        public void Create_SaveAppointmentConflictingWithAnother_HappyPath()
        {
            ScheduleController controller;
            var isDbChanged = false;
            AppointmentViewModel vm;

            // Dates that will be used by this test.
            // - utcNow and localNow: used to mock Now values from Utc and User point of view.
            // - start and end: start and end time of the appointments that will be created.
            DateTime utcStart, utcEnd;
            var localNow = new DateTime(2012, 07, 19, 12, 00, 00, 000);

            // Setting Now to be on an thursday, mid day.
            // We know that Dr. House works only after 13:00, so we need to set appointments after that.
            // 28 days from now in the future.
            var start = localNow.Date.AddDays(28).AddHours(13); // 2012-07-19 13:00
            var end = start.AddMinutes(30); // 2012-07-19 13:30

            try
            {
                // Creating practice and doctor.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.Single().Practice.WindowsTimeZoneId);
                var utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);
                utcStart = TimeZoneInfo.ConvertTimeToUtc(start, timeZoneInfo);
                utcEnd = TimeZoneInfo.ConvertTimeToUtc(end, timeZoneInfo);

                // Creating an appointment.
                var appointment = new Appointment
                    {
                        CreatedBy = docAndre.Users.Single(),
                        CreatedOn = utcNow,
                        Description = "This is a generic appointment.",
                        Doctor = docAndre,
                        Start = utcStart,
                        End = utcEnd,
                        Type = (int)TypeAppointment.GenericAppointment,
                        HealthInsuranceId = docAndre.HealthInsurances.First(hi => hi.IsActive).Id,
                        PracticeId = docAndre.PracticeId,
                    };
                this.db.Appointments.AddObject(appointment);
                this.db.SaveChanges();

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");

                controller = mr.CreateController<ScheduleController>(
                    setupNewDb: db2 => db2.SavingChanges += (s, e) => { isDbChanged = true; });

                DateTimeHelper.SetUtcNow(utcNow);

                // Setting view-model values to create a new appointment.
                // - this view-model must be valid for this test... if some day it becomes invalid,
                //   then it must be made valid again.
                vm = new AppointmentViewModel
                    {
                        Description = "Another generic appointment.",
                        LocalDateTime = start.Date,
                        DoctorId = docAndre.Id,
                        Start = start.ToString("HH:mm"),
                        End = end.ToString("HH:mm"),
                        IsGenericAppointment = true,
                        HealthInsuranceId = docAndre.HealthInsurances.First(hi => hi.IsActive).Id,
                    };

                Mvc3TestHelper.SetModelStateErrors(controller, vm);

                if (!controller.ModelState.IsValid)
                    throw new Exception("The given view-model must be valid for this test.");
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
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
            Assert.AreEqual(DateAndTimeValidationState.Warning, vm.DateAndTimeValidationState);

            // Verifying the controller.
            Assert.AreEqual(controller.ViewBag.IsEditingOrCreating, null); // when JsonResult there must be no ViewBag
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            // Verifying the DB.
            Assert.IsTrue(isDbChanged, "Create actions must change DB.");
            using (var db2 = new CerebelloEntities(this.db.Connection.ConnectionString))
            {
                int appointmentsCountAtSameTime = db2.Appointments
                    .Where(a => a.Start == utcStart)
                    .Count(a => a.End == utcEnd);

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
            // - utcNow and localNow: used to mock Now values from Utc and User point of view.
            // - start and end: start and end time of the appointments that will be created.
            DateTime utcStart, utcEnd;
            var localNow = new DateTime(2012, 07, 19, 12, 00, 00, 000);

            // We know that Dr. House works only after 13:00, so we need to set appointments after that.
            // Setting Now to be on an thursday, mid day.
            // 28 days ago. (we are going to create an appointment in the past)
            var start = localNow.Date.AddDays(-28).AddHours(13);
            var end = start.AddMinutes(30);

            try
            {
                // Creating practice and doctor.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.Single().Practice.WindowsTimeZoneId);
                DateTime utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);
                utcStart = TimeZoneInfo.ConvertTimeToUtc(start, timeZoneInfo);
                utcEnd = TimeZoneInfo.ConvertTimeToUtc(end, timeZoneInfo);

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = mr.CreateController<ScheduleController>(
                    setupNewDb: db2 => db2.SavingChanges += (s, e) => { isDbChanged = true; });

                // Mocking 'Now' values.
                DateTimeHelper.SetUtcNow(utcNow);

                // Setting view-model values to create a new appointment.
                // - this view-model must be valid for this test... if some day it becomes invalid,
                //   then it must be made valid again.
                vm = new AppointmentViewModel
                    {
                        Description = "Generic appointment.",
                        LocalDateTime = start.Date,
                        DoctorId = docAndre.Id,
                        Start = start.ToString("HH:mm"),
                        End = end.ToString("HH:mm"),
                        IsGenericAppointment = true,
                        HealthInsuranceId = docAndre.HealthInsurances.First(hi => hi.IsActive).Id,
                    };

                Mvc3TestHelper.SetModelStateErrors(controller, vm);

                if (!controller.ModelState.IsValid)
                    throw new Exception("The given view-model must be valid for this test.");
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
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
            Assert.AreEqual(DateAndTimeValidationState.Warning, vm.DateAndTimeValidationState);

            // Verifying the controller.
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            // Verifying the DB.
            Assert.IsTrue(isDbChanged, "Create actions must change DB.");
            using (var db2 = new CerebelloEntities(this.db.Connection.ConnectionString))
            {
                int appointmentsCountAtSameTime = db2.Appointments
                    .Count(a => a.Start == utcStart && a.End == utcEnd);

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
            // - utcNow and localNow: used to mock Now values from Utc and User point of view.
            // - start and end: start and end time of the appointments that will be created.
            DateTime utcStart, utcEnd;
            var localNow = new DateTime(2012, 07, 19, 12, 00, 00, 000);

            // We know that Dr. House works only after 13:00, so we need to set appointments after that.
            // Setting Now to be on an thursday, mid day.
            // 28 days ago. (we are going to create an appointment in the past)
            var start = localNow.Date.AddDays(28).AddHours(13);
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

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.Single().Practice.WindowsTimeZoneId);
                DateTime utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);
                utcStart = TimeZoneInfo.ConvertTimeToUtc(start, timeZoneInfo);
                utcEnd = TimeZoneInfo.ConvertTimeToUtc(end, timeZoneInfo);

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = mr.CreateController<ScheduleController>(
                    setupNewDb: db2 => db2.SavingChanges += (s, e) => { isDbChanged = true; });

                // Mocking 'Now' values.
                DateTimeHelper.SetUtcNow(utcNow);

                // Setting view-model values to create a new appointment.
                // - this view-model must be valid for this test... if some day it becomes invalid,
                //   then it must be made valid again.
                vm = new AppointmentViewModel
                    {
                        Description = "Generic appointment.",
                        LocalDateTime = start.Date,
                        DoctorId = docAndre.Id,
                        Start = start.ToString("HH:mm"),
                        End = end.ToString("HH:mm"),
                        IsGenericAppointment = true,
                        HealthInsuranceId = docAndre.HealthInsurances.First(hi => hi.IsActive).Id,
                    };

                Mvc3TestHelper.SetModelStateErrors(controller, vm);

                if (!controller.ModelState.IsValid)
                    throw new Exception("The given view-model must be valid for this test.");
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
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
            Assert.AreEqual(DateAndTimeValidationState.Warning, vm.DateAndTimeValidationState);

            // Verifying the controller.
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            // Verifying the DB.
            Assert.IsTrue(isDbChanged, "Create actions must change DB.");
            using (var db2 = new CerebelloEntities(this.db.Connection.ConnectionString))
            {
                int appointmentsCountAtSameTime = db2.Appointments
                    .Count(a => a.Start == utcStart && a.End == utcEnd);

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
            // - utcNow and localNow: used to mock Now values from Utc and User point of view.
            // - start and end: start and end time of the appointments that will be created.
            DateTime utcStart, utcEnd;
            var localNow = new DateTime(2012, 07, 19, 12, 00, 00, 000);

            // Setting Now to be on an thursday, mid day.
            // We know that Dr. House lunch time is from 12:00 until 13:00.
            var start = localNow.Date.AddDays(7).AddHours(12); // 2012-07-19 13:00
            var end = start.AddMinutes(30); // 2012-07-19 13:30

            try
            {
                // Creating practice and doctor.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.Single().Practice.WindowsTimeZoneId);
                DateTime utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);
                utcStart = TimeZoneInfo.ConvertTimeToUtc(start, timeZoneInfo);
                utcEnd = TimeZoneInfo.ConvertTimeToUtc(end, timeZoneInfo);

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = mr.CreateController<ScheduleController>(
                    setupNewDb: db2 => db2.SavingChanges += (s, e) => { isDbChanged = true; });

                DateTimeHelper.SetUtcNow(utcNow);

                // Setting view-model values to create a new appointment.
                // - this view-model must be valid for this test... if some day it becomes invalid,
                //   then it must be made valid again.
                vm = new AppointmentViewModel
                    {
                        Description = "Generic appointment on lunch time.",
                        LocalDateTime = start.Date,
                        DoctorId = docAndre.Id,
                        Start = start.ToString("HH:mm"),
                        End = end.ToString("HH:mm"),
                        IsGenericAppointment = true,
                        HealthInsuranceId = docAndre.HealthInsurances.First(hi => hi.IsActive).Id,
                    };

                Mvc3TestHelper.SetModelStateErrors(controller, vm);

                if (!controller.ModelState.IsValid)
                    throw new Exception("The given view-model must be valid for this test.");
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
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
            Assert.AreEqual(DateAndTimeValidationState.Warning, vm.DateAndTimeValidationState);

            // Verifying the controller.
            Assert.AreEqual(controller.ViewBag.IsEditingOrCreating, null); // when JsonResult there must be no ViewBag
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            // Verifying the DB.
            Assert.IsTrue(isDbChanged, "Create actions must change DB.");
            using (var db2 = new CerebelloEntities(this.db.Connection.ConnectionString))
            {
                var appointmentsCountAtSameTime = db2.Appointments
                    .Where(a => a.Start == utcStart).Count(a => a.End == utcEnd);

                Assert.AreEqual(1, appointmentsCountAtSameTime);
            }
        }

        /// <summary>
        /// This test consists of creating an appointment leaving all the form data missing.
        /// This is an invalid, and should result in validation messages.
        /// Issue #54.
        /// </summary>
        [TestMethod]
        public void Create_SaveWithEmptyFormModel()
        {
            ScheduleController controller;
            bool isDbChanged = false;
            AppointmentViewModel vm;


            var localNow = new DateTime(2012, 07, 19, 12, 00, 00, 000);

            // Setting Now to be on an thursday, mid day.
            // We know that Dr. House lunch time is from 12:00 until 13:00.

            try
            {
                // Creating practice and doctor.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.First().Practice.WindowsTimeZoneId);

                // Dates that will be used by this test.
                // - utcNow and localNow: used to mock Now values from Utc and User point of view.
                var utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = mr.CreateController<ScheduleController>(
                    setupNewDb: db2 => db2.SavingChanges += (s, e) => { isDbChanged = true; });

                DateTimeHelper.SetUtcNow(utcNow);

                // Setting view-model values to create a new appointment.
                // - this view-model must be valid for this test... if some day it becomes invalid,
                //   then it must be made valid again.
                vm = new AppointmentViewModel();

                Mvc3TestHelper.SetModelStateErrors(controller, vm);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
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

            // Verifying the view-model.
            Assert.AreEqual(DateAndTimeValidationState.Failed, vm.DateAndTimeValidationState);

            // Verifying the controller.
            Assert.AreEqual(controller.ViewBag.IsEditingOrCreating, 'C');
            Assert.IsInstanceOfType(controller.ViewBag.HealthInsuranceSelectItems, typeof(List<SelectListItem>));
            Assert.AreEqual(3, ((List<SelectListItem>)controller.ViewBag.HealthInsuranceSelectItems).Count);
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState should be invalid.");

            // Verifying the DB.
            Assert.IsFalse(isDbChanged, "Create actions must not change DB when there is an error.");
        }

        /// <summary>
        /// This test consists of creating an appointment for the future that sets the Status for
        /// NotAccomplished. Which must generate a ModelState error
        /// Issue #54.
        /// </summary>
        [TestMethod]
        public void Create_SaveAppointmentWhenStatusIsSetForTheFuture_NotAccomplished()
        {
            ScheduleController controller;
            AppointmentViewModel vm;

            try
            {
                // Creating practice, doctor and patient.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);
                var patient = Firestarter.CreateFakePatients(docAndre, this.db, 1)[0];
                patient.LastUsedHealthInsuranceId = null;
                this.db.SaveChanges();

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.Single().Practice.WindowsTimeZoneId);
                var localNow = new DateTime(2012, 11, 09, 13, 00, 00, 000);
                var utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);
                // the next free time from "now" 
                var db2 = new CerebelloEntitiesAccessFilterWrapper(this.db);
                db2.SetCurrentUserById(docAndre.Users.Single().Id);

                // the reason for this .AddMinutes(1) is that FindNextFreeTimeInPracticeLocalTime returns now, when now is available
                // but now is not considered future, and I need a date in the future so the Status validation will fail
                var nextFreeTime = ScheduleController.FindNextFreeTimeInPracticeLocalTime(db2, docAndre, localNow.AddMinutes(1));

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = mr.CreateController<ScheduleController>();

                // Mocking 'Now' values.
                DateTimeHelper.SetUtcNow(utcNow);

                // Setting view-model values to create a new appointment.
                vm = new AppointmentViewModel
                {
                    PatientId = patient.Id,
                    PatientNameLookup = patient.Person.FullName,
                    HealthInsuranceId = docAndre.HealthInsurances.First(hi => hi.IsActive).Id,
                    LocalDateTime = nextFreeTime.Item1.Date,
                    DoctorId = docAndre.Id,
                    Start = nextFreeTime.Item1.ToString("HH:mm"),
                    End = nextFreeTime.Item2.ToString("HH:mm"),
                    IsGenericAppointment = false,
                    // this has to generate an error because the appointment is in the future
                    Status = (int)TypeAppointmentStatus.NotAccomplished,
                };

                Mvc3TestHelper.SetModelStateErrors(controller, vm);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            var result = controller.Create(vm);

            Assert.IsFalse(controller.ModelState.IsValid);
            Assert.AreEqual(1, controller.ModelState["Status"].Errors.Count);
        }

        /// <summary>
        /// This test consists of creating an appointment for the future that sets the Status for
        /// Accomplished. Which must generate a ModelState error
        /// Issue #54.
        /// </summary>
        [TestMethod]
        public void Create_SaveAppointmentWhenStatusIsSetForTheFuture_Accomplished()
        {
            ScheduleController controller;
            AppointmentViewModel vm;

            try
            {
                // Creating practice, doctor and patient.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);
                var patient = Firestarter.CreateFakePatients(docAndre, this.db, 1)[0];
                patient.LastUsedHealthInsuranceId = null;
                this.db.SaveChanges();

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.Single().Practice.WindowsTimeZoneId);
                var localNow = new DateTime(2012, 11, 09, 13, 00, 00, 000);
                var utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);
                // the next free time from "now" 
                var db2 = new CerebelloEntitiesAccessFilterWrapper(this.db);
                db2.SetCurrentUserById(docAndre.Users.Single().Id);

                // the reason for this .AddMinutos(1) is that FindNextFreeTimeInPracticeLocalTime returns now, when now is available
                // but now is not considered future, and I need a date in the future so the Status validation will fail
                var nextFreeTime = ScheduleController.FindNextFreeTimeInPracticeLocalTime(db2, docAndre, localNow.AddMinutes(1));

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = mr.CreateController<ScheduleController>();

                // Mocking 'Now' values.
                DateTimeHelper.SetUtcNow(utcNow);

                // Setting view-model values to create a new appointment.
                vm = new AppointmentViewModel
                {
                    PatientId = patient.Id,
                    PatientNameLookup = patient.Person.FullName,
                    HealthInsuranceId = docAndre.HealthInsurances.First(hi => hi.IsActive).Id,
                    LocalDateTime = nextFreeTime.Item1.Date,
                    DoctorId = docAndre.Id,
                    Start = nextFreeTime.Item1.ToString("HH:mm"),
                    End = nextFreeTime.Item2.ToString("HH:mm"),
                    IsGenericAppointment = false,
                    // this has to generate an error because the appointment is in the future
                    Status = (int)TypeAppointmentStatus.Accomplished,
                };

                Mvc3TestHelper.SetModelStateErrors(controller, vm);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            controller.Create(vm);

            Assert.IsFalse(controller.ModelState.IsValid);
            Assert.AreEqual(1, controller.ModelState["Status"].Errors.Count);
        }

        /// <summary>
        /// This test consists of creating an appointment for the past that sets the Status for
        /// NotAccomplished. It should not generate an error
        /// Issue #54.
        /// </summary>
        [TestMethod]
        public void Create_SaveAppointmentWhenStatusIsSetForThePast_NotAccomplished()
        {
            ScheduleController controller;
            AppointmentViewModel vm;

            try
            {
                // Creating practice, doctor and patient.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);
                var patient = Firestarter.CreateFakePatients(docAndre, this.db, 1)[0];
                patient.LastUsedHealthInsuranceId = null;
                this.db.SaveChanges();

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.Single().Practice.WindowsTimeZoneId);
                var localNow = new DateTime(2012, 11, 09, 13, 00, 00, 000);
                var utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);
                // finds a free time in the past week.
                var db2 = new CerebelloEntitiesAccessFilterWrapper(this.db);
                db2.SetCurrentUserById(docAndre.Users.Single().Id);
                var freeTimeInPastWeek = ScheduleController.FindNextFreeTimeInPracticeLocalTime(db2, docAndre, localNow.AddDays(-7));

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = mr.CreateController<ScheduleController>();

                // Mocking 'Now' values.
                DateTimeHelper.SetUtcNow(utcNow);

                // Setting view-model values to create a new appointment.
                vm = new AppointmentViewModel
                {
                    PatientId = patient.Id,
                    PatientNameLookup = patient.Person.FullName,
                    HealthInsuranceId = docAndre.HealthInsurances.First(hi => hi.IsActive).Id,
                    LocalDateTime = freeTimeInPastWeek.Item1.Date,
                    DoctorId = docAndre.Id,
                    Start = freeTimeInPastWeek.Item1.ToString("HH:mm"),
                    End = freeTimeInPastWeek.Item2.ToString("HH:mm"),
                    IsGenericAppointment = false,
                    // this should work because it's in the past
                    Status = (int)TypeAppointmentStatus.NotAccomplished,
                };

                Mvc3TestHelper.SetModelStateErrors(controller, vm);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            controller.Create(vm);

            Assert.IsTrue(controller.ModelState.IsValid);
        }

        /// <summary>
        /// This test consists of creating an appointment for the past that sets the Status for
        /// Accomplished. It should not generate an error
        /// Issue #54.
        /// </summary>
        [TestMethod]
        public void Create_SaveAppointmentWhenStatusIsSetForThePast_Accomplished()
        {
            ScheduleController controller;
            AppointmentViewModel vm;

            try
            {
                // Creating practice, doctor and patient.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);
                var patient = Firestarter.CreateFakePatients(docAndre, this.db, 1)[0];
                patient.LastUsedHealthInsuranceId = null;
                this.db.SaveChanges();

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.Single().Practice.WindowsTimeZoneId);
                var localNow = new DateTime(2012, 11, 09, 13, 00, 00, 000);
                var utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);
                // finds a free time in the past week.
                var db2 = new CerebelloEntitiesAccessFilterWrapper(this.db);
                db2.SetCurrentUserById(docAndre.Users.Single().Id);
                var freeTimeInPastWeek = ScheduleController.FindNextFreeTimeInPracticeLocalTime(db2, docAndre, localNow.AddDays(-7));

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = mr.CreateController<ScheduleController>();

                // Mocking 'Now' values.
                DateTimeHelper.SetUtcNow(utcNow);

                // Setting view-model values to create a new appointment.
                vm = new AppointmentViewModel
                {
                    PatientId = patient.Id,
                    PatientNameLookup = patient.Person.FullName,
                    HealthInsuranceId = docAndre.HealthInsurances.First(hi => hi.IsActive).Id,
                    LocalDateTime = freeTimeInPastWeek.Item1.Date,
                    DoctorId = docAndre.Id,
                    Start = freeTimeInPastWeek.Item1.ToString("HH:mm"),
                    End = freeTimeInPastWeek.Item2.ToString("HH:mm"),
                    IsGenericAppointment = false,
                    // this should work because it's in the past
                    Status = (int)TypeAppointmentStatus.Accomplished,
                };

                Mvc3TestHelper.SetModelStateErrors(controller, vm);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            controller.Create(vm);

            Assert.IsTrue(controller.ModelState.IsValid);
        }

        #endregion

        #region Edit [Get]

        [TestMethod]
        public void EditView_View_HappyPath()
        {
            ScheduleController controller;
            bool isDbChanged = false;

            // Dates that will be used by this test.
            // - utcNow and localNow: used to mock Now values from Utc and User point of view.
            // - start and end: start and end time of the appointments that will be created.
            var localNow = new DateTime(2012, 11, 09, 16, 30, 00, 000);

            Appointment appointment;
            try
            {
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);
                var patient = Firestarter.CreateFakePatients(docAndre, this.db, 1)[0];
                appointment = Firestarter.CreateFakeAppointment(
                    this.db,
                    new DateTime(2012, 11, 05, 11, 35, 00, 000),
                    docAndre,
                    new DateTime(2012, 11, 09, 17, 00, 00, 000),
                    TimeSpan.FromMinutes(30.0),
                    patient);

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.Single().Practice.WindowsTimeZoneId);
                DateTime utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);

                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = mr.CreateController<ScheduleController>(
                    setupNewDb: db2 => db2.SavingChanges += (s, e) => { isDbChanged = true; });
                DateTimeHelper.SetUtcNow(utcNow);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // View new appointment.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Edit(appointment.Id);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");

            // Verify view-model.
            var viewResult = (ViewResult)actionResult;
            var viewModel = (AppointmentViewModel)viewResult.Model;
            Assert.AreEqual("15:00", viewModel.Start);
            Assert.AreEqual("15:30", viewModel.End);
            Assert.AreEqual(appointment.HealthInsuranceId, viewModel.HealthInsuranceId);

            // Asserts related to the view bag.
            Assert.AreEqual(controller.ViewBag.IsEditingOrCreating, 'E');
            Assert.IsInstanceOfType(controller.ViewBag.HealthInsuranceSelectItems, typeof(List<SelectListItem>));
            Assert.AreEqual(3, ((List<SelectListItem>)controller.ViewBag.HealthInsuranceSelectItems).Count);

            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid, but should be.");
            Assert.IsFalse(isDbChanged, "View actions cannot change DB.");
        }

        #endregion

        #region FindNextFreeTime

        [TestMethod]
        public void FindNextFreeTime_AllSlotsFree_HappyPath()
        {
            ScheduleController controller;
            var utcNow = new DateTime(2012, 09, 12, 18, 00, 00, DateTimeKind.Utc);
            bool isDbChanged = false;
            try
            {
                var andre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(andre, this.db);

                var mr = new MockRepository();
                mr.SetCurrentUser_Andre_CorrectPassword();
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "FindNextFreeTime");

                controller = mr.CreateController<ScheduleController>(
                    callOnActionExecuting: true,
                    setupNewDb: db2 => db2.SavingChanges += (s, e) => { isDbChanged = true; });

                DateTimeHelper.SetUtcNow(utcNow);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            JsonResult jsonResult;
            {
                jsonResult = controller.FindNextFreeTime("", "");
            }

            Assert.IsFalse(isDbChanged, "Database should not be changed.");

            dynamic data = jsonResult.Data;
            Assert.AreEqual("12/09/2012", data.date);
            Assert.AreEqual("15:00", data.start);
            Assert.AreEqual("15:30", data.end);
            Assert.AreEqual("quarta-feira, hoje", data.dateSpelled);
        }

        [TestMethod]
        public void FindNextFreeTime_SkipDoctorVacation_HappyPath()
        {
            ScheduleController controller;
            var utcNow = new DateTime(2012, 09, 12, 18, 00, 00, DateTimeKind.Utc);
            bool isDbChanged = false;
            try
            {
                var andre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(andre, this.db);

                var daysOff = DateTimeHelper.Range(new DateTime(2012, 09, 01), 30, d => d.AddDays(1.0))
                    .Select(
                        d => new CFG_DayOff
                            {
                                Date = d,
                                DoctorId = andre.Id,
                                Description = "Férias",
                                PracticeId = andre.PracticeId,
                            })
                    .ToArray();

                foreach (var eachDayOff in daysOff)
                    this.db.CFG_DayOff.AddObject(eachDayOff);

                this.db.SaveChanges();

                var mr = new MockRepository();
                mr.SetCurrentUser_Andre_CorrectPassword();
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "FindNextFreeTime");

                controller = mr.CreateController<ScheduleController>(
                    callOnActionExecuting: true,
                    setupNewDb: db2 => db2.SavingChanges += (s, e) => { isDbChanged = true; });

                DateTimeHelper.SetUtcNow(utcNow);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            JsonResult jsonResult;
            {
                jsonResult = controller.FindNextFreeTime("", "");
            }

            Assert.IsFalse(isDbChanged, "Database should not be changed.");

            dynamic data = jsonResult.Data;
            Assert.AreEqual("01/10/2012", data.date);
            Assert.AreEqual("09:00", data.start);
            Assert.AreEqual("09:30", data.end);
            Assert.AreEqual("segunda-feira, daqui a 19 dias", data.dateSpelled);
        }

        #endregion

        #region Delete

        [TestMethod]
        public void Delete_HappyPath()
        {
            ScheduleController controller;
            Appointment appointment;

            try
            {
                // Creating practice and doctor.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);

                Firestarter.CreateFakePatients(docAndre, db, 1);
                Patient patient = docAndre.Patients.First();

                var referenceTime = DateTime.UtcNow;
                appointment = new Appointment
                    {
                        Doctor = docAndre,
                        CreatedBy = docAndre.Users.First(),
                        CreatedOn = referenceTime,
                        PatientId = patient.Id,
                        Start = referenceTime,
                        End = referenceTime + TimeSpan.FromMinutes(30),
                        PracticeId = docAndre.PracticeId,
                        HealthInsuranceId = docAndre.HealthInsurances.First(hi => hi.IsActive).Id,
                    };

                this.db.Appointments.AddObject(appointment);
                this.db.SaveChanges();

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = mr.CreateController<ScheduleController>();
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            controller.Delete(appointment.Id);

            var deletedAppointment = this.db.Appointments.FirstOrDefault(a => a.Id == appointment.Id);
            Assert.IsNull(deletedAppointment);
        }

        #endregion
    }
}
