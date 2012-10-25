using System;
using System.Diagnostics;
using System.Web.Mvc;
using Cerebello.Model;

namespace CerebelloWebRole.Code.Filters
{
    public abstract class PermissionAttribute : AuthorizeAttribute
    {
        // reference:
        // http://farm-fresh-code.blogspot.com.br/2009/11/customizing-authorization-in-aspnet-mvc.html

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            if (filterContext.Result == null)
            {
                var cerebelloController = filterContext.Controller as CerebelloController;

                if (cerebelloController != null)
                {
                    cerebelloController.InitDb();
                    cerebelloController.InitDbUser(filterContext.RequestContext);

                    Debug.Assert(cerebelloController.DbUser != null, "cerebelloController.DbUser must not be null");
                    bool canAccess = this.CanAccessResource(cerebelloController.DbUser);

                    if (!canAccess)
                        filterContext.Result = new HttpUnauthorizedResult();
                }
                else
                    throw new Exception("The PermissionAttribute cannot be used on actions of this controller.");
            }
        }

        public abstract bool CanAccessResource(User user);
    }
}
