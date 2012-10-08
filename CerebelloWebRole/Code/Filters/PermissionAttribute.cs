using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CerebelloWebRole.Code.Filters
{
    public class PermissionAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {

            return base.AuthorizeCore(httpContext);
        }
    }
}