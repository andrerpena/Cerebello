namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The max size for "names" in the database
        /// </summary>
        public const int DB_NAME_MAX_LENGTH = 200;

        /// <summary>
        /// The size of a page of the grid
        /// </summary>
        public const int GRID_PAGE_SIZE = 20;

        /// <summary>
        /// Number of items to show when displaying only the most recent records.
        /// </summary>
        public const int RECENTLY_REGISTERED_LIST_MAXSIZE = 10;

        /// <summary>
        /// The default password given to every new user.
        /// When loggin in with this password the user will be asked to change the password.
        /// The user is not allowed to use this passwrod.
        /// </summary>
        public const string DEFAULT_PASSWORD = "123abc";

        /// <summary>
        /// The name of the file container for the patient files.
        /// A file container is a subset for the files storaged.
        /// *MUST BE LOWER CASE* (really, I lost 1 hour trying to figure out way. Ask Azure team)
        /// </summary>
        public const string AZURE_STORAGE_PATIENT_FILES_CONTAINER_NAME = "patientfiles";

        /// <summary>
        /// The name of the file container for the temp files.
        /// A file container is a subset for the files storaged.
        /// *MUST BE LOWER CASE* (really, I lost 1 hour trying to figure out way. Ask Azure team)
        /// </summary>
        public const string AZURE_STORAGE_TEMP_FILES_CONTAINER_NAME = "temp";

        /// <summary>
        /// The name of the file container for person profile pictures.
        /// A file container is a subset for the files storaged.
        /// *MUST BE LOWER CASE* (really, I lost 1 hour trying to figure out way. Ask Azure team)
        /// </summary>
        public const string PERSON_PROFILE_PICTURE_CONTAINER_NAME = "personprofilepictures";

#if DEBUG
        public const string DOMAIN = "localhost";
#else
        public const string DOMAIN = "www.cerebello.com.br";
#endif

        public const string EMAIL_POWEREDBY = "www.cerebello.com.br";
    }
}
