﻿using CerebelloWebRole.Code.Helpers;
using CerebelloWebRole.WorkerRole.Code.Workers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests.Helpers
{
    [TestClass]
    public class RazorHelperTests
    {
        [TestMethod]
        public void RenderEmbeddedRazor_PatientEmail_AppointmentReminder()
        {
            RouteHelper.RegisterAllRoutes();
            var emailViewModel = new PatientEmailModel
                {
                    PatientName = "Miguel Angelo",
                    PracticeUrlId = "practiceurlid",
                };
            var result = RazorHelper.RenderEmbeddedRazor(typeof(EmailSenderWorker), "AppointmentReminderEmail", emailViewModel);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result), "'result' must not be null or empty.");
        }
    }
}