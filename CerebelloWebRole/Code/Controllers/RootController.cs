using System;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Code.Helpers;
using CerebelloWebRole.Models;
using JetBrains.Annotations;

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
        /// <param name="viewData">The viewData object containing the model object to pass to the partial view.</param>
        /// <returns>The string rendered from the partial view.</returns>
        protected string RenderPartialViewToString(
            [AspMvcView][AspMvcPartialView] string viewName,
            ViewDataDictionary viewData = null)
        {
            return MvcHelper.RenderPartialViewToString(this.ControllerContext, viewName, viewData);
        }

        public EmailHelper.SendEmailAction EmailSender { get; set; }

        /// <summary>
        /// Sends an e-mail message using the default SmtpClient.
        /// Tries 3 times before failing, and returning false. Returns true if succeded.
        /// </summary>
        /// <param name="message">MailMessage containing the informations about the message to be sent.</param>
        public virtual bool TrySendEmail(MailMessage message)
        {
            return EmailHelper.TrySendEmail(message);
        }

        public virtual CerebelloEntities CreateNewCerebelloEntities()
        {
            return this.CerebelloEntitiesCreator();
        }

        /// <summary>
        /// Creates an email message using an MVC view.
        /// The title, the html and text contents of the e-mail will be given by this view.
        /// </summary>
        /// <param name="contentView">View name to use to render the e-mail contents, and to get the text from.</param>
        /// <param name="toAddress">Address of the recipient.</param>
        /// <param name="model">Data that should be sent to the view.</param>
        /// <param name="sourceName">Source name for this e-mail.</param>
        /// <returns>Returns a 'MailMessage' that can be sent using the 'TrySendEmail' method.</returns>
        public virtual MailMessage CreateEmailMessage(
            [AspMvcView] [AspMvcPartialView] string contentView,
            MailAddress toAddress,
            object model,
            string sourceName = EmailHelper.DEFAULT_SOURCE)
        {
            return EmailHelper.CreateEmailMessageFromView(
                this.RenderPartialViewToString, contentView, toAddress, model, sourceName);
        }
    }
}
