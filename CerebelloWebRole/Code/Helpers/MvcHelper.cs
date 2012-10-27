using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
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
        public static Attribute[] GetAttributesOfAction(
            this ControllerContext @this,
            [AspMvcAction]string action = null,
            [AspMvcController]string controller = null,
            string method = "GET")
        {
            // TODO: must cache all of these informations

            if (@this == null)
                throw new ArgumentNullException("this");

            var actionName = action
                ?? @this.RouteData.GetRequiredString("action");

            var controllerName = controller
                ?? @this.RouteData.GetRequiredString("controller");

            const bool testable = true;
            var controllerFactory =
                testable
                ? ((ControllerBuilder)typeof(MvcHandler)
                    .GetProperty("ControllerBuilder", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(@this.HttpContext.Handler))
                    .GetControllerFactory()
                : ControllerBuilder.Current.GetControllerFactory();

            var otherController = (ControllerBase)controllerFactory
                .CreateController(
                    new RequestContext(@this.HttpContext, new RouteData()),
                    controllerName);

            var controllerType = otherController.GetType();

            var controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            //var httpContext = @this.HttpContext.ApplicationInstance.Context;
            var controllerContextWithMethodParam = new ControllerContext(
                new MockHttpContext { Request2 = new MockHttpRequest { HttpMethod2 = method } },
                new RouteData(),
                otherController);

            var actionDescriptor = controllerDescriptor
                .FindAction(controllerContextWithMethodParam, actionName);

            var attributes = actionDescriptor.GetCustomAttributes(true)
                .Cast<Attribute>()
                .ToArray();

            return attributes;
        }

        public class MockHttpContext : HttpContextBase
        {
            public HttpRequestBase Request2 { get; set; }
            public override HttpRequestBase Request { get { return this.Request2; } }
        }

        public class MockHttpRequest : HttpRequestBase
        {
            public string HttpMethod2 { get; set; }
            public override string HttpMethod { get { return this.HttpMethod2; } }

            public override NameValueCollection Headers { get { return new NameValueCollection(); } }
            public override NameValueCollection Form { get { return new NameValueCollection(); } }
            public override NameValueCollection QueryString { get { return new NameValueCollection(); } }
        }
    }
}
