using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CerebelloWebRole.Code.Filters
{
    public class CanonicalUrlFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.RequestContext.HttpContext.Request;
            if (!request.IsAjaxRequest() && request.Url.Host != "localhost" && request.Url.Host != ConfigurationManager.AppSettings["CanonicalUrlHost"])
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