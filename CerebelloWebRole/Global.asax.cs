using System;
using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CerebelloWebRole.Code;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using SimpleInjector;
using SimpleInjector.Integration.Web.Mvc;

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
            // Trace listeners should always be the first thing here.
            RegisterTraceListeners(Trace.Listeners);

            Trace.TraceInformation(string.Format("MvcApplication.Application_Start(): app started! [Debug={0}]", DebugConfig.IsDebug));

            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);

            RouteTable.Routes.MapHubs();
            RegisterRoutes(RouteTable.Routes);

            ModelBinders.Binders.DefaultBinder = new DefaultDictionaryBinder();

            DefaultModelBinder.ResourceClassKey = "ModelStrings";

            // Will create a thread to send notifications
            NotificationsHelper.CreateNotificationsJob();

            SetupDependencyInjector();
        }

        private static void SetupDependencyInjector()
        {
            // 1. Create a new Simple Injector container
            var container = new Container();

            // 2. Configure the container (register)
            container.Register(() => new DateTimeService() as IDateTimeService);
            container.Register(() => DebugConfig.IsDebug ? new LocalStorageService() : new AzureStorageService() as IStorageService);

            // 3. Optionally verify the container's configuration.
            container.Verify();

            // 4. Register the container as MVC3 IDependencyResolver.
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
        }

        /// <summary>
        /// Registers trace listeners to be used by the application.
        /// Generally web.config is used to do this, but there are special cases.
        /// </summary>
        /// <param name="traceListenerCollection">
        /// The trace Listener Collection to register trace listeners at.
        /// </param>
        public static void RegisterTraceListeners(TraceListenerCollection traceListenerCollection)
        {
            // This replaces web.config setting: configuration\system.diagnostics\trace\listeners\add type="Microsoft.WindowsAzure.Diagnostics.DiagnosticMonitorTraceListener, Microsoft.WindowsAzure.Diagnostics, Version=1.8.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" name="AzureDiagnostics"
            // This is done because DiagnosticMonitorTraceListener class throws exception when not running in azure/devfabric.
            if (RoleEnvironment.IsAvailable)
            {
                // See 'diagnostics.wadcfg' file. It contains all configurations of the DiagnosticMonitor.
                // It is not needed to configure nor start the DiagnosticMonitor manually in code.
                // It will be started automatically and will use settings in the 'diagnostics.wadcfg' file.
                // All we need to do is to add the trace listener.
                // reference: http://www.windowsazure.com/en-us/develop/net/common-tasks/diagnostics/
                // google: https://www.google.com/search?q=diagnostics.wadcfg
                traceListenerCollection.Add(new DiagnosticMonitorTraceListener());
            }
        }

        void Application_Error(object sender, EventArgs e)
        {
            // Get the exception object.
            var exc = this.Server.GetLastError();
            Trace.TraceError("MvcApplication.Application_Error(sender, e): Unhandled exception in an HTTP request. Ex: " + exc.Message);
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