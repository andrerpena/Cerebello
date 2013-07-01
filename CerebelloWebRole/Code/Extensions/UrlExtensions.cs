using System;
using System.Web.Mvc;
using JetBrains.Annotations;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Extensions for the UrlHelper class.
    /// </summary>
    public static class UrlExtensions
    {
        public static string ActionAbsolute(this UrlHelper @this, [AspMvcAction] string actionName, [AspMvcController] string controllerName, object routeValues)
        {
            var isHttps = !DebugConfig.IsDebug || @this.RequestContext.HttpContext.Request.IsSecureConnection;
            var uriBuilder = new UriBuilder(@this.Action(actionName, controllerName, routeValues, isHttps ? "https" : "http"));
            ChangeUrlParams(uriBuilder);
            return uriBuilder.ToString();
        }

        public static string ActionAbsolute(this UrlHelper @this, [AspMvcAction] string actionName, object routeValues)
        {
            var isHttps = !DebugConfig.IsDebug || @this.RequestContext.HttpContext.Request.IsSecureConnection;
            var uriBuilder = new UriBuilder(@this.Action(actionName, "" + @this.RequestContext.RouteData.Values["controller"], routeValues, isHttps ? "https" : "http"));
            ChangeUrlParams(uriBuilder);
            return uriBuilder.ToString();
        }

        public static string ActionAbsolute(this UrlHelper @this, [AspMvcAction] string actionName, [AspMvcController] string controllerName)
        {
            var isHttps = !DebugConfig.IsDebug || @this.RequestContext.HttpContext.Request.IsSecureConnection;
            var uriBuilder = new UriBuilder(@this.Action(actionName, controllerName, new { }, isHttps ? "https" : "http"));
            ChangeUrlParams(uriBuilder);
            return uriBuilder.ToString();
        }

        public static string ActionAbsolute(this UrlHelper @this, [AspMvcAction] string actionName)
        {
            var isHttps = !DebugConfig.IsDebug || @this.RequestContext.HttpContext.Request.IsSecureConnection;
            var uriBuilder = new UriBuilder(@this.Action(actionName, "" + @this.RequestContext.RouteData.Values["controller"], new { }, isHttps ? "https" : "http"));
            ChangeUrlParams(uriBuilder);
            return uriBuilder.ToString();
        }

        private static void ChangeUrlParams(UriBuilder uriBuilder)
        {
#if DEBUG
            if (!DebugConfig.NoHttps)
            {
                if (DebugConfig.HostEnvironment == HostEnv.IisExpress)
                {
                    uriBuilder.Scheme = "https";
                    uriBuilder.Port = 44300;
                }
                else if (DebugConfig.HostEnvironment == HostEnv.Iis)
                {
                    uriBuilder.Scheme = "https";
                    uriBuilder.Port = RoleEnvironment.IsAvailable
                        ? RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["HttpsIn"].IPEndpoint.Port
                        : 443;
                }

                // WebDev server does not support HTTPS... no redirects to HTTPS will happen.
            }
            else
            {
                if (DebugConfig.HostEnvironment == HostEnv.IisExpress)
                {
                    uriBuilder.Scheme = "http";
                    uriBuilder.Port = 12621;
                }
                else if (DebugConfig.HostEnvironment == HostEnv.Iis)
                {
                    uriBuilder.Scheme = "http";
                    uriBuilder.Port = RoleEnvironment.IsAvailable
                        ? RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["HttpIn"].IPEndpoint.Port
                        : 80;
                }
            }
#endif
        }
    }
}