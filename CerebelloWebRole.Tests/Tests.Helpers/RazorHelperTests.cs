using System.Web.Mvc;
using CerebelloWebRole.Code;
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
            var viewDataDic = new ViewDataDictionary(emailViewModel);
            var result = RazorHelper.RenderEmbeddedRazor(typeof(EmailSenderWorker), "AppointmentReminderEmail", viewDataDic);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result), "'result' must not be null or empty.");
        }
    }
}
