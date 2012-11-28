using System;
using System.Web.Mvc;
using CerebelloWebRole.Code.Controllers;
using CerebelloWebRole.WorkerRole.Code.Workers;

namespace CerebelloWebRole.Controllers
{
    public class MasterAdminController : RootController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SendReminderEmails()
        {
            using (this.CreateNewCerebelloEntities())
            {
                EmailSenderWorker emailSender;
                try
                {
                    emailSender = new EmailSenderWorker();
                    emailSender.RunOnce();
                }
                catch (Exception ex)
                {
                    return this.Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
                return this.Json(new { success = true, message = "E-mails enviados com sucesso: " + emailSender.EmailsCount }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
