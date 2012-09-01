using System;
using System.Web.Mvc;
using CerebelloWebRole.Code.Security;

namespace CerebelloWebRole.Code.Filters
{
    public class AuthenticationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            // if the user is in the "app" area, then he/she can only pass when authenticated
            if (filterContext.RouteData.DataTokens.ContainsKey("area") && filterContext.RouteData.DataTokens["area"].ToString().ToLower() == "app")
            {
                if (filterContext.HttpContext.Request.IsAuthenticated)
                {
                    var authenticatedPrincipal = filterContext.HttpContext.User as AuthenticatedPrincipal;

                    if (authenticatedPrincipal == null)
                        throw new Exception("HttpContext.User should be a AuthenticatedPrincipal when the user is authenticated");
                }
                else
                {
                    // if the user is in the "app" area but is not authenticated, he/she will be redirected to the login page
                    filterContext.Result = new HttpUnauthorizedResult();
                }
            }
        }
    }
}