using System;

namespace CerebelloWebRole.Code.Services
{
    /// <summary>
    /// Service used to get the current UTC date and time.
    /// </summary>
    public class DateTimeService : IDateTimeService
    {
        /// <summary>
        /// Gets the current date and time.
        /// </summary>
        public DateTime UtcNow
        {
            get { return DateTime.UtcNow + DebugConfig.CurrentTimeOffset; }
        }
    }
}
