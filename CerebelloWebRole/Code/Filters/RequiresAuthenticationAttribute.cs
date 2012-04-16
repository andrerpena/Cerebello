using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CerebelloWebRole.Code
{
    public class RequiresAuthenticationAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
           return httpContext.Request.IsAuthenticated;
        }
    }
}