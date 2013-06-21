using System;
using System.Configuration;
using System.Web.Mvc;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace CerebelloWebRole.Code
{
    public class CanonicalUrlHttpsFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.RequestContext.HttpContext.Request;
            var uriBuilder = new UriBuilder(request.Url);

            var isNotCanonical = !request.IsAjaxRequest() && request.Url != null && !request.Url.IsLoopback &&
                                  request.Url.Host != ConfigurationManager.AppSettings["CanonicalUrlHost"];

            var isNotHttps = !request.IsSecureConnection;

            var isRedirectNeeded = false;

            if (isNotCanonical)
            {
                isRedirectNeeded = true;
                uriBuilder.Host = ConfigurationManager.AppSettings["CanonicalUrlHost"];
            }

            if (isNotHttps)
            {
#if DEBUG
                if (!DebugConfig.NoHttps)
                {
                    if (DebugConfig.HostEnvironment == HostEnv.IisExpress)
                    {
                        isRedirectNeeded = true;
                        uriBuilder.Scheme = "https";
                        uriBuilder.Port = 44300;
                    }
                    else if (DebugConfig.HostEnvironment == HostEnv.Iis)
                    {
                        isRedirectNeeded = true;
                        uriBuilder.Scheme = "https";
                        uriBuilder.Port = RoleEnvironment.IsAvailable
                            ? RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["HttpsIn"].IPEndpoint.Port
                            : 443;
                    }

                    // WebDev server does not support HTTPS... no redirects to HTTPS will happen.
                }
#else
                isRedirectNeeded = true;
                uriBuilder.Scheme = "https";
                uriBuilder.Port = 443;
#endif
            }
#if DEBUG
            else if (DebugConfig.NoHttps)
            {
                if (DebugConfig.HostEnvironment == HostEnv.IisExpress)
                {
                    isRedirectNeeded = true;
                    uriBuilder.Scheme = "http";
                    uriBuilder.Port = 12621;
                }
                else if (DebugConfig.HostEnvironment == HostEnv.Iis)
                {
                    isRedirectNeeded = true;
                    uriBuilder.Scheme = "http";
                    uriBuilder.Port = RoleEnvironment.IsAvailable
                        ? RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["HttpIn"].IPEndpoint.Port
                        : 80;
                }
            }
#endif

            if (isRedirectNeeded)
                filterContext.Result = new RedirectResult(uriBuilder.ToString());
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }
}