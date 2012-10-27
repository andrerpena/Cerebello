using System;
using System.Web.Mvc;
using CerebelloWebRole.Code.Access;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code.Extensions
{
    public static class AccessExtensions
    {
        /// <summary>
        /// Checks whether the current user can access an action.
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

                var result = @this.ViewContext.Controller.ControllerContext
                    .CanAccessAction(cerebelloController.DbUser, action, controller, method);

                return result;
            }

            return false;
        }
    }
}