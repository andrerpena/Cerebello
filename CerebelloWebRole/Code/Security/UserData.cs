using System;

namespace CerebelloWebRole.Code
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

        /// <summary>
        /// Gets or sets whether the user logged in using a sys password.
        /// </summary>
        /// <remarks>
        /// This is a special purpose password used by Cerebello staff to access accounts as they were the users.
        /// The password can only be used once, after that it is erased.
        /// To get a new password, you need to update the User table with a new SYS_PasswordAlt.
        /// </remarks>
        public bool IsUsingSysPassword { get; set; }
    }
}