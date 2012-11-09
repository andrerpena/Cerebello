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
                bool canAccess = this.CanAccessResource(cerebelloController.DbUser);

                if (!canAccess)
                {
                    // todo: check for json request: Request.AcceptTypes.Contains("application/json")
                    if (filterContext.HttpContext.Request.IsAjaxRequest())
                    {
                        filterContext.Result = new JsonUnauthorizedResult(
                            this.StatusDescription ?? "The current user is not authorized to access "
                            + "the resource because it hasn't got permission.")
                            {
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                            };
                    }
                    else
                    {
                        filterContext.Result = new HttpUnauthorizedResult(this.StatusDescription);
                    }
                }
            }
        }

        public abstract bool CanAccessResource(User user);

        public Type StatusDescriptionResourceType { get; set; }
        public string StatusDescriptionResourceName { get; set; }

        [Localizable(true)] // must be [Localizable(true)]
        [JetBrains.Annotations.LocalizationRequired(true)]
        public string StatusDescription { get; set; }
    }
}
