using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
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
            (this.EmailSender ?? EmailHelper.SendEmail)(message);
        }

        public virtual CerebelloEntities CreateNewCerebelloEntities()
        {
            return this.CerebelloEntitiesCreator();
        }

        #region Transaction [needs changes - not good as is now]
        /// <summary>
        /// Overrides the default IActionInvoker used by this controller, by one that allows usage of
        /// the special attribute TransactionScopeAttribute that creates a transaction wide enough
        /// to cover all filters and the action method itself.
        /// </summary>
        /// <returns></returns>
        protected override IActionInvoker CreateActionInvoker()
        {
            return new RootActionInvoker();
        }

        /// <summary>
        /// ControllerActionInvoker that allows the usage of TransactionScopeAttribute over action methods.
        /// </summary>
        class RootActionInvoker : ControllerActionInvoker
        {
            public override bool InvokeAction(ControllerContext controllerContext, string actionName)
            {
                // reading transaction attribute
                ControllerDescriptor controllerDescriptor = GetControllerDescriptor(controllerContext);
                ActionDescriptor actionDescriptor = FindAction(controllerContext, controllerDescriptor, actionName);
                var transactionAttribute = actionDescriptor
                    .GetCustomAttributes(true).OfType<TransactionScopeAttribute>()
                    .SingleOrDefault();

                var rootController = (RootController)controllerContext.Controller;

                // creating transaction if needed
                TransactionScope scope = null;
                if (transactionAttribute != null)
                    scope = new TransactionScope(transactionAttribute.ScopeOption);
                rootController.TransactionScope = scope;

                try
                {
                    return base.InvokeAction(controllerContext, actionName);
                }
                finally
                {
                    if (scope != null)
                        scope.Dispose();
                }
            }
        }

        /// <summary>
        /// Transaction scope created when using a TransactionScopeAttribute over the action method.
        /// This transaction must have the Complete method called by the action, or a filter of that action,
        /// when a commit is desired. If this is not done, the transaction will be rolled-back.
        /// </summary>
        public TransactionScope TransactionScope { get; set; }
        #endregion
    }
}
