using System;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Web.Mvc;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code.Helpers
{
    /// <summary>
    /// Class containing ASP.NET MVC utilities, to help with rendering views to strings,
    /// mocking Http classes and getting informations about actions.
    /// </summary>
    public static class MvcHelper
    {
        /// <summary>
        /// Renders a partial MVC view to a string.
        /// The view search locations is relative to the ControllerContext.
        /// </summary>
        /// <param name="controllerContext">ControllerContext that is used to locate the view.</param>
        /// <param name="viewName">The name of the partial view to render.</param>
        /// <param name="viewData">The viewData, containing the model object to pass to the partial view.</param>
        /// <returns>The string rendered from the partial view.</returns>
        public static string RenderPartialViewToString(
            ControllerContext controllerContext,
            [AspMvcPartialView] string viewName,
            ViewDataDictionary viewData = null)
        {
            var tempData = new TempDataDictionary();
            var viewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
            using (var sw = new StringWriter())
            {
                var viewContext = new ViewContext(controllerContext, viewResult.View, viewData, tempData, sw);
                viewResult.View.Render(viewContext, sw);
                return sw.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Renders a partial MVC view to a string.
        /// The view search locations is relative to the ControllerContext.
        /// </summary>
        /// <param name="controllerContext">ControllerContext that is used to locate the view.</param>
        /// <param name="viewName">The name of the partial view to render.</param>
        /// <param name="viewData">The viewData, containing the model object to pass to the partial view.</param>
        /// <param name="masterName">Name of the layout page.</param>
        /// <returns>The string rendered from the partial view.</returns>
        public static string RenderViewToString(
            ControllerContext controllerContext,
            [AspMvcView] string viewName,
            ViewDataDictionary viewData = null,
            [AspMvcMaster]string masterName = "")
        {
            var tempData = new TempDataDictionary();
            var viewResult = ViewEngines.Engines.FindView(controllerContext, viewName, masterName);
            using (var sw = new StringWriter())
            {
                var viewContext = new ViewContext(controllerContext, viewResult.View, viewData, tempData, sw);
                viewResult.View.Render(viewContext, sw);
                return sw.GetStringBuilder().ToString();
            }
        }

        public class MockHttpContext : HttpContextBase
        {
            private readonly HttpResponseBase response = new MockHttpResponse();

            public HttpRequestBase Request2 { get; set; }

            public override HttpRequestBase Request
            {
                get { return this.Request2; }
            }

            public override HttpResponseBase Response
            {
                get { return this.response; }
            }
        }

        public class MockHttpRequest : HttpRequestBase
        {
            private readonly Uri url = GetUrl();

            private static Uri GetUrl()
            {
#if DEBUG
                if (DebugConfig.IsDebug)
                {
                    var uriBuilder = new UriBuilder("https://localhost");
                    if (DebugConfig.HostEnvironment == HostEnv.IisExpress)
                    {
                        uriBuilder.Port = 44300;
                    }
                    else if (DebugConfig.HostEnvironment == HostEnv.WebDevServer)
                    {
                        uriBuilder.Scheme = "http";
                        uriBuilder.Port = 12621;
                    }
                    else
                    {
                    }

                    return uriBuilder.Uri;
                }
#endif
                return new Uri(string.Format("https://{0}", Constants.DOMAIN), UriKind.Absolute);
            }

            private readonly HttpCookieCollection cookies = new HttpCookieCollection();
            private readonly NameValueCollection serverVars = new NameValueCollection();
            private readonly NameValueCollection headers = new NameValueCollection();
            private readonly NameValueCollection form = new NameValueCollection();
            private readonly NameValueCollection queryStr = new NameValueCollection();

            private HttpRequestBase httpRequestBase;

            public MockHttpRequest()
            {
            }

            public MockHttpRequest(HttpRequestBase httpRequestBase)
            {
                this.httpRequestBase = httpRequestBase;
            }

            public override string AppRelativeCurrentExecutionFilePath
            {
                get { return this.httpRequestBase.AppRelativeCurrentExecutionFilePath; }
            }

            public override string PathInfo
            {
                get { return this.httpRequestBase.PathInfo; }
            }

            public string HttpMethod2 { get; set; }

            public Uri Url2 { get; set; }

            public override string HttpMethod
            {
                get { return this.HttpMethod2; }
            }

            public override Uri Url
            {
                get { return this.Url2 ?? this.url; }
            }

            public override string ApplicationPath
            {
                get { return "/"; }
            }

            public override HttpCookieCollection Cookies
            {
                get { return this.cookies; }
            }

            public override NameValueCollection ServerVariables
            {
                get { return this.serverVars; }
            }

            public override NameValueCollection Headers
            {
                get { return this.headers; }
            }

            public override NameValueCollection Form
            {
                get { return this.form; }
            }

            public override NameValueCollection QueryString
            {
                get { return this.queryStr; }
            }
        }

        public class MockHttpResponse : HttpResponseBase
        {
            private readonly HttpCookieCollection cookies = new HttpCookieCollection();

            public override string ApplyAppPathModifier(string virtualPath)
            {
                return virtualPath;
            }

            public override HttpCookieCollection Cookies
            {
                get { return this.cookies; }
            }
        }
    }
}
