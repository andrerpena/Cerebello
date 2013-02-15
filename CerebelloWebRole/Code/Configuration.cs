using System.Configuration;

namespace CerebelloWebRole.Code
{
    // todo: this class must be injectable... remove the Instance property and pass it everywhere.
    public class Configuration
    {
        public static Configuration Instance
        {
            get { return new Configuration(); }
        }

        private readonly bool isLocalPresentation = bool.Parse(ConfigurationManager.AppSettings["IsLocalPresentation"]);
        private readonly bool useDesktopEmailBox = bool.Parse(ConfigurationManager.AppSettings["UseDesktopEmailBox"]);
        private readonly bool emailAddressOverride = bool.Parse(ConfigurationManager.AppSettings["EmailAddressOverride"]);

        private static string EmptyDefault(string src, string def = null)
        {
            return string.IsNullOrWhiteSpace(src) ? def : src;
        }

        /// <summary>
        /// IsLocalPresentation may be true if IsLocalPresentation app setting is true in the config file,
        /// and the code is in DEBUG mode.
        /// </summary>
        public virtual bool IsLocalPresentation
        {
            get
            {
#if DEBUG
                return isLocalPresentation;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// UseDesktopEmailBox may be true if UseDesktopEmailBox app setting is true in the config file,
        /// and the code is in DEBUG mode. When IsLocalPresentation is true, this is also true.
        /// </summary>
        public virtual bool UseDesktopEmailBox
        {
            get
            {
#if DEBUG
                return useDesktopEmailBox || IsLocalPresentation;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// EmailAddressOverride is used to replace the destination of emails to "cerebello@cerebello.com.br".
        /// This is true only when the code is in DEBUG mode.
        /// </summary>
        public virtual bool EmailAddressOverride
        {
            get
            {
#if DEBUG
                return emailAddressOverride;
#else
                return false;
#endif
            }
        }
    }
}