using System.Configuration;

namespace CerebelloWebRole.Code
{
    // todo: this class must be injectable... remove the Instance property and pass it everywhere.
    public sealed class Configuration
    {
        private Configuration()
        {
            var isLocalPresentationConfiguration = ConfigurationManager.AppSettings["IsLocalPresentation"];
            var useDesktopConfiguration = ConfigurationManager.AppSettings["UseDesktopEmailBox"];
            var emailAddressOverrideConfiguration = ConfigurationManager.AppSettings["EmailAddressOverride"];

            this.isLocalPresentation = isLocalPresentationConfiguration != null && bool.Parse(isLocalPresentationConfiguration);
            this.useDesktopEmailBox = useDesktopConfiguration != null && bool.Parse(useDesktopConfiguration);
            this.emailAddressOverride = emailAddressOverrideConfiguration != null && bool.Parse(emailAddressOverrideConfiguration);
        }

        public static Configuration Instance
        {
            get { return new Configuration(); }
        }

        private readonly bool isLocalPresentation;
        private readonly bool useDesktopEmailBox;
        private readonly bool emailAddressOverride;

        private static string EmptyDefault(string src, string def = null)
        {
            return string.IsNullOrWhiteSpace(src) ? def : src;
        }

        /// <summary>
        /// IsLocalPresentation may be true if IsLocalPresentation app setting is true in the config file,
        /// and the code is in DEBUG mode.
        /// </summary>
        public bool IsLocalPresentation
        {
            get
            {
#if DEBUG
                return this.isLocalPresentation;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// UseDesktopEmailBox may be true if UseDesktopEmailBox app setting is true in the config file,
        /// and the code is in DEBUG mode. When IsLocalPresentation is true, this is also true.
        /// </summary>
        public bool UseDesktopEmailBox
        {
            get
            {
#if DEBUG
                return this.useDesktopEmailBox || this.IsLocalPresentation;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// EmailAddressOverride is used to replace the destination of emails to "cerebello@cerebello.com.br".
        /// This is true only when the code is in DEBUG mode.
        /// </summary>
        public bool EmailAddressOverride
        {
            get
            {
#if DEBUG
                return this.emailAddressOverride;
#else
                return false;
#endif
            }
        }
    }
}