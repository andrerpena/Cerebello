using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Code.Helpers;

namespace CerebelloWebRole.Code.Controllers
{
    public class RootController : Controller
    {
        public RootController()
        {
            this.UtcNowGetter = () => DateTime.UtcNow;

            this.CerebelloEntitiesCreator = () => new CerebelloEntities();
        }

        public Func<DateTime> UtcNowGetter { get; set; }

        public Func<CerebelloEntities> CerebelloEntitiesCreator { get; set; }

        /// <summary>
        /// Mockable version of the DateTime.UtcNow property.
        /// </summary>
        /// <returns></returns>
        public virtual DateTime GetUtcNow()
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
            return MvcHelper.RenderPartialViewToString(this.ControllerContext, viewName, model);
        }

        public EmailHelper.SendEmailAction EmailSender { get; set; }

        /// <summary>
        /// Sends an e-mail message using the default SmtpClient.
        /// </summary>
        /// <param name="message">MailMessage containing the informations about the message to be sent.</param>
        public virtual void SendEmail(MailMessage message)
        {
            try
            {
                (this.EmailSender ?? EmailHelper.SendEmail)(message);
            }
            catch (SmtpException exception)
            {
                if (this.HttpContext.Request.Url == null || !this.HttpContext.Request.Url.IsLoopback)
                    throw;
                this.SaveEmailLocal(message);
            }
        }

        public virtual void SaveEmailLocal(MailMessage message)
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var emailsPath = Path.Combine(desktopPath, @"Emails");

            Directory.CreateDirectory(emailsPath);

            var name = message.Subject + ".html";
            name = Regex.Replace(name, @"\s+", ".");
            name = Regex.Replace(name, @"[^\w\d]", ".", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, @"\.+", ".");

            var currentEmailPath = Path.Combine(emailsPath, name);

            using (var file = System.IO.File.Create(currentEmailPath))
            {
                message.AlternateViews.First().ContentStream.CopyTo(file);
            }
        }

        public virtual CerebelloEntities CreateNewCerebelloEntities()
        {
            return this.CerebelloEntitiesCreator();
        }
    }
}
