using System;
using System.ComponentModel;

namespace CerebelloWebRole.Areas.App.Models
{
    public class LockScriptViewModel
    {
        public LockScriptViewModel(LockScriptType type)
        {
            this.Type = type;
        }

        public LockScriptViewModel(LockScriptType type, [Localizable(true)] string selfText)
        {
            this.Type = type;
            this.SelfText = selfText;
        }

        public LockScriptType Type { get; set; }

        [Localizable(true)]
        public string SelfText { get; set; }
    }

    [Flags]
    public enum LockScriptType
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
