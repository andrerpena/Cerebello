namespace CerebelloWebRole.Code
{
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
