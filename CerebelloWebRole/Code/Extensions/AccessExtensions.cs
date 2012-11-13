using System;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Helpers;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code.Extensions
{
    public static class AccessExtensions
    {
        /// <summary>
        /// Checks whether the current user can access the specified action.
        /// At this moment it looks only at PermissionAttribute attributes.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static bool CanAccessAction(
            this WebViewPage @this,
            [AspMvcAction]string action = null,
            [AspMvcController]string controller = null,
            string method = "GET")
        {
            // TODO: must cache all of these informations

            if (@this == null)
                throw new ArgumentNullException("this");

            var cerebelloController = @this.ViewContext.Controller as CerebelloController;

            if (cerebelloController != null)
            {
                cerebelloController.InitDb();
                cerebelloController.InitDbUser(@this.Request.RequestContext);

                var attributes = @this.ViewContext.Controller.ControllerContext
                    .GetFiltersForAction(action, controller, method)
                    .Select(f => f.Instance)
                    .OfType<PermissionAttribute>()
                    .ToArray();

                var result = !attributes.Any()
                    || attributes.All(pa => pa.CanAccessResource(cerebelloController.DbUser));

                return result;
            }

            return false;
        }

        /// <summary>
        /// Checks whether the current view is the result of the specified action.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static bool IsAction(
            this WebViewPage @this,
            [AspMvcAction]string action = null,
            [AspMvcController]string controller = null,
            string method = null)
        {
            if (@this == null)
                throw new ArgumentNullException("this");

            if (method == null)
            {
                var routeData = @this.ViewContext.RouteData;
                var currentAction = routeData.GetRequiredString("action");
                var currentController = routeData.GetRequiredString("controller");

                if (controller != null)
                    return string.Compare(currentController, controller, true) == 0
                           && string.Compare(currentAction, action, true) == 0;

                return string.Compare(currentAction, action, true) == 0;
            }

            ControllerContext controllerContextWithMethodParam;
            var actionDescriptor = @this.ViewContext.Controller.ControllerContext
                .GetActionDescriptor(action, controller, method, out controllerContextWithMethodParam);

            var controllerDescriptor = new ReflectedControllerDescriptor(@this.ViewContext.Controller.GetType());
            var actionDescriptor2 = controllerDescriptor.FindAction(@this.ViewContext.Controller.ControllerContext, action);

            if (actionDescriptor == null || actionDescriptor2 == null)
                return false;

            return actionDescriptor.UniqueId == actionDescriptor2.UniqueId;
        }
    }
}