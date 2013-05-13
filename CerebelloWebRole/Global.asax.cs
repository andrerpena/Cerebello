using System;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Notifications;
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
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
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
            //RegisterTraceListeners(Trace.Listeners);
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
            // Code that runs when an unhandled error occurs

            // Get the exception object.
            Exception exc = Server.GetLastError();
            Trace.TraceError("Unhandled exception in an HTTP request. Ex: " + exc.Message);
        }

        ///// <summary>
        ///// Registers trace listeners to be used by the application.
        ///// Generally web.config is used to do this, but there are special cases.
        ///// </summary>
        ///// <param name="traceListenerCollection">
        ///// The trace Listener Collection to register trace listeners at.
        ///// </param>
        //private static void RegisterTraceListeners(TraceListenerCollection traceListenerCollection)
        //{
        //    // This replaces web.config setting: configuration\system.diagnostics\trace\listeners\add type="Microsoft.WindowsAzure.Diagnostics.DiagnosticMonitorTraceListener, Microsoft.WindowsAzure.Diagnostics, Version=1.8.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" name="AzureDiagnostics"
        //    // This is done because DiagnosticMonitorTraceListener class throws exception when not running in azure/devfabric.
        //    if (RoleEnvironment.IsAvailable)
        //    {
        //        var azureTraceListener = new Microsoft.WindowsAzure.Diagnostics.DiagnosticMonitorTraceListener();
        //        traceListenerCollection.Add(azureTraceListener);
        //    }
        //}

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