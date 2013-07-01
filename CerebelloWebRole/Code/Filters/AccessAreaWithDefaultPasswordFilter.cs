using System;
using System.Net;
using System.Web.Mvc;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code
{
    // todo: this should be an IActionFilter instead of an IAuthorizationFilter
    public class AccessAreaWithDefaultPasswordFilter : IAuthorizationFilter
    {
        private readonly string areaName;
        private readonly string controllerName;
        private readonly string actionName;

        public AccessAreaWithDefaultPasswordFilter([AspMvcArea] string areaName, [AspMvcController] string controllerName, [AspMvcAction] string actionName)
        {
            this.areaName = areaName;
            this.controllerName = controllerName;
            this.actionName = actionName;
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var dataTokens = filterContext.RouteData.DataTokens;
            if (!dataTokens.ContainsKey("area") || !string.Equals("" + dataTokens["area"], this.areaName, StringComparison.InvariantCultureIgnoreCase))
                return;

            if (!filterContext.HttpContext.Request.IsAuthenticated)
                return;

            var authenticatedPrincipal = filterContext.HttpContext.User as AuthenticatedPrincipal;

            if (authenticatedPrincipal == null)
                return;

            // When the user is accessing the software for the first time,
            // he/she will be asked to set an access password, at this place:
            // https://www.cerebello.com.br/p/consultoriodrhouse/users/changepassword
            // All other places in the software will redirect the user back to that
            // place until he/she sets the password.
            bool isUsingDefaultPwd = authenticatedPrincipal.Profile.IsUsingDefaultPassword;

            bool isRedirectNeeded =
                string.Equals("" + dataTokens["controller"], this.controllerName, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals("" + dataTokens["action"], this.actionName, StringComparison.InvariantCultureIgnoreCase);

            if (!isUsingDefaultPwd || !isRedirectNeeded)
                return;

            // Ajax requests have no user interaction, so we just return an error to the browser.
            // Normal requests allow the user to correct the issue that is causing the error, by redirecting to the page that does this.
            if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
            {
                // Response.StatusCode:
                // - Unauthorized when the user will have access to the resource after creating a valid password;
                // - Forbidden when the user will be denied access to the resource after creating a valid password.
                //    This is not needed since, forbidden places will be filtered by the authentication filters,
                //    so that only places that the user will have access will be processed by this code.

                // todo: cannot return Unauthorized yet, because of Asp.Net module that handles Unauthorized
                // todo: results and redirects them to the login page, causing the ajax request to get the Html of the login page.
                filterContext.Result = new StatusCodeResult(HttpStatusCode.Forbidden, "The current user is not authorized to access the resource because his/her password has not been reset yet");
            }
            else
            {
                // Cannot access resource. Redirecting to page where the user may correct any pending issue.
                filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary
                    {
                        { "area", this.areaName },
                        { "controller", this.controllerName },
                        { "action", this.actionName },
                    });
            }
        }
    }
}