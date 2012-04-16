using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code.Security;

namespace CerebelloWebRole.Code
{
    public class RequiresPracticeAuthorizationAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            
        }
    }
}