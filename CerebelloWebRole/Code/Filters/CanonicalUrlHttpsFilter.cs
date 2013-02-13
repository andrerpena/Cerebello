using System;
using System.Configuration;
using System.Web.Mvc;

namespace CerebelloWebRole.Code.Filters
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
                if (Environment.CommandLine.Contains("iisexpress.exe"))
                {
                    isRedirectNeeded = true;
                    uriBuilder.Scheme = "https";
                    uriBuilder.Port = 44300;
                }
                else if (Environment.CommandLine.Contains("w3wp.exe"))
                {
                    isRedirectNeeded = true;
                    uriBuilder.Scheme = "https";
                    uriBuilder.Port = 443;
                }
                // WebDev server does not support HTTPS... no redirects to HTTPS will happen.
#else
                isRedirectNeeded = true;
                uriBuilder.Scheme = "https";
                uriBuilder.Port = 443;
#endif
            }

            if (isRedirectNeeded)
                filterContext.Result = new RedirectResult(uriBuilder.ToString());
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }
}