using System;
using System.Web.Mvc;
using System.Web.Security;
using CerebelloWebRole.Code.Security;

namespace CerebelloWebRole.Code.Filters
{
    public class AuthenticationFilter : IAuthorizationFilter
    {
        // reference:
        // if someday we have problems with caching restricted-access pages, the following could be useful:
        // http://farm-fresh-code.blogspot.com.br/2009/11/customizing-authorization-in-aspnet-mvc.html

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            // if the user is in the "app" area, then he/she can only pass when authenticated
            var dataTokens = filterContext.RouteData.DataTokens;
            var httpContext = filterContext.HttpContext;

            if (!dataTokens.ContainsKey("area") || dataTokens["area"].ToString().ToLower() != "app")
                return;

            // forcing all controller in the 'App' area to inherit from CerebelloController...
            // also, now that it is assured that the controller is a CerebelloController,
            // we can use all of it's properties and methods
            var controller = filterContext.Controller as CerebelloController;
            if (controller == null)
                throw new Exception("Controllers in the 'App' area must inherit from 'CerebelloController'.");

            var isAuthenticated = httpContext.Request.IsAuthenticated;
            if (isAuthenticated)
            {
                var authenticatedPrincipal = httpContext.User as AuthenticatedPrincipal;

                if (authenticatedPrincipal == null)
                    throw new Exception(
                        "HttpContext.User should be a AuthenticatedPrincipal"
                        + " when the user is authenticated");

                // Signout user if the account has been disabled.
                if (controller.InitDb().AccountDisabled)
                {
                    isAuthenticated = false;
                    FormsAuthentication.SignOut();
                }
            }

            // if the user is still authenticated, then exit this method
            if (!isAuthenticated)
            {
                // if the user is in the "App" area but is not authenticated:
                // not ajax: user will be redirected to the login page
                // ajax: return a Json with information
                filterContext.Result = new UnauthorizedResult(
                    "The current user is not authorized to access the resource because it's not authenticated");
            }
        }

    }
}
