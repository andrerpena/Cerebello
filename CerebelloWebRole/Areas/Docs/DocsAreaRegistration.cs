using System.Web.Mvc;

namespace CerebelloWebRole.Areas.Docs
{
    public class DocsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Docs";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Docs_default",
                "docs/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
