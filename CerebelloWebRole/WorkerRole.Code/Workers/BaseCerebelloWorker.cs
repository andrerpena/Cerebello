using System;
using System.Net.Mail;
using System.Web.Mvc;
using CerebelloWebRole.Code;
using JetBrains.Annotations;

namespace CerebelloWebRole.WorkerRole.Code.Workers
{
    public abstract class BaseCerebelloWorker : BaseWorker
    {
        public virtual CerebelloEntities CreateNewCerebelloEntities()
        {
            return new CerebelloEntities();
        }

        /// <summary>
        /// Mockable version of the DateTime.UtcNow property.
        /// </summary>
        /// <returns>Returns DateTime.UtcNow.</returns>
        public virtual DateTime GetUtcNow()
        {
            return DateTime.UtcNow + DebugConfig.CurrentTimeOffset;
        }

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
            return EmailHelper.CreateEmailMessageFromView(this.RenderView, contentView, toAddress, model, sourceName);
        }

        /// <summary>
        /// Renders an embedded view to a string.
        /// This is useful to render e-mail messages.
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="viewData"></param>
        /// <returns></returns>
        public string RenderView([AspMvcView]string viewName, ViewDataDictionary viewData = null)
        {
            return RazorHelper.RenderEmbeddedRazor(this.GetWorkerType(), viewName, viewData);
        }

        public virtual Type GetWorkerType()
        {
            return this.GetType();
        }
    }
}