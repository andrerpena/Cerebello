using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Code.Security.Principals
{
    /// <summary>
    /// User information that is stored in the Authentication cookie.
    /// </summary>
    public class UserData
    {
        public int Id { get; set; }

        public String Email { get; set; }

        public String FullName { get; set; }

        /// <summary>
        /// Indicates whether the user has logged in with the default first-time password.
        /// When this is the case, the user will be asked to set an access password, before being abled to use the software.
        /// </summary>
        public bool IsUsingDefaultPassword { get; set; }

        public string PracticeIdentifier { get; set; }
    }
}