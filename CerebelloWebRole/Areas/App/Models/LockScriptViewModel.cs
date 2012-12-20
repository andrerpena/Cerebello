using System;
using System.ComponentModel;

namespace CerebelloWebRole.Areas.App.Models
{
    public class LockScriptViewModel
    {
        public LockScriptViewModel(
            LockAccessType accessType,
            [Localizable(true)] string selfText = null,
            LockElementActionType elementAction = LockElementActionType.ScreenAccess,
            string cssClass = "lock-title",
            [Localizable(true)] string notes = null)
        {
            this.AccessType = accessType;
            this.SelfText = selfText;
            this.ElementAction = elementAction;
            this.CssClass = cssClass;
            this.Notes = notes;
        }

        public LockAccessType AccessType { get; set; }

        [Localizable(true)]
        public string SelfText { get; set; }

        public LockElementActionType ElementAction { get; set; }

        public string CssClass { get; set; }
        public string Notes { get; set; }
    }

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

    public enum LockElementActionType
    {
        /// <summary>
        /// Locks some not specified thing.
        /// </summary>
        Generic,

        /// <summary>
        /// Locks a specific field.
        /// </summary>
        Field,

        /// <summary>
        /// Locks the edition of a specific field.
        /// </summary>
        FieldEdit,

        /// <summary>
        /// Locks a groups of fields.
        /// </summary>
        Section,

        /// <summary>
        /// Locks the edition of a groups of fields.
        /// </summary>
        SectionEdit,

        /// <summary>
        /// Locks the access to an entire screen.
        /// </summary>
        ScreenAccess,

        /// <summary>
        /// Locks the access to an entire area of the software.
        /// </summary>
        SoftwareAreaAccess,
    }
}
