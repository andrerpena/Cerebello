using System;

namespace CerebelloWebRole.Code
{
    [Flags]
    public enum LockAccessType
    {
        None = 0,

        AdminOrOwner = Admin + Owner,
        SelfOrAdminOrOwner = Self + Admin + Owner,

        /// <summary>
        /// You, the logged user.
        /// </summary>
        You = 1,

        /// <summary>
        /// The owner of the information.
        /// </summary>
        Self = 2,

        /// <summary>
        /// Any administrator.
        /// </summary>
        Admin = 4,

        /// <summary>
        /// The owner of the account.
        /// </summary>
        Owner = 8,
    }
}