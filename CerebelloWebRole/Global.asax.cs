using System;
using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Notifications;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace CerebelloWebRole
{
    /// <summary>
    /// Cerebello http application class.
    /// </summary>
    public class MvcApplication : HttpApplication
    {
        /// <summary>
        /// Registers global filters.
        /// </summary>
        /// <param name="filters">Filters collection to register the global filters at.</param>
        private static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CanonicalUrlHttpsFilter());
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AuthenticationFilter());
            filters.Add(new FirstAccessFilter());
            filters.Add(new ValidateInputAttribute(false));
        }

        /// <summary>
        /// Registers global routes.
        /// </summary>
        /// <param name="routes">Routes collection to registers routes at.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "home", action = "index", id = UrlParameter.Optional });
        }

        /// <summary>
        /// Runs when the web application is started.
        /// </summary>
        protected void Application_Start()
        {
            var config = DiagnosticMonitor.GetDefaultInitialConfiguration();
            // Set an overall quota of 8GB.
            config.OverallQuotaInMB = 4096;
            // Set the sub-quotas and make sure it is less than the OverallQuotaInMB set above
            config.Logs.BufferQuotaInMB = 512;

            var myTimeSpan = TimeSpan.FromMinutes(2);
            config.Logs.ScheduledTransferPeriod = myTimeSpan;//Transfer data to storage every 2 minutes

            // Filter what will be sent to persistent storage.
            config.Logs.ScheduledTransferLogLevelFilter = LogLevel.Undefined;//Transfer everything
            // Apply the updated configuration to the diagnostic monitor.
            // The first parameter is for the connection string configuration setting.
            DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", config);
            Trace.Listeners.Add(new DiagnosticMonitorTraceListener());


            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);

            RouteTable.Routes.MapHubs();
            RegisterRoutes(RouteTable.Routes);

            DefaultModelBinder.ResourceClassKey = "ModelStrings";

            // Will create a thread to send notifications
            NotificationsHelper.CreateNotificationsJob();
        }

        void Application_Error(object sender, EventArgs e)
        {
            // Get the exception object.
            var exc = this.Server.GetLastError();
            Trace.TraceError("Unhandled exception in an HTTP request. Ex: " + exc.Message);
        }

        /// <summary>
        /// Authenticates the current request.
        /// </summary>
        protected void Application_AuthenticateRequest()
        {
            var httpContext = new HttpContextWrapper(this.Context);
            SecurityManager.SetPrincipal(httpContext);
        }
    }
}