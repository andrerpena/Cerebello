using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Notifications;

namespace Cerebello
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CanonicalUrlHttpsFilter());
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AuthenticationFilter());
            filters.Add(new FirstAccessFilter());
            filters.Add(new ValidateInputAttribute(false));
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "home", action = "index", id = UrlParameter.Optional });
        }

        protected void Application_Start()
        {
            RegisterTraceListeners();

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);

            RouteTable.Routes.MapHubs();
            RegisterRoutes(RouteTable.Routes);

            DefaultModelBinder.ResourceClassKey = "ModelStrings";

            // Will create a thread to send notifications
            NotificationsHelper.CreateNotificationsJob();
        }

        private static void RegisterTraceListeners()
        {
            // This replaces web.config setting: configuration\system.diagnostics\trace\listeners\add type="Microsoft.WindowsAzure.Diagnostics.DiagnosticMonitorTraceListener, Microsoft.WindowsAzure.Diagnostics, Version=1.8.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" name="AzureDiagnostics"
            // This is done because DiagnosticMonitorTraceListener class throws exception when not running in azure/devfabric.
            try
            {
               var azureTraceListener = new Microsoft.WindowsAzure.Diagnostics.DiagnosticMonitorTraceListener();
                Trace.Listeners.Add(azureTraceListener);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
        }

        protected void Application_AuthenticateRequest()
        {
            var httpContext = new HttpContextWrapper(this.Context);
            SecurityManager.SetPrincipal(httpContext);
        }
    }
}