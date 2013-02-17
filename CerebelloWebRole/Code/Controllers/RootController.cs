using System;
using System.Net.Mail;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Code.Helpers;

namespace CerebelloWebRole.Code.Controllers
{
    public class RootController : Controller
    {
        public RootController()
        {
            this.UtcNowGetter = () => DateTime.UtcNow + DebugConfig.CurrentTimeOffset;

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
            (this.EmailSender ?? EmailHelper.SendEmail)(message);
        }

        public virtual CerebelloEntities CreateNewCerebelloEntities()
        {
            return this.CerebelloEntitiesCreator();
        }
    }
}
