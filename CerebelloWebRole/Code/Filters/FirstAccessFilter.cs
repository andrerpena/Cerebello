using System.Net;
using System.Web.Mvc;

namespace CerebelloWebRole.Code
{
    // todo: this should be an IActionFilter instead of an IAuthorizationFilter
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
                        // https://www.cerebello.com.br/p/consultoriodrhouse/users/changepassword
                        // All other places in the software will redirect the user back to that
                        // place until he/she sets the password.
                        bool isUsingDefaultPwd = authenticatedPrincipal.Profile.IsUsingDefaultPassword;

                        bool isRedirectNeeded = string.Format("{0}", filterContext.RouteData.DataTokens["area"]).ToLowerInvariant() != "app"
                            || string.Format("{0}", filterContext.RouteData.Values["controller"]).ToLowerInvariant() != "users"
                            || string.Format("{0}", filterContext.RouteData.Values["action"]).ToLowerInvariant() != "changepassword";

                        if (isUsingDefaultPwd && isRedirectNeeded)
                        {
                            // todo: check for json request: Request.AcceptTypes.Contains("application/json")
                            // if it's an Ajax Request, must return a Json
                            if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
                            {
                                // todo: shouldn't Response.StatusCode be Unauthorized instead of Forbidden
                                // todo: replace this code by JsonForbiddenResult or JsonUnauthorizedResult

                                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                filterContext.Result = new JsonResult
                                {
                                    Data = new
                                    {
                                        error = true,
                                        errorType = "unauthorized",
                                        errorMessage = "The current user is not authorized to access the resource because his/her password has not been reset yet"
                                    },
                                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                                };
                            }
                            else
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
}