using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;

namespace CerebelloWebRole.Code
{

    public class GuestIdentity : IIdentity {
        public GuestIdentity() { }
        public string Name { get { return "Guest"; } }
        public string AuthenticationType { get { return "Forms"; } }
        public bool IsAuthenticated { get { return false; } }
    }
}
