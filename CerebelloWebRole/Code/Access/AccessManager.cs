using System;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Helpers;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code
{
    public static class AccessManager
    {
        /// <summary>
        /// Finds out whether user can access the specified action.
        /// At this moment it looks only at PermissionAttribute attributes.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="user"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static bool CanAccessAction(
            this ControllerContext @this,
            User user,
            [AspMvcAction]string action = null,
            [AspMvcController]string controller = null,
            string method = "GET")
        {
            // TODO: must cache all of these informations

            // TODO: there is much to be improved in this method:
            // - Use global filters
            // - Use the controller itself as a filter
            // - Use attributes that are filters, not only derived from PermissionAttribute.

            if (@this == null)
                throw new ArgumentNullException("this");

            if (user == null)
                throw new ArgumentNullException("user");

            var attributes = @this.GetAttributesOfAction(action, controller, method)
                .OfType<PermissionAttribute>()
                .ToArray();

            var result = !attributes.Any()
                || attributes.All(pa => pa.CanAccessResource(user));

            return result;
        }
    }
}
