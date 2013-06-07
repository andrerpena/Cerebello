using System.Security.Principal;

namespace CerebelloWebRole.Code
{
    public class AuthenticatedPrincipal : Principal {
        public AuthenticatedPrincipal(IIdentity identity, UserData user) : base(identity, user) { }
    }
}
