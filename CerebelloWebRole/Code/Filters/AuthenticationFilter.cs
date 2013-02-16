using System;
using System.Net;
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
                var authenticatedPrincipal = httpContext.User as AuthenticatedPrincipal;

                if (authenticatedPrincipal == null)
                    throw new Exception("HttpContext.User should be a AuthenticatedPrincipal"
                                        + " when the user is authenticated");
            }
            else
            {
                // todo: check for json request: Request.AcceptTypes.Contains("application/json")
                // if it's an Ajax Request, must return a Json
                if (httpContext.Request.IsAjaxRequest())
                {
                    // todo: shouldn't Response.StatusCode be Unauthorized instead of Forbidden
                    // todo: replace this code by JsonForbiddenResult or JsonUnauthorizedResult

                    httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    filterContext.Result =
                        new JsonResult
                            {
                                Data = new
                                           {
                                               error = true,
                                               errorType = "unauthorized",
                                               errorMessage = "The current user is not authorized to access "
                                                              + "the resource because it's not authenticated"
                                           },
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet
                            };
                }
                // if it's a regular request, the user will be redirected to the login page
                else
                {
                    // if the user is in the "app" area but is not authenticated,
                    // he/she will be redirected to the login page
                    filterContext.Result = new HttpUnauthorizedResult();
                }
            }
        }

    }
}
