using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Cerebello;

namespace CerebelloWebRole.Code.Helpers
{
    public static class RouteHelper
    {
        public static RouteData GetRouteDataByUrl(string url)
        {
            return RouteTable.Routes.GetRouteData(new RewritedHttpContextBase(url));
        }

        private class RewritedHttpContextBase : HttpContextBase
        {
            private readonly HttpRequestBase mockHttpRequestBase;

            public RewritedHttpContextBase(string appRelativeUrl)
            {
                this.mockHttpRequestBase = new MockHttpRequestBase(appRelativeUrl);
            }


            public override HttpRequestBase Request
            {
                get
                {
                    return mockHttpRequestBase;
                }
            }

            private class MockHttpRequestBase : HttpRequestBase
            {
                private readonly string appRelativeUrl;

                public MockHttpRequestBase(string appRelativeUrl)
                {
                    this.appRelativeUrl = appRelativeUrl;
                }

                public override string AppRelativeCurrentExecutionFilePath
                {
                    get { return appRelativeUrl; }
                }

                public override string PathInfo
                {
                    get { return ""; }
                }
            }
        }

        private static object lockerRegisterAllRoutes = new object();

        /// <summary>
        /// Register all areas in the assembly of the application.
        /// This is used by tests and by the Worker-Role.
        /// </summary>
        public static void RegisterAllRoutes()
        {
            if (lockerRegisterAllRoutes != null)
                lock (lockerRegisterAllRoutes)
                {
                    lockerRegisterAllRoutes = null;
                    // Registering all routes, from all areas.
                    var allAreas = typeof(MvcApplication).Assembly.GetTypes()
                        .Where(type => typeof(AreaRegistration).IsAssignableFrom(type))
                        .Where(type => type.GetConstructor(Type.EmptyTypes) != null)
                        .Select(type => (AreaRegistration)Activator.CreateInstance(type))
                        .ToArray();

                    foreach (var eachAreaRegistration in allAreas)
                    {
                        eachAreaRegistration.RegisterArea(
                            new AreaRegistrationContext(eachAreaRegistration.AreaName, RouteTable.Routes));
                    }

                    MvcApplication.RegisterRoutes(RouteTable.Routes);
                }
        }
    }
}