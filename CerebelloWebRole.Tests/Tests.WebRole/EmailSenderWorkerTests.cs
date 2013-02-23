using System;
using System.Linq;
using System.Net.Mail;
using Cerebello.Firestarter;
using Cerebello.Model;
using CerebelloWebRole.WorkerRole.Code.Workers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CerebelloWebRole.Tests.Tests.Helpers
{
    [TestClass]
    public class EmailSenderWorkerTests : DbTestBase
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
        public void SendEmailToUser_HappyPath()
        {
            bool isDbChanged = false;

            // Dates that will be used by this test.
            // - utcNow and localNow: used to mock Now values from Utc and User point of view.
            // - start and end: start and end time of the appointments that will be created.
            var localNow = new DateTime(2012, 11, 23, 10, 00, 00, 000);

            MailMessage mailMessage = null;

            EmailSenderWorker worker;
            try
            {
                var docAndre = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                Firestarter.SetupDoctor(docAndre, this.db);

                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(docAndre.Users.Single().Practice.WindowsTimeZoneId);
                DateTime utcNow = TimeZoneInfo.ConvertTimeToUtc(localNow, timeZoneInfo);

                // Create an appointment for tomorrow.
                Patient patient = Firestarter.CreateFakePatients(docAndre, this.db, 1).Single();
                Firestarter.CreateFakeAppointment(this.db, utcNow, docAndre, utcNow.AddDays(1.0), TimeSpan.FromMinutes(30.0), patient);

                var workerMock = new Mock<EmailSenderWorker> { CallBase = true };
                workerMock.Setup(w => w.CreateNewCerebelloEntities()).Returns(() =>
                {
                    var db2 = CreateNewCerebelloEntities();
                    db2.SavingChanges += (e, s) => isDbChanged = true;
                    return db2;
                });
                workerMock.Setup(w => w.GetUtcNow()).Returns(utcNow);
                workerMock.Setup(w => w.GetWorkerType()).Returns(typeof(EmailSenderWorker));
                workerMock.Setup(w => w.TrySendEmail(It.IsAny<MailMessage>())).Callback((MailMessage mm) => { mailMessage = mm; });
                worker = workerMock.Object;
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            worker.RunOnce();

            Assert.IsTrue(isDbChanged, "Database must be saved.");
            Assert.IsNotNull(mailMessage, "mailMessage must have been sent.");

            using (var db3 = CreateNewCerebelloEntities())
            {
                var appointment = db3.Appointments.Single();
                Assert.IsTrue(appointment.ReminderEmailSent, "ReminderEmailSent flag must be set.");
            }
        }
    }
}
