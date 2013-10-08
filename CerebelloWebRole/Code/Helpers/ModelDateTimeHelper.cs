using System;
using Cerebello.Model;

namespace CerebelloWebRole.Code
{
    public class ModelDateTimeHelper
    {
        /// <summary>
        /// Converts the specified UTC date and time for the location of the current practice.
        /// </summary>
        /// <param name="practice"> </param>
        /// <param name="utcDateTime"></param>
        /// <returns></returns>
        public static DateTime ConvertToLocalDateTime(Practice practice, DateTime utcDateTime)
        {
            if (practice == null) throw new ArgumentNullException("practice");

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(practice.WindowsTimeZoneId);
            var result = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);
            return result;
        }

        /// <summary>
        /// Converts the specified nullable UTC date and time for the location of the current practice.
        /// </summary>
        /// <param name="practice"> </param>
        /// <param name="utcDateTime"></param>
        /// <returns></returns>
        public static DateTime? ConvertToLocalDateTime(Practice practice, DateTime? utcDateTime)
        {
            if (practice == null) throw new ArgumentNullException("practice");

            if (utcDateTime == null)
                return null;

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(practice.WindowsTimeZoneId);
            var result = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime.Value, timeZoneInfo);
            return result;
        }
        
        /// <summary>
        /// Converts the specified nullable UTC date and time for the location of the current practice.
        /// </summary>
        /// <param name="utcDateTime"></param>
        /// <returns></returns>
        public DateTime? ConvertToLocalDateTime(DateTime? utcDateTime)
        {
            if (utcDateTime.HasValue)
                return this.ConvertToLocalDateTime(utcDateTime.Value);

            return null;
        }

        /// <summary>
        /// Converts the specified date and time at the location of the current practice to UTC.
        /// </summary>
        /// <param name="practice"> </param>
        /// <param name="practiceDateTime"></param>
        /// <returns></returns>
        public static DateTime ConvertToUtcDateTime(Practice practice, DateTime practiceDateTime)
        {
            if (practice == null) throw new ArgumentNullException("practice");

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(practice.WindowsTimeZoneId);
            return DateTimeHelper.ConvertToUtcDateTime(practiceDateTime, timeZoneInfo);
        }

        /// <summary>
        /// Converts the specified date and time at the location of the current practice to UTC.
        /// </summary>
        /// <param name="practice"> </param>
        /// <param name="practiceDateTime"></param>
        /// <returns></returns>
        public static DateTime? ConvertToUtcDateTime(Practice practice, DateTime? practiceDateTime)
        {
            if (practice == null) throw new ArgumentNullException("practice");

            if (practiceDateTime == null)
                return null;

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(practice.WindowsTimeZoneId);
            return DateTimeHelper.ConvertToUtcDateTime(practiceDateTime.Value, timeZoneInfo);
        }
    }
}