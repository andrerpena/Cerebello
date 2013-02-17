using System;
using System.Configuration;
using System.Web;

namespace CerebelloWebRole.Code
{
    public class DebugConfig
    {
        // this class is used only for debugging purposes... it may not be injectable, nor testable in any way

        public DebugConfig Instance { get; set; }

        private static readonly bool isLocalPresentation = ParseBool(ConfigurationManager.AppSettings["Debug:IsLocalPresentation"]);
        private static readonly bool useDesktopEmailBox = ParseBool(ConfigurationManager.AppSettings["Debug:UseDesktopEmailBox"]);
        private static readonly bool emailAddressOverride = ParseBool(ConfigurationManager.AppSettings["Debug:EmailAddressOverride"]);
        private static readonly TimeSpan currentTimeOffset = ParseTimeSpan(ConfigurationManager.AppSettings["Debug:CurrentTimeOffset"]);

        private static string EmptyDefault(string src, string def = null)
        {
            return string.IsNullOrWhiteSpace(src) ? def : src;
        }

        private static bool ParseBool(string src)
        {
            return bool.Parse(EmptyDefault(src, "false"));
        }

        private static TimeSpan ParseTimeSpan(string src)
        {
            return TimeSpan.Parse(EmptyDefault(src, "0"));
        }

        /// <summary>
        /// IsLocalPresentation may be true if IsLocalPresentation app setting is true in the config file,
        /// and the code is in DEBUG mode.
        /// </summary>
        public static bool IsLocalPresentation
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
        public static bool UseDesktopEmailBox
        {
            get
            {
#if DEBUG
                if (HttpContext.Current != null && HttpContext.Current.Items.Contains("Debug:UseDesktopEmailBox"))
                    return (bool)HttpContext.Current.Items["Debug:UseDesktopEmailBox"];
                return useDesktopEmailBox || IsLocalPresentation;
#else
                return false;
#endif
            }
            set
            {
#if DEBUG
                if (HttpContext.Current != null)
                    HttpContext.Current.Items["Debug:UseDesktopEmailBox"] = value;
#endif
            }
        }

        /// <summary>
        /// EmailAddressOverride is used to replace the destination of emails to "cerebello@cerebello.com.br".
        /// This is true only when the code is in DEBUG mode.
        /// </summary>
        public static bool EmailAddressOverride
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

        public static TimeSpan CurrentTimeOffset
        {
            get
            {
#if DEBUG
                return this.currentTimeOffset;
#else
                return TimeSpan.Zero;
#endif
            }
        }
    }
}