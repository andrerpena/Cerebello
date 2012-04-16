using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;
using CerebelloWebRole.Code.Security.Principals;

namespace CerebelloWebRole.Code.Security
{
    public class AuthenticatedPrincipal : Principal {
        public AuthenticatedPrincipal(IIdentity identity, UserData user) : base(identity, user) { }
    }
}
