using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
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
            string method = "GET",
            object routeValues = null)
        {
            // TODO: must cache all of these informations

            if (@this == null)
                throw new ArgumentNullException("this");

            var mvcHelper = new MvcHelper(@this.ViewContext.Controller.ControllerContext, action, controller, method, routeValues);

            // Getting the current DB User... (the logged user).
            var cerebelloController = @this.ViewContext.Controller as CerebelloController;
            User dbUser = null;
            if (cerebelloController != null)
            {
                cerebelloController.InitDb();
                cerebelloController.InitDbUser(@this.Request.RequestContext);
                dbUser = cerebelloController.DbUser;
            }

            // If there is a logged user, then use permission attributes to determine whether user has access or not.
            if (dbUser != null)
            {
                var attributes = mvcHelper
                        .GetFilters()
                        .Select(f => f.Instance)
                        .OfType<PermissionAttribute>()
                        .ToArray();

                var permissionContext = new PermissionContext
                    {
                        User = dbUser,
                        ControllerContext = mvcHelper.MockControllerContext,
                    };

                var result = !attributes.Any()
                             || attributes.All(pa => pa.CanAccessResource(permissionContext));

                return result;
            }

            return false;
        }

        /// <summary>
        /// Checks whether the current view is the result of the specified action call.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static bool IsAction(
            this WebViewPage @this,
            [AspMvcAction]string action,
            [AspMvcController]string controller = null)
        {
            if (@this == null)
                throw new ArgumentNullException("this");

            var routeData = @this.ViewContext.RouteData;
            var currentAction = routeData.GetRequiredString("action");
            var currentController = routeData.GetRequiredString("controller");

            if (controller != null)
                return string.Compare(currentController, controller, true) == 0
                       && string.Compare(currentAction, action, true) == 0;

            return string.Compare(currentAction, action, true) == 0;
        }

        /// <summary>
        /// Checks whether the current view is the result of the specified controller call.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="controllerNames"></param>
        /// <returns></returns>
        public static bool IsController(
            this WebViewPage @this,
            [AspMvcController]params string[] controllerNames)
        {
            if (@this == null)
                throw new ArgumentNullException("this");

            var routeData = @this.ViewContext.RouteData;
            var currentController = routeData.GetRequiredString("controller");

            return controllerNames.Any(cn => String.Compare(currentController, cn, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}