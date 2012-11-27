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
    public static class MvcHelper
    {
        /// <summary>
        /// Returns the attributes places on an action method.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        [Obsolete("This method is not being used anymore. 2012-11-11.")]
        public static Attribute[] GetAttributesOfAction(
            this ControllerContext @this,
            [AspMvcAction]string action = null,
            [AspMvcController]string controller = null,
            string method = "GET")
        {
            ControllerContext controllerContextWithMethodParam;
            var actionDescriptor = GetActionDescriptor(@this, action, controller, method, out controllerContextWithMethodParam);

            var attributes = actionDescriptor.GetCustomAttributes(true)
                .Cast<Attribute>()
                .ToArray();

            return attributes;
        }

        /// <summary>
        /// Returns all the filters that are executed when calling an action.
        /// This uses the default Mvc classes used to get the filters,
        /// so the behavior is the same.
        /// This means that the filters are returned in order,
        /// according to Order and Scope values of the filters. 
        /// </summary>
        /// <param name="this"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Filter[] GetFiltersForAction(
            this ControllerContext @this,
            [AspMvcAction]string action = null,
            [AspMvcController]string controller = null,
            string method = "GET")
        {
            ControllerContext controllerContextWithMethodParam;
            var actionDescriptor = GetActionDescriptor(@this, action, controller, method, out controllerContextWithMethodParam);

            // The default Controller.ActionInvoker.GetFilters returns filters from FilterProviders.Providers.GetFilters method.
            // So this method may not be compatible with custom controller implementations that override the ActionInvoker,
            // or override the GetFilters method.
            var filters = FilterProviders.Providers
                .GetFilters(controllerContextWithMethodParam, actionDescriptor)
                .ToArray();

            return filters;
        }

        public static ActionDescriptor GetActionDescriptor(this ControllerContext @this, string action, string controller, string method, out ControllerContext mockControllerContext)
        {
            // TODO: must cache all of these informations, they can become quite expensive if this method is used everywhere.

            if (@this == null)
                throw new ArgumentNullException("this");

            var actionName = action
                             ?? @this.RouteData.GetRequiredString("action");

            var controllerName = controller
                                 ?? @this.RouteData.GetRequiredString("controller");

            // The default MvcHandler.ControllerBuilder returns ControllerBuilder.Current.
            // So this method may not be compatible with other implementations of MvcHandler.
            var controllerFactory = ControllerBuilder.Current.GetControllerFactory();

            var otherController = (ControllerBase)controllerFactory
                .CreateController(
                // note: the area does not affect which controller is selected
                new RequestContext(@this.HttpContext, new RouteData()),
                controllerName);

            var controllerType = otherController.GetType();

            var controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            //var httpContext = @this.HttpContext.ApplicationInstance.Context;
            var controllerContextWithMethodParam = new ControllerContext(
                new MockHttpContext { Request2 = new MockHttpRequest { HttpMethod2 = method } },
                new RouteData(),
                otherController);

            mockControllerContext = controllerContextWithMethodParam;

            var actionDescriptor = controllerDescriptor
                .FindAction(controllerContextWithMethodParam, actionName);

            return actionDescriptor;
        }

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
                    "http://www.{0}{1}/",
                    Constants.DOMAIN,
                    Constants.PORT.HasValue ? ":" + Constants.PORT : ""),
                UriKind.Absolute);

            private readonly HttpCookieCollection cookies = new HttpCookieCollection();
            private readonly NameValueCollection serverVars = new NameValueCollection();
            private readonly NameValueCollection headers = new NameValueCollection();
            private readonly NameValueCollection form = new NameValueCollection();
            private readonly NameValueCollection queryStr = new NameValueCollection();

            public string HttpMethod2 { get; set; }

            public override string HttpMethod
            {
                get { return this.HttpMethod2; }
            }

            public override Uri Url
            {
                get { return this.url; }
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
