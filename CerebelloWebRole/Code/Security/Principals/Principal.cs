using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;
using CerebelloWebRole.Code.Security.Principals;

namespace CerebelloWebRole.Code.Security
{
    public abstract class Principal : GenericPrincipal {
        private UserData _userProfile;
        public UserData Profile { get { return _userProfile; } }

        public Principal(IIdentity identity, UserData userProfile)
            : base(identity, new String[]{})
        {
            this._userProfile = userProfile;
        }
    }
}