using System;
using System.Net.Mail;
using Cerebello.Model;
using CerebelloWebRole.Code.Helpers;

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
            return DateTime.UtcNow;
        }

        /// <summary>
        /// Sends an e-mail message using the default SmtpClient.
        /// </summary>
        /// <param name="message">MailMessage containing the informations about the message to be sent.</param>
        public virtual void SendEmail(MailMessage message)
        {
            EmailHelper.SendEmail(message);
        }

        /// <summary>
        /// Renders an embedded view to a string.
        /// This is useful to render e-mail messages.
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public string RenderView([JetBrains.Annotations.AspMvcView]string viewName, object model = null)
        {
            return RazorHelper.RenderEmbeddedRazor(this.GetType(), viewName, model);
        }
    }
}