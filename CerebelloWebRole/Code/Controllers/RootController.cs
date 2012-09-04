using System;
using System.IO;
using System.Net.Mail;
using System.Web.Mvc;
using System.Net;

namespace CerebelloWebRole.Code.Controllers
{
    public class RootController : Controller
    {
        public RootController()
        {
            this.UtcNowGetter = () => DateTime.UtcNow;
            this.EmailSender = mm =>
            {
                using (var smtpClient = this.CreateSmtpClient())
                    smtpClient.Send(mm);
            };
        }

        public Func<DateTime> UtcNowGetter { get; set; }

        public Action<MailMessage> EmailSender { get; set; }

        /// <summary>
        /// Mockable version of the DateTime.UtcNow property.
        /// </summary>
        /// <returns></returns>
        public DateTime GetUtcNow()
        {
            return this.UtcNowGetter();
        }

        /// <summary>
        /// Renders a partial view to a string.
        /// </summary>
        /// <param name="viewName">The name of the partial view to render.</param>
        /// <param name="model">The model objeto to pass to the partial view.</param>
        /// <returns>The string rendered from the partial view.</returns>
        protected string RenderPartialViewToString(string viewName, object model = null)
        {
            var viewData = new ViewDataDictionary(model);
            var tempData = new TempDataDictionary();
            ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(this.ControllerContext, viewName);
            using (StringWriter sw = new StringWriter())
            {
                ViewContext viewContext = new ViewContext(this.ControllerContext, viewResult.View, viewData, tempData, sw);
                viewResult.View.Render(viewContext, sw);
                return sw.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Creates an e-mail message. The 'From' address is fixed, and is valid in the Smtp server used by the 'SendEmail' method.
        /// </summary>
        /// <param name="toAddress">Address to send the message to.</param>
        /// <param name="subject">Subject of the message.</param>
        /// <param name="body">Body of the message. When 'isBodyHtml' parameter is true, this may contain Html, otherwise text only.</param>
        /// <param name="isBodyHtml">Defines whether the 'body' parameter contains Html or is pure text.</param>
        /// <returns>Returns a 'MailMessage' that can be sent using the 'SendEmail' method.</returns>
        protected MailMessage CreateEmailMessage(MailAddress toAddress, string subject, string body, bool isBodyHtml)
        {
            MailMessage message;
            message = new MailMessage
            {
                Subject = subject,
                Body = body
            };
            message.To.Add(toAddress);
            message.From = new MailAddress("mig.ang.san.bic@gmail.com", "www.cerebello.com");
            message.IsBodyHtml = isBodyHtml;
            return message;
        }

        /// <summary>
        /// Sends an e-mail message using the default SmtpClient.
        /// </summary>
        /// <param name="message">MailMessage containing the informations about the message to be sent.</param>
        protected void SendEmail(MailMessage message)
        {
            using (message)
                this.EmailSender(message);
        }

        /// <summary>
        /// Creates an SmtpClient that will be used to send e-mails.
        /// </summary>
        /// <returns></returns>
        private SmtpClient CreateSmtpClient()
        {
            var smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("mig.ang.san.bic@gmail.com", "50kf96wu")
            };

            return smtpClient;
        }
    }
}