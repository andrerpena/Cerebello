using System;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Firestarter;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CerebelloWebRole.Code;

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
                actionResult = controller.Create((DateTime?)null, "10:00", "10:30", (int?)null, false);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.AreEqual(controller.ViewBag.IsEditingOrCreating, 'C');
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
            Assert.IsFalse(isDbChanged, "View actions cannot change DB.");
        }

        /// <summary>
        /// This test a graceful degradation,
        /// for a bug that is caused a URL rendered in a view without all the needed params,
        /// that should never happen if the application is functioning correctly.
        /// But in this case, it ensures that app will degrade instead of being destroyed.
        /// Issue #54.
        /// </summary>
        [TestMethod]
        public void CreateView_ViewNew_HappyPath()
        {
            ScheduleController controller;
            bool isDbChanged = false;

            // Dates that will be used by this test.
            // - utcNow and localNow: used to mock Now values from Utc and User point of view.
            // - start and end: start and end time of the appointments that will be created.
            DateTime utcNow;
            var localNow = new DateTime(2012, 07, 26, 12, 33, 00, 000);

            try
            {
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.FirstOrDefault().Practice.WindowsTimeZoneId);
                utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);

                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr);
                controller.UtcNowGetter = () => utcNow;
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
                actionResult = controller.Create((DateTime?)null, null, null, (int?)null, (bool?)null);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");

            // Verify view-model.
            var viewResult = (ViewResult)actionResult;
            var viewModel = (AppointmentViewModel)viewResult.Model;
            Assert.AreEqual("12:33", viewModel.Start);
            Assert.AreEqual("13:03", viewModel.End);

            Assert.AreEqual(controller.ViewBag.IsEditingOrCreating, 'C');
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
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
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
                actionResult = controller.Create((DateTime?)null, "10:07", "12:42", (int?)null, false);
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
            DateTime utcNow;
            var localNow = new DateTime(2012, 07, 25, 12, 00, 00, 000);

            try
            {
                // Creating DB entries.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.FirstOrDefault().Practice.WindowsTimeZoneId);
                utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);

                // Filling current day, so that the next available time slot is on the next day.
                var start1 = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2012, 07, 25, 9, 00, 00, 000), timeZoneInfo);
                Firestarter.CreateFakeAppointments(this.db, utcNow, docAndre, start1, TimeSpan.FromHours(3), "Before mid-day.");
                var start2 = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2012, 07, 25, 13, 00, 00, 000), timeZoneInfo);
                Firestarter.CreateFakeAppointments(this.db, utcNow, docAndre, start2, TimeSpan.FromHours(5), "After mid-day.");

                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr);
                controller.UtcNowGetter = () => utcNow;
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
                actionResult = controller.Create((DateTime?)null, "", "", (int?)null, true);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");

            // Verify view-model.
            var viewResult = (ViewResult)actionResult;
            var viewModel = (AppointmentViewModel)viewResult.Model;
            Assert.AreEqual(new DateTime(2012, 07, 26), viewModel.Date);
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
            DateTime utcNow;
            var localNow = new DateTime(2012, 07, 25, 12, 00, 00, 000);

            try
            {
                // Creating DB entries.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.FirstOrDefault().Practice.WindowsTimeZoneId);
                utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);

                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr);
                controller.UtcNowGetter = () => utcNow;
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
                actionResult = controller.Create(localNow.AddDays(30).Date, "", "", (int?)null, true);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");

            // Verify view-model.
            var viewResult = (ViewResult)actionResult;
            var viewModel = (AppointmentViewModel)viewResult.Model;
            Assert.AreEqual(new DateTime(2012, 08, 24), viewModel.Date);
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
                actionResult = controller.Create((DateTime?)null, "00:00", "00:30", (int?)null, false);
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

                // Creating test objects.
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
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
                actionResult = controller.Create((DateTime?)null, "10:00", "10:30", (int?)patient.Id, false);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.IsInstanceOfType(viewResult.Model, typeof(AppointmentViewModel));
            var resultViewModel = (AppointmentViewModel)viewResult.Model;
            Assert.AreEqual(patient.Person.FullName, resultViewModel.PatientNameLookup);
            Assert.AreEqual(controller.ViewBag.IsEditingOrCreating, 'C');
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
            Assert.IsFalse(isDbChanged, "View actions cannot change DB.");
        }

        [TestMethod]
        public void CreateView_ViewNewPredefinedPatientFromAnotherPractice()
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
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
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
                actionResult = controller.Create((DateTime?)null, "10:00", "10:30", (int?)patient.Id, false);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.IsInstanceOfType(viewResult.Model, typeof(AppointmentViewModel));
            var resultViewModel = (AppointmentViewModel)viewResult.Model;
            Assert.AreNotEqual("Pedro Paulo Machado", resultViewModel.PatientNameLookup);
            Assert.AreEqual(controller.ViewBag.IsEditingOrCreating, 'C');
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
            Assert.IsFalse(isDbChanged, "View actions cannot change DB.");
        }
        #endregion

        #region Create [Post]
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

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.FirstOrDefault().Practice.WindowsTimeZoneId);
                var utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);
                utcStart = TimeZoneInfo.ConvertTimeToUtc(start, timeZoneInfo);
                utcEnd = TimeZoneInfo.ConvertTimeToUtc(end, timeZoneInfo);

                // Creating an appointment.
                var appointment = this.db.Appointments.CreateObject();
                appointment.CreatedBy = docAndre.Users.Single();
                appointment.CreatedOn = utcNow;
                appointment.Description = "This is a generic appointment.";
                appointment.Doctor = docAndre;
                appointment.Start = utcStart;
                appointment.End = utcEnd;
                appointment.Type = (int)TypeAppointment.GenericAppointment;
                this.db.SaveChanges();

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr);

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
                this.db.SavingChanges += (s, e) => { isDbChanged = true; };
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
            DateTime utcNow, utcStart, utcEnd;
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

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.FirstOrDefault().Practice.WindowsTimeZoneId);
                utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);
                utcStart = TimeZoneInfo.ConvertTimeToUtc(start, timeZoneInfo);
                utcEnd = TimeZoneInfo.ConvertTimeToUtc(end, timeZoneInfo);

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr);

                // Mocking 'Now' values.
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
            Assert.AreEqual(DateAndTimeValidationState.Warning, vm.DateAndTimeValidationState);

            // Verifying the controller.
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            // Verifying the DB.
            Assert.IsTrue(isDbChanged, "Create actions must change DB.");
            using (var db2 = new CerebelloEntities(this.db.Connection.ConnectionString))
            {
                int appointmentsCountAtSameTime = db2.Appointments
                    .Where(a => a.Start == utcStart)
                    .Where(a => a.End == utcEnd)
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
            // - utcNow and localNow: used to mock Now values from Utc and User point of view.
            // - start and end: start and end time of the appointments that will be created.
            DateTime utcNow, utcStart, utcEnd;
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

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.FirstOrDefault().Practice.WindowsTimeZoneId);
                utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);
                utcStart = TimeZoneInfo.ConvertTimeToUtc(start, timeZoneInfo);
                utcEnd = TimeZoneInfo.ConvertTimeToUtc(end, timeZoneInfo);

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr);

                // Mocking 'Now' values.
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
            Assert.AreEqual(DateAndTimeValidationState.Warning, vm.DateAndTimeValidationState);

            // Verifying the controller.
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");

            // Verifying the DB.
            Assert.IsTrue(isDbChanged, "Create actions must change DB.");
            using (var db2 = new CerebelloEntities(this.db.Connection.ConnectionString))
            {
                int appointmentsCountAtSameTime = db2.Appointments
                    .Where(a => a.Start == utcStart)
                    .Where(a => a.End == utcEnd)
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
            // - utcNow and localNow: used to mock Now values from Utc and User point of view.
            // - start and end: start and end time of the appointments that will be created.
            DateTime utcNow, utcStart, utcEnd;
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

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.FirstOrDefault().Practice.WindowsTimeZoneId);
                utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);
                utcStart = TimeZoneInfo.ConvertTimeToUtc(start, timeZoneInfo);
                utcEnd = TimeZoneInfo.ConvertTimeToUtc(end, timeZoneInfo);

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr);

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
            // - start and end: start and end time of the appointments that will be created.
            var start = localNow.Date.AddDays(7).AddHours(12); // 2012-07-19 13:00
            var end = start.AddMinutes(30); // 2012-07-19 13:30

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
                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr);

                controller.UtcNowGetter = () => utcNow;

                // Setting view-model values to create a new appointment.
                // - this view-model must be valid for this test... if some day it becomes invalid,
                //   then it must be made valid again.
                vm = new AppointmentViewModel
                {
                };

                Mvc3TestHelper.SetModelStateErrors(controller, vm);

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
            var viewResult = (ViewResult)actionResult;

            // Verifying the view-model.
            Assert.AreEqual(DateAndTimeValidationState.Failed, vm.DateAndTimeValidationState);

            // Verifying the controller.
            Assert.AreEqual(controller.ViewBag.IsEditingOrCreating, 'C');
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState should be invalid.");

            // Verifying the DB.
            Assert.IsFalse(isDbChanged, "Create actions must not change DB when there is an error.");
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

                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr, callOnActionExecuting: true);
                controller.UtcNowGetter = () => utcNow;

                this.db.SavingChanges += (s, e) => { isDbChanged = true; };
            }
            catch (Exception ex)
            {
                Assert.Inconclusive("Test initialization has failed.\n\n{0}", ex.FlattenMessages());
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
                    .Select(d => new CFG_DayOff { Date = d, DoctorId = andre.Id, Description = "Férias" })
                    .ToArray();

                foreach (var eachDayOff in daysOff)
                    this.db.CFG_DayOff.AddObject(eachDayOff);

                this.db.SaveChanges();

                var mr = new MockRepository();
                mr.SetCurrentUser_Andre_CorrectPassword();
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "FindNextFreeTime");

                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr, callOnActionExecuting: true);
                controller.UtcNowGetter = () => utcNow;

                this.db.SavingChanges += (s, e) => { isDbChanged = true; };
            }
            catch (Exception ex)
            {
                Assert.Inconclusive("Test initialization has failed.\n\n{0}", ex.FlattenMessages());
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
            Patient patient;
            Appointment appointment;

            try
            {
                // Creating practice and doctor.
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);

                Firestarter.CreateFakePatients(docAndre, db, 1);
                patient = docAndre.Patients.First();

                var referenceTime = DateTime.UtcNow;
                appointment = new Appointment()
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

                // Creating Asp.Net Mvc mocks.
                var mr = new MockRepository(true);
                mr.SetRouteData_ConsultorioDrHouse_GregoryHouse(typeof(ScheduleController), "Create");
                controller = Mvc3TestHelper.CreateControllerForTesting<ScheduleController>(this.db, mr);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive(string.Format("Test initialization has failed.\n\n{0}", ex.FlattenMessages()));
                return;
            }

            controller.Delete(appointment.Id);

            var deletedAppointment = this.db.Appointments.FirstOrDefault(a => a.Id == appointment.Id);
            Assert.IsNull(deletedAppointment);
        }

        #endregion
    }
}
