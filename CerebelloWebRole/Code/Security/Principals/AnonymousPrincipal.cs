using System.Security.Principal;

namespace CerebelloWebRole.Code
{
    public class AnonymousPrincipal : Principal {
        public AnonymousPrincipal(IIdentity identity) : base(identity, null) { }
    }
}
