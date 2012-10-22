using System;
using System.ComponentModel;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Web.Mvc;
using System.Net;
using System.Web.UI.WebControls;
using Cerebello.Model;

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
            this.CerebelloEntitiesCreator = () => new CerebelloEntities();
        }

        public Func<DateTime> UtcNowGetter { get; set; }

        public Action<MailMessage> EmailSender { get; set; }

        public Func<CerebelloEntities> CerebelloEntitiesCreator { get; set; }

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
        protected string RenderPartialViewToString(
            [JetBrains.Annotations.AspMvcView][JetBrains.Annotations.AspMvcPartialView] string viewName,
            object model = null)
        {
            var viewData = new ViewDataDictionary(model);
            var tempData = new TempDataDictionary();
            var viewResult = ViewEngines.Engines.FindPartialView(this.ControllerContext, viewName);
            using (var sw = new StringWriter())
            {
                var viewContext = new ViewContext(this.ControllerContext, viewResult.View, viewData, tempData, sw);
                viewResult.View.Render(viewContext, sw);
                return sw.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Creates an e-mail message.
        /// The 'From' address is fixed, and is valid in the Smtp server used by the 'SendEmail' method.
        /// </summary>
        /// <param name="toAddress">Address to send the message to.</param>
        /// <param name="subject">Subject of the message.</param>
        /// <param name="bodyHtml">Body of the message in Html format.</param>
        /// <param name="bodyText">Body of the message in plain text format.</param>
        /// <returns>Returns a 'MailMessage' that can be sent using the 'SendEmail' method.</returns>
        protected MailMessage CreateEmailMessage(
            MailAddress toAddress,
            [Localizable(true)] string subject,
            [Localizable(true)] string bodyHtml,
            [Localizable(true)] string bodyText)
        {
            if (string.IsNullOrEmpty(bodyText))
                throw new ArgumentException("bodyText must be provided.", "bodyText");

            // NOTE: The string "mig.ang.san.bic@gmail.com" is repeated in other place.
            var fromAddress = new MailAddress("mig.ang.san.bic@gmail.com", "www.cerebello.com");
            var mailMessage = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = bodyText.Trim() };

            // Adding Html body.
            if (!string.IsNullOrWhiteSpace(bodyHtml))
                mailMessage.AlternateViews.Add(
                    AlternateView.CreateAlternateViewFromString(bodyHtml.Trim(), new ContentType(MediaTypeNames.Text.Html)));

            return mailMessage;
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

        public virtual CerebelloEntities CreateNewCerebelloEntities()
        {
            return this.CerebelloEntitiesCreator();
        }
    }
}