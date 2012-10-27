using System;
using System.Net;
using System.Web.Mvc;
using CerebelloWebRole.Code.Security;

namespace CerebelloWebRole.Code.Filters
{
    public class AuthenticationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            // if the user is in the "app" area, then he/she can only pass when authenticated
            var dataTokens = filterContext.RouteData.DataTokens;
            var httpContext = filterContext.HttpContext;

            if (!dataTokens.ContainsKey("area") || dataTokens["area"].ToString().ToLower() != "app")
                return;

            if (httpContext.Request.IsAuthenticated)
            {
                var authenticatedPrincipal = httpContext.User as AuthenticatedPrincipal;

                if (authenticatedPrincipal == null)
                    throw new Exception("HttpContext.User should be a AuthenticatedPrincipal"
                                        + " when the user is authenticated");
            }
            else
            {
                // if it's an Ajax Request, must return a Json
                if (httpContext.Request.IsAjaxRequest())
                {
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
