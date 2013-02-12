using System.Configuration;
using System.Web;

namespace CerebelloWebRole.Code
{
    // todo: this class must be injectable... remove the Instance property and pass it everywhere.
    public class Configuration
    {
        public static Configuration Instance
        {
            get { return new Configuration(); }
        }

        private readonly bool isLocalPresentation = ConfigurationManager.AppSettings["IsLocalPresentation"].ToLowerInvariant() == "true";

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
    }
}