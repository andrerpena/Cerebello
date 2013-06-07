using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Class containing ASP.NET MVC utilities to help with actions.
    /// </summary>
    public class MvcActionHelper
    {
        public MvcActionHelper(
            ControllerContext currentControllerContext,
            [AspMvcAction] string actionName = null,
            [AspMvcController] string controllerName = null,
            string httpMethod = "GET",
            object routeValues = null,
            string protocol = null,
            string hostName = null)
            : this(
                currentControllerContext,
                actionName,
                controllerName,
                httpMethod,
                new RouteValueDictionary(routeValues),
                protocol,
                hostName)
        {
        }

        public MvcActionHelper(
            ControllerContext currentControllerContext,
            [AspMvcAction] string actionName = null,
            [AspMvcController] string controllerName = null,
            string httpMethod = "GET",
            RouteValueDictionary routeValues = null,
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
                routeValues,
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
        /// <returns>Filters that are used when calling the action.</returns>
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
    }
}
