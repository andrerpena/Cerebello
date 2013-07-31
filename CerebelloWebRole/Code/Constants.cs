using System;

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

#if DEBUG
        /// <summary>
        /// These DEBUG values are required... we cannot risk other official accounts with data
        /// being uploaded in debug mode.
        /// </summary>
        public const string AZURE_STORAGE_PATIENT_FILES_CONTAINER_NAME = "patientfiles-debug";
        public const string AZURE_STORAGE_TEMP_FILES_CONTAINER_NAME = "temp-debug";
        public const string PERSON_PROFILE_PICTURE_CONTAINER_NAME = "personprofilepictures-debug";
#else
        /// <summary>
        /// The name of the file container for the patient files.
        /// A file container is a subset for the files storaged.
        /// *MUST BE LOWER CASE* (really, I lost 1 hour trying to figure out why. Ask Azure team)
        /// </summary>
        public const string AZURE_STORAGE_PATIENT_FILES_CONTAINER_NAME = "patientfiles";

        /// <summary>
        /// The name of the file container for the temp files.
        /// A file container is a subset for the files storaged.
        /// *MUST BE LOWER CASE* (really, I lost 1 hour trying to figure out why. Ask Azure team)
        /// </summary>
        public const string AZURE_STORAGE_TEMP_FILES_CONTAINER_NAME = "temp";

        /// <summary>
        /// The name of the file container for person profile pictures.
        /// A file container is a subset for the files storaged.
        /// *MUST BE LOWER CASE* (really, I lost 1 hour trying to figure out why. Ask Azure team)
        /// </summary>
        public const string PERSON_PROFILE_PICTURE_CONTAINER_NAME = "personprofilepictures";
#endif

#if DEBUG
        public const string DOMAIN = "localhost";
#else
        public const string DOMAIN = "www.cerebello.com.br";
#endif

        public const string SITE_CEREBELLO = "www.cerebello.com";
        public const string SITE_CEREBELLO_FULL = "https://www.cerebello.com";
        public const string EMAIL_POWEREDBY = SITE_CEREBELLO;

        public const string TELEPHONE_NUMBER = "(11) 3280-0995";
        public const string EMAIL_CEREBELLO = "cerebello@cerebello.com";

        public static readonly int MAX_HOURS_TO_VERIFY_TRIAL_ACCOUNT = 24;
        public static readonly TimeSpan MaxTimeToVerifyProfessionalAccount = TimeSpan.FromMinutes(30 + 15);
        public static readonly int MAX_DAYS_TO_RESET_PASSWORD = 30;
    }
}
