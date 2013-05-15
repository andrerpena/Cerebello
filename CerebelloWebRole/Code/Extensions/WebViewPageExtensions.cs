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
    /// <summary>
    /// Extension methods to the view page that allows changin the view
    /// accordin to access control rules, and other route informations.
    /// </summary>
    public static class WebViewPageExtensions
    {
        /// <summary>
        /// Checks whether the current user can access the specified action.
        /// At this moment it looks only at PermissionAttribute attributes.
        /// </summary>
        /// <param name="this">The current view page.</param>
        /// <param name="action">Action name to test.</param>
        /// <param name="controller">Controller name to test.</param>
        /// <param name="method">Http method to differentiate GET, HEAD, POST, PUT and DELETE actions.</param>
        /// <param name="routeValues">An object containing the route values for the action. </param>
        /// <returns>Returns true if the current user has access to the given action; otherwise false. </returns>
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

            var routeValuesDic = new RouteValueDictionary(routeValues);
            var mvcHelper = new MvcActionHelper(
                @this.ViewContext.Controller.ControllerContext, action, controller, method, routeValuesDic);

            if (mvcHelper.ActionDescriptor == null)
            {
                // The view does not exist... this means that nobody can access it.
                return false;
            }

            if (routeValues != null)
            {
                // checking action parameters
                var actionParams = mvcHelper.ActionDescriptor.GetParameters();

                // todo: check routeValuesDic to see if the contained values fit the actionParams
                // todo: maybe we should try to bind values (it could be slow)
            }

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
        /// <param name="this">The current view page.</param>
        /// <param name="action">Action name to test.</param>
        /// <param name="controller">Controller name to test.</param>
        /// <returns>Returns true if the current view represents the result of the given action; otherwise false.</returns>
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
                return string.Equals(currentController, controller, StringComparison.OrdinalIgnoreCase)
                       && string.Equals(currentAction, action, StringComparison.OrdinalIgnoreCase);

            return string.Equals(currentAction, action, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks whether the current view is the result of the specified controller call.
        /// </summary>
        /// <param name="this">The current view page.</param>
        /// <param name="controllerNames">Names of the controllers to test.</param>
        /// <returns>Returns true if the current view represents the result of any of the given controller; otherwise false.</returns>
        public static bool IsController(
            this WebViewPage @this,
            [AspMvcController]params string[] controllerNames)
        {
            if (@this == null)
                throw new ArgumentNullException("this");

            var routeData = @this.ViewContext.RouteData;
            var currentController = routeData.GetRequiredString("controller");

            return controllerNames.Any(cn => string.Equals(currentController, cn, StringComparison.OrdinalIgnoreCase));
        }

        public static bool CanAlternateBetweenUsers(
            this WebViewPage @this,
            [AspMvcAction]string action = null,
            [AspMvcController]string controller = null,
            string method = "GET",
            object routeValues = null)
        {
            // TODO: must cache all of these informations
            // todo: this method is similar to CanAccessAction... maybe we can merge them somehow

            if (@this == null)
                throw new ArgumentNullException("this");

            var routeValuesDic = new RouteValueDictionary(routeValues);
            var mvcHelper = new MvcActionHelper(
                @this.ViewContext.Controller.ControllerContext, action, controller, method, routeValuesDic);

            if (mvcHelper.ActionDescriptor == null)
            {
                // The view does not exist... this means that nobody can access it.
                return false;
            }

            var attributes = mvcHelper
                    .GetFilters()
                    .Select(f => f.Instance)
                    .OfType<CanAlternateUserAttribute>()
                    .ToArray();

            var result = attributes.Length > 0;

            return result;
        }
    }
}