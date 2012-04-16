using System.Security.Principal;

namespace CerebelloWebRole.Code.Security
{
    public class AnonymousPrincipal : Principal {
        public AnonymousPrincipal(IIdentity identity) : base(identity, null) { }
    }
}
