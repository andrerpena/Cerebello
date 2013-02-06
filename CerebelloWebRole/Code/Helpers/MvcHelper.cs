using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code.Helpers
{
    public class MvcHelper
    {
        public MvcHelper(
            ControllerContext currentControllerContext,
            [AspMvcAction] string actionName = null,
            [AspMvcController] string controllerName = null,
            string httpMethod = "GET",
            object routeValues = null,
            string protocol = null,
            string hostName = null)
        {
            this.CurrentControllerContext = currentControllerContext;

            this.ActionName = actionName ?? this.CurrentControllerContext.RouteData.GetRequiredString("action");
            this.ControllerName = controllerName ?? this.CurrentControllerContext.RouteData.GetRequiredString("controller");

            this.HttpMethod = httpMethod;

            var httpContext = new MvcHelper.MockHttpContext
            {
                Request2 =
                    new MvcHelper.MockHttpRequest(this.CurrentControllerContext.HttpContext.Request)
                    {
                        HttpMethod2 = this.HttpMethod,
                        Url2 = this.Uri,
                    }
            };

            // Building route data.
            var urlHelper = new UrlHelper(this.CurrentControllerContext.RequestContext);
            var currentUri = this.CurrentControllerContext.RequestContext.HttpContext.Request.Url;
            this.Uri = new Uri(urlHelper.Action(
                this.ActionName,
                this.ControllerName,
                new RouteValueDictionary(routeValues),
                protocol ?? currentUri.Scheme,
                hostName ?? currentUri.Host));

            var routeData = RouteTable.Routes.GetRouteData(httpContext);

            // Creating controller.
            this.Controller = (ControllerBase)this.ControllerFactory
                .CreateController(
                // note: the area does not affect which controller is selected
                new RequestContext(httpContext, routeData),
                this.ControllerName);

            this.ControllerType = this.Controller.GetType();

            this.ControllerDescriptor = new ReflectedControllerDescriptor(this.ControllerType);

            // Creating fake controller context.
            this.MockControllerContext = new ControllerContext(
                httpContext,
                routeData,
                this.Controller);

            this.Controller.ControllerContext = this.MockControllerContext;

            this.ActionDescriptor = this.ControllerDescriptor
                .FindAction(this.MockControllerContext, this.ActionName);
        }

        /// <summary>
        /// Returns all the filters that are executed when calling an action.
        /// This uses the default Mvc classes used to get the filters,
        /// so the behavior is the same.
        /// This means that the filters are returned in order,
        /// according to Order and Scope values of the filters. 
        /// </summary>
        /// <returns></returns>
        public Filter[] GetFilters()
        {
            var actionDescriptor = this.ActionDescriptor;

            // The default Controller.ActionInvoker.GetFilters returns filters from FilterProviders.Providers.GetFilters method.
            // So this method may not be compatible with custom controller implementations that override the ActionInvoker,
            // or override the GetFilters method.
            var filters = FilterProviders.Providers
                .GetFilters(this.MockControllerContext, actionDescriptor)
                .ToArray();

            return filters;
        }

        public IControllerFactory ControllerFactory
        {
            get
            {
                // The default MvcHandler.ControllerBuilder returns ControllerBuilder.Current.
                // So this may not be compatible with other implementations of MvcHandler.
                return ControllerBuilder.Current.GetControllerFactory();
            }
        }

        public ControllerContext CurrentControllerContext { get; set; }

        public string ActionName { get; private set; }

        public string ControllerName { get; private set; }

        public string HttpMethod { get; set; }

        public ControllerBase Controller { get; private set; }

        public ActionDescriptor ActionDescriptor { get; private set; }

        public ControllerContext MockControllerContext { get; private set; }

        public Type ControllerType { get; private set; }

        public ReflectedControllerDescriptor ControllerDescriptor { get; private set; }

        public Uri Uri { get; private set; }
    
        public class MockHttpContext : HttpContextBase
        {
            private readonly HttpResponseBase response = new MockHttpResponse();
            public HttpRequestBase Request2 { get; set; }
            public override HttpRequestBase Request { get { return this.Request2; } }

            public override HttpResponseBase Response
            {
                get { return this.response; }
            }
        }

        public class MockHttpRequest : HttpRequestBase
        {
            private readonly Uri url = new Uri(
                String.Format(
                    "https://www.{0}{1}/",
                    Constants.DOMAIN,
                    Constants.PORT.HasValue ? ":" + Constants.PORT : ""),
                UriKind.Absolute);

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

        /// <summary>
        /// Renders a partial MVC view to a string.
        /// The view search locations is relative to the ControllerContext.
        /// </summary>
        /// <param name="controllerContext">ControllerContext that is used to locate the view.</param>
        /// <param name="viewName">The name of the partial view to render.</param>
        /// <param name="model">The model objeto to pass to the partial view.</param>
        /// <returns>The string rendered from the partial view.</returns>
        public static string RenderPartialViewToString(
            ControllerContext controllerContext,
            [JetBrains.Annotations.AspMvcView][JetBrains.Annotations.AspMvcPartialView] string viewName,
            object model = null)
        {
            var viewData = new ViewDataDictionary(model);
            var tempData = new TempDataDictionary();
            var viewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
            using (var sw = new StringWriter())
            {
                var viewContext = new ViewContext(controllerContext, viewResult.View, viewData, tempData, sw);
                viewResult.View.Render(viewContext, sw);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
