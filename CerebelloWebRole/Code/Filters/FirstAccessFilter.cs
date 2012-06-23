using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code.Security;

namespace CerebelloWebRole.Code.Filters
{
    public class FirstAccessFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.RouteData.DataTokens.ContainsKey("area") && filterContext.RouteData.DataTokens["area"].ToString().ToLower() == "app")
            {
                if (filterContext.HttpContext.Request.IsAuthenticated)
                {
                    var authenticatedPrincipal = filterContext.HttpContext.User as AuthenticatedPrincipal;

                    if (authenticatedPrincipal != null)
                    {
                        // When the user is accessing the software for the first time,
                        // he/she will be asked to set an access password, at this place:
                        // http://www.cerebello.com.br/p/consultoriodrhourse/users/changepassword
                        // All other places in the software will redirect the user back to that
                        // place until he/she sets the password.
                        bool isUsingDefaultPwd = authenticatedPrincipal.Profile.IsUsingDefaultPassword;

                        bool isRedirectNeeded = string.Format("{0}", filterContext.RouteData.DataTokens["area"]).ToLowerInvariant() != "app"
                            || string.Format("{0}", filterContext.RouteData.Values["controller"]).ToLowerInvariant() != "users"
                            || string.Format("{0}", filterContext.RouteData.Values["action"]).ToLowerInvariant() != "changepassword";

                        if (isUsingDefaultPwd && isRedirectNeeded)
                        {
                            filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary
                            {
                                { "area", "app" },
                                { "controller", "users" },
                                { "action", "changepassword" },
                            });
                        }
                    }
                }
            }
        }
    }
}