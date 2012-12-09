using System.Web.Mvc;

namespace CerebelloWebRole.Areas.App
{
    public class AppAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "App";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Doctor",
                "p/{practice}/d/{doctor}/{controller}/{action}/{id}",
                new { controller = "practicehome", action = "Index", id = UrlParameter.Optional },
                new string[] { "CerebelloWebRole.Areas.App.Controllers" }
            );

            context.MapRoute(
                "Practice",
                "p/{practice}/{controller}/{action}/{id}",
                new { controller = "practicehome", action = "Index", id = UrlParameter.Optional },
                new string[] { "CerebelloWebRole.Areas.App.Controllers" }
            );
        }
    }
}
