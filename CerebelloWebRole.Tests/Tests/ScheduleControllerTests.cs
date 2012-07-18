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
            Assert.AreEqual("Pedro Paulo Machado", resultViewModel.PatientNameLookup);
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
    }
}
