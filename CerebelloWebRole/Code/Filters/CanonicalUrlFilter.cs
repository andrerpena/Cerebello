using System;
using System.Configuration;
using System.Web.Mvc;

namespace CerebelloWebRole.Code.Filters
{
    public class CanonicalUrlFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.RequestContext.HttpContext.Request;
            if (!request.IsAjaxRequest() && request.Url != null && !request.Url.IsLoopback && request.Url.Host != ConfigurationManager.AppSettings["CanonicalUrlHost"])
            {
                var uriBuilder = new UriBuilder(request.Url) { Host = ConfigurationManager.AppSettings["CanonicalUrlHost"] };
                filterContext.Result = new RedirectResult(uriBuilder.ToString());
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }
}