using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using CerebelloWebRole.Code.Mvc;

namespace CerebelloWebRole.Code
{
    public class DateTimeHelper
    {
        [Flags]
        public enum RelativeDateOptions
        {
            ReplaceYesterdayAndTomorrow = 1,
            ReplaceToday = 2,
            IncludeSuffixes = 4,
            IncludePrefixes = 8
        }

        // Methods

        public static IEnumerable<DateTime> Range(DateTime start, int count, Func<DateTime, DateTime> nextGetter)
        {
            DateTime d = start;
            for (int i = 0; i < count; i++, d = nextGetter(d))
                yield return d;
        }

        [Obsolete("This method is not being used. 2012-08-15.", true)]
        public static int CalculateAge(DateTime birth, DateTime now)
        {
            DateTime today = now.Date;
            int num = today.Year - birth.Year;
            if (today < birth.AddYears(num))
            {
                num--;
            }
            return num;
        }

        public static string GetDayOfWeekAsString(DateTime date)
        {
            return GetDayOfWeekAsString(date.DayOfWeek);
        }

        /// <summary>
        /// Retorna o dia da semana como string. Ex: segunda-feira
        /// </summary>
        public static string GetDayOfWeekAsString(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Friday:
                    return "sexta-feira";
                case DayOfWeek.Monday:
                    return "segunda-feira";
                case DayOfWeek.Saturday:
                    return "s�bado";
                case DayOfWeek.Sunday:
                    return "domingo";
                case DayOfWeek.Thursday:
                    return "quinta-feira";
                case DayOfWeek.Tuesday:
                    return "ter�a-feira";
                case DayOfWeek.Wednesday:
                    return "quarta-feira";
                default:
                    throw new NotImplementedException();
            }
        }

        public static string ConvertToRelative(DateTime localDateTime, DateTime localNow, RelativeDateOptions flags = 0)
        {
            // This method is intended to return a human readable output,
            // so this means that the dates passed as input to this method must be local to the user.
            if (localDateTime.Kind != DateTimeKind.Unspecified)
                throw new ArgumentException("'localDateTime' must be expressed in practice time-zone.", "localDateTime");

            if (localNow.Kind != DateTimeKind.Unspecified)
                throw new ArgumentException("'localNow' must be expressed in practice time-zone.", "localNow");

            // todo: WTF??? this is throwing away needed information for this method... it needs the seconds data.
            localDateTime = localDateTime.Date;
            localNow = localNow.Date;

            TimeSpan span = localNow - localDateTime;

            bool isPast = span.TotalMinutes > 0;
            double totalSeconds = Math.Abs(span.TotalSeconds);

            // a representa��o textual do DateTime
            String spelledDate = null;

            bool requiresPreposition = true;

            if ((flags & RelativeDateOptions.ReplaceToday) == RelativeDateOptions.ReplaceToday && localDateTime.Date == localNow.Date)
            {
                requiresPreposition = false;
                spelledDate = "hoje";
            }
            else if ((flags & RelativeDateOptions.ReplaceYesterdayAndTomorrow) == RelativeDateOptions.ReplaceYesterdayAndTomorrow && localDateTime.Date == localNow.Date.AddDays(1))
            {
                requiresPreposition = false;
                spelledDate = "amanh�";
            }

            else if ((flags & RelativeDateOptions.ReplaceYesterdayAndTomorrow) == RelativeDateOptions.ReplaceYesterdayAndTomorrow && localDateTime.Date == localNow.Date.AddDays(-1))
            {
                requiresPreposition = false;
                spelledDate = "ontem";
            }
            else if (totalSeconds < 172800.0)
            {
                if (totalSeconds < 60.0)
                    spelledDate = ((Math.Abs(span.Seconds) == 1) ? "um segundo" : (Math.Abs(span.Seconds) + " segundos"));

                else if (totalSeconds < 120.0)
                    spelledDate = "um minuto";

                else if (totalSeconds < 2700.0)
                    spelledDate = (Math.Abs(span.Minutes) + " minutos");

                else if (totalSeconds < 5400.0)
                    spelledDate = "uma hora";

                else if (totalSeconds < 86400.0)
                    spelledDate = (Math.Abs(span.Hours) + " horas");

                else if (totalSeconds < 172800.0)
                    spelledDate = "um dia";
            }
            else if (totalSeconds < 2592000.0)
            {
                requiresPreposition = true;
                spelledDate = (Math.Abs(span.Days) + " dias");
            }
            else if (totalSeconds < 31104000.0)
            {
                requiresPreposition = true;
                int num2 = Convert.ToInt32(Math.Floor((double)(((double)Math.Abs(span.Days)) / 30.0)));
                spelledDate = ((num2 <= 1) ? "aprox. um m�s" : ("aprox. " + num2 + " meses"));
            }
            else
            {
                int num3 = Convert.ToInt32(Math.Floor((double)(((double)Math.Abs(span.Days)) / 365.0)));
                requiresPreposition = true;
                spelledDate = ((num3 <= 1) ? "aprox. um ano" : ("aprox. " + num3 + " anos"));
            }


            if (requiresPreposition & ((flags & RelativeDateOptions.IncludeSuffixes) == RelativeDateOptions.IncludeSuffixes) && isPast)
                spelledDate += " atr�s";

            if (requiresPreposition & ((flags & RelativeDateOptions.IncludePrefixes) == RelativeDateOptions.IncludePrefixes))
                if (isPast)
                    spelledDate = "h� " + spelledDate;
                else
                    spelledDate = "daqui a " + spelledDate;

            return spelledDate;
        }

        [Obsolete("This method is not being used. 2012-08-15.", true)]
        public static string ConvertToRelativeShort(DateTime localDateTime, DateTime localNow)
        {
            // This method is intended to return a human readable output,
            // so this means that the dates passed as input to this method must be local to the user.
            if (localDateTime.Kind != DateTimeKind.Unspecified)
                throw new ArgumentException("'localDateTime' must be expressed in practice time-zone.", "localDateTime");

            if (localNow.Kind != DateTimeKind.Unspecified)
                throw new ArgumentException("'localNow' must be expressed in practice time-zone.", "localNow");

            TimeSpan span = localNow - localDateTime;
            double totalSeconds = span.TotalSeconds;

            if (totalSeconds < 60.0)
                return ((span.Seconds == 1) ? "1s atr�s" : (span.Seconds + "s atr�s"));

            if (totalSeconds < 120.0)
                return "1m atr�s";

            if (totalSeconds < 2700.0)
                return (span.Minutes + "m atr�s");

            if (totalSeconds < 5400.0)
                return "1h atr�s";

            if (totalSeconds < 86400.0)
                return (span.Hours + "h atr�s");

            if (totalSeconds < 172800.0)
                return "ontem";

            if (totalSeconds < 2592000.0)
                return (span.Days + "d atr�s");

            if (totalSeconds < 31104000.0)
            {
                int num2 = Convert.ToInt32(Math.Floor((double)(((double)span.Days) / 30.0)));
                return ((num2 <= 1) ? "1m atr�s" : (num2 + "m atr�s"));
            }
            int num3 = Convert.ToInt32(Math.Floor((double)(((double)span.Days) / 365.0)));
            return ((num3 <= 1) ? "1a atr�s" : (num3 + "a atr�s"));
        }

        /// <summary>
        /// Retorna a data formatada. Ex. 25/12/2010
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        [Obsolete("This method is not being used. 2012-08-15.", true)]
        public static string GetFormattedDate(DateTime localDateTime)
        {
            // This method is intended to return a human readable output,
            // so this means that the dates passed as input to this method must be local to the user.
            if (localDateTime.Kind != DateTimeKind.Unspecified)
                throw new ArgumentException("'localDateTime' must be expressed in practice time-zone.", "localDateTime");

            StringBuilder builder = new StringBuilder();
            if (localDateTime.Day < 10)
                builder.Append("0");
            builder.Append(localDateTime.Day.ToString());
            builder.Append("/");
            if (localDateTime.Month < 10)
                builder.Append("0");
            builder.Append(localDateTime.Month.ToString());
            builder.Append("/");
            builder.Append(localDateTime.Year.ToString());
            return builder.ToString();
        }

        /// <summary>
        /// Retorna uma hora formatada. Ex. 12:00h
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string GetFormattedTime(DateTime localDateTime)
        {
            // This method is intended to return a human readable output,
            // so this means that the dates passed as input to this method must be local to the user.
            if (localDateTime.Kind != DateTimeKind.Unspecified)
                throw new ArgumentException("'localDateTime' must be expressed in practice time-zone.", "localDateTime");

            StringBuilder builder = new StringBuilder();
            if (localDateTime.Hour < 10)
                builder.Append("0");
            builder.Append(localDateTime.Hour.ToString());
            builder.Append(":");
            if (localDateTime.Minute < 10)
                builder.Append("0");
            builder.Append(localDateTime.Minute.ToString());
            builder.Append("h");
            return builder.ToString();
        }


        /// <summary>
        /// Converte o DateTime passado em um formato: 12/08/2011 �s 13:40h
        /// </summary>
        [Obsolete("This method is not being used. 2012-08-15.", true)]
        public static string GetFormattedDateAndTime(DateTime dateTime)
        {
            // todo: WTF??? this method is stub.
            return String.Format("{0} �s {1}", dateTime, dateTime);
        }

        public static string GetShortForMonth(int monthIndex)
        {
            return DateTimeHelper.GetShortForMonth(monthIndex, true);
        }

        public static string GetShortForMonth(int monthIndex, bool includeDotAtTheEnd)
        {
            string[] strArray = new string[] { "jan", "fev", "mar", "abr", "mai", "jun", "jul", "ago", "set", "out", "nov", "dez" };
            return strArray[monthIndex - 1] + (includeDotAtTheEnd ? "." : "");
        }

        [Obsolete("This method is not being used. 2012-08-15.", true)]
        public static int GetPersonAge(DateTime dateOfBirth, DateTime now)
        {
            now = now.Date;
            dateOfBirth = dateOfBirth.Date;

            int age = now.Year - dateOfBirth.Year;
            if (dateOfBirth > now.AddYears(-age)) age--;

            return age;
        }

        public static String GetPersonAgeInWords(DateTime dateOfBirth, DateTime currentDate, bool @short = false)
        {
            if (currentDate < dateOfBirth)
                return "ainda n�o nascida";

            TimeSpan difference = currentDate.Subtract(dateOfBirth);
            // This is to convert the timespan to datetime object
            DateTime age = DateTime.MinValue + difference;

            // Min value is 01/01/0001
            // Actual age is say 24 yrs, 9 months and 3 days represented as timespan
            // Min Value + actual age = 25 yrs , 10 months and 4 days.
            // subtract our addition or 1 on all components to get the actual date.

            int ageInYears = age.Year - 1;
            int ageInMonths = age.Month - 1;
            int ageInDays = age.Day - 1;

            if (@short)
                return String.Format("{0} anos", ageInYears);
            else
                return String.Format("{0} anos, {1} meses and {2} dias", ageInYears, ageInMonths, ageInDays);
        }

        public static String FormatDate(DateTime date)
        {
            return date.Month + "/" + date.Day + "/" + date.Year;
        }

        /// <summary>
        /// Returns a time-span given a string in the format "hh:mm"
        /// where hh is the hour component, and mm is the minute component.
        /// Both must have 2 digits.
        /// </summary>
        /// <param name="time">String in the format "hh:mm".</param>
        /// <returns></returns>
        public static TimeSpan GetTimeSpan(string time)
        {
            var match = TimeDataTypeAttribute.Regex.Match(time);
            return new TimeSpan(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), 0);
        }
    }


    public enum TimeFormat
    {
        Absolute,
        Relative
    }
}
