using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Web.Mvc;
using Cerebello.Model;

namespace CerebelloWebRole.Code.Filters
{
    /// <summary>
    /// Base authorization filter used to allow or deny a loged user to access a specific protected resource.
    /// This is intended to be used as as attribute, but it can be used as a generic IAuthorizationFilter as well.
    /// </summary>
    public abstract class PermissionAttribute : FilterAttribute, IAuthorizationFilter
    {
        // reference:
        // if someday we have problems with caching restricted-access pages, the following could be useful:
        // http://farm-fresh-code.blogspot.com.br/2009/11/customizing-authorization-in-aspnet-mvc.html

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.Result == null)
            {
                var cerebelloController = filterContext.Controller as CerebelloController;

                if (cerebelloController == null)
                    throw new Exception("The PermissionAttribute can only be used on actions of CerebelloController inheritors.");

                cerebelloController.InitDb();
                cerebelloController.InitDbUser(filterContext.RequestContext);

                Debug.Assert(cerebelloController.DbUser != null, "cerebelloController.DbUser must not be null");
                bool canAccess = this.CanAccessResource(
                    new PermissionContext
                        {
                            User = cerebelloController.DbUser,
                            ControllerContext = filterContext.Controller.ControllerContext
                        });

                if (!canAccess)
                {
                    filterContext.Result = new UnauthorizedResult(
                        "The current user is not authorized to access the resource because it hasn't got permission.");
                }
            }
        }

        public abstract bool CanAccessResource(PermissionContext permissionContext);

        public Type StatusDescriptionResourceType { get; set; }
        public string StatusDescriptionResourceName { get; set; }

        [Localizable(true)] // must be [Localizable(true)]
        [JetBrains.Annotations.LocalizationRequired(true)]
        public string StatusDescription { get; set; }
    }

    public class PermissionContext
    {
        public ControllerContext ControllerContext { get; set; }
        public User User { get; set; }
    }
}
