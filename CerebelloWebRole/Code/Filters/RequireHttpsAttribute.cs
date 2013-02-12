using System.Web.Mvc;

namespace CerebelloWebRole.Code.Filters
{
    public class RequireHttpsPortAttribute : RequireHttpsAttribute
    {
        private readonly int port;

        public RequireHttpsPortAttribute(int port)
        {
            this.port = port;
        }

        protected override void HandleNonHttpsRequest(AuthorizationContext filterContext)
        {
            base.HandleNonHttpsRequest(filterContext);
            if (port != 443)
            {
                var url = "https://" + filterContext.HttpContext.Request.Url.Host + ':' + port + filterContext.HttpContext.Request.RawUrl;
                filterContext.Result = (ActionResult)new RedirectResult(url);
            }
        }
    }
}
