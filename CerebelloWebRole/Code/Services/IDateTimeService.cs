using System;

namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Interface representing the ability to get the current UTC date and time.
    /// </summary>
    public interface IDateTimeService
    {
        /// <summary>
        /// Gets the current UTC date and time.
        /// </summary>
        DateTime UtcNow { get; }
    }
}
