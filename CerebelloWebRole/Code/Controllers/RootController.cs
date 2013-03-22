using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
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

            this.CerebelloEntitiesCreator = () => new CerebelloEntities(DebugConfig.DataBaseConnectionString);
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
            [AspMvcPartialView] string viewName,
            ViewDataDictionary viewData = null)
        {
            return MvcHelper.RenderPartialViewToString(this.ControllerContext, viewName, viewData);
        }

        /// <summary>
        /// Renders a partial view to a string.
        /// </summary>
        /// <param name="viewName">The name of the partial view to render.</param>
        /// <param name="viewData">The viewData object containing the model object to pass to the partial view.</param>
        /// <returns>The string rendered from the partial view.</returns>
        protected string RenderViewToString(
            [AspMvcView] string viewName,
            ViewDataDictionary viewData = null)
        {
            return MvcHelper.RenderViewToString(this.ControllerContext, viewName, viewData);
        }

        /// <summary>
        /// Renders a partial view to a string.
        /// </summary>
        /// <param name="viewName">The name of the partial view to render.</param>
        /// <param name="viewData">The viewData object containing the model object to pass to the partial view.</param>
        /// <param name="masterName">Name of the layout page.</param>
        /// <returns>The string rendered from the partial view.</returns>
        protected string RenderViewToString(
            [AspMvcView] string viewName,
            ViewDataDictionary viewData,
            [AspMvcMaster]string masterName)
        {
            return MvcHelper.RenderViewToString(this.ControllerContext, viewName, viewData, masterName);
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

        /// <summary>
        /// Sends an e-mail message using the default SmtpClient.
        /// Tries 6 times before failing, and returning an exception in the task.
        /// </summary>
        /// <param name="message">MailMessage containing the informations about the message to be sent.</param>
        /// <returns>The <see cref="Task"/> object containing information about the task. </returns>
        public virtual Task SendEmailAsync(MailMessage message)
        {
            return EmailHelper.SendEmailAsync(message);
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
        public virtual MailMessage CreateEmailMessagePartial(
            [AspMvcPartialView] string contentView,
            MailAddress toAddress,
            object model,
            string sourceName = EmailHelper.DEFAULT_SOURCE)
        {
            return EmailHelper.CreateEmailMessageFromView(
                this.RenderPartialViewToString, contentView, toAddress, model, sourceName);
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
            [AspMvcView] string contentView,
            MailAddress toAddress,
            object model,
            string sourceName = EmailHelper.DEFAULT_SOURCE)
        {
            return EmailHelper.CreateEmailMessageFromView(
                this.RenderViewToString, contentView, toAddress, model, sourceName);
        }
    }
}
