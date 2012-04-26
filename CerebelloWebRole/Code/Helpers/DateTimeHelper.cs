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
        public enum RelativeDateOptions
        {
            ReplaceYesterdayAndTomorrow = 1,
            ReplaceToday = 2,
            IncludeSuffixes = 4,
            IncludePrefixes = 8
        }

        // Methods
        public static int CalculateAge(DateTime birth)
        {
            DateTime today = DateTime.Today;
            int num = today.Year - birth.Year;
            if (today < birth.AddYears(num))
            {
                num--;
            }
            return num;
        }

        /// <summary>
        /// Returns 
        /// </summary>
        /// <returns></returns>
        public static DateTime GetTimeZoneNow()
        {
            return DateTime.Now;
        }

        /// <summary>
        /// Converts a time to the current time zone. The passed time zone is 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ConvertToCurrentTimeZone(DateTime dateTime)
        {
            var currentTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            var convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, currentTimeZoneInfo);
            return convertedDateTime;
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
                    return "sábado";
                case DayOfWeek.Sunday:
                    return "domingo";
                case DayOfWeek.Thursday:
                    return "quinta-feira";
                case DayOfWeek.Tuesday:
                    return "terça-feira";
                case DayOfWeek.Wednesday:
                    return "quarta-feira";
                default:
                    throw new NotImplementedException();
            }
        }

        public static string ConvertToRelative(DateTime dateTime, DateTime now, RelativeDateOptions flags = 0)
        {
            dateTime = dateTime.Date;
            now = now.Date;

            TimeSpan span = now - dateTime;

            bool isPast = span.TotalMinutes > 0;
            double totalSeconds = Math.Abs(span.TotalSeconds);

            // a representação textual do DateTime
            String spelledDate = null;

            bool requiresPreposition = true;

            if ((flags & RelativeDateOptions.ReplaceToday) == RelativeDateOptions.ReplaceToday && dateTime.Date == now.Date)
            {
                requiresPreposition = false;
                spelledDate = "hoje";
            }
            else if ((flags & RelativeDateOptions.ReplaceYesterdayAndTomorrow) == RelativeDateOptions.ReplaceYesterdayAndTomorrow && dateTime.Date == now.Date.AddDays(1))
            {
                requiresPreposition = false;
                spelledDate = "amanhã";
            }

            else if ((flags & RelativeDateOptions.ReplaceYesterdayAndTomorrow) == RelativeDateOptions.ReplaceYesterdayAndTomorrow && dateTime.Date == now.Date.AddDays(-1))
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
                spelledDate = ((num2 <= 1) ? "aprox. um mês" : ("aprox. " + num2 + " meses"));
            }
            else
            {
                int num3 = Convert.ToInt32(Math.Floor((double)(((double)Math.Abs(span.Days)) / 365.0)));
                requiresPreposition = true;
                spelledDate = ((num3 <= 1) ? "aprox. um ano" : ("aprox. " + num3 + " anos"));
            }


            if (requiresPreposition & ((flags & RelativeDateOptions.IncludeSuffixes) == RelativeDateOptions.IncludeSuffixes) && isPast)
                spelledDate += " atrás";

            if (requiresPreposition & ((flags & RelativeDateOptions.IncludePrefixes) == RelativeDateOptions.IncludePrefixes))
                if (isPast)
                    spelledDate = "há " + spelledDate;
                else
                    spelledDate = "daqui há " + spelledDate;

            return spelledDate;
        }

        public static string ConvertToRelativeShort(DateTime dateTime)
        {
            TimeSpan span = new TimeSpan(DateTime.UtcNow.Ticks - dateTime.Ticks);
            double totalSeconds = span.TotalSeconds;

            if (totalSeconds < 60.0)
                return ((span.Seconds == 1) ? "1s atrás" : (span.Seconds + "s atrás"));

            if (totalSeconds < 120.0)
                return "1m atrás";

            if (totalSeconds < 2700.0)
                return (span.Minutes + "m atrás");

            if (totalSeconds < 5400.0)
                return "1h atrás";

            if (totalSeconds < 86400.0)
                return (span.Hours + "h atrás");

            if (totalSeconds < 172800.0)
                return "ontem";

            if (totalSeconds < 2592000.0)
                return (span.Days + "d atrás");

            if (totalSeconds < 31104000.0)
            {
                int num2 = Convert.ToInt32(Math.Floor((double)(((double)span.Days) / 30.0)));
                return ((num2 <= 1) ? "1m atrás" : (num2 + "m atrás"));
            }
            int num3 = Convert.ToInt32(Math.Floor((double)(((double)span.Days) / 365.0)));
            return ((num3 <= 1) ? "1a atrás" : (num3 + "a atrás"));
        }

        /// <summary>
        /// Retorna a data formatada. Ex. 25/12/2010
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string GetFormattedDate(DateTime dateTime)
        {
            StringBuilder builder = new StringBuilder();
            if (dateTime.Day < 10)
                builder.Append("0");
            builder.Append(dateTime.Day.ToString());
            builder.Append("/");
            if (dateTime.Month < 10)
                builder.Append("0");
            builder.Append(dateTime.Month.ToString());
            builder.Append("/");
            builder.Append(dateTime.Year.ToString());
            return builder.ToString();
        }

        /// <summary>
        /// Retorna uma hora formatada. Ex. 12:00h
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string GetFormattedTime(DateTime dateTime)
        {
            StringBuilder builder = new StringBuilder();
            if (dateTime.Hour < 10)
                builder.Append("0");
            builder.Append(dateTime.Hour.ToString());
            builder.Append(":");
            if (dateTime.Minute < 10)
                builder.Append("0");
            builder.Append(dateTime.Minute.ToString());
            builder.Append("h");
            return builder.ToString();
        }


        /// <summary>
        /// Converte o DateTime passado em um formato: 12/08/2011 às 13:40h
        /// </summary>
        public static string GetFormattedDateAndTime(DateTime dateTime)
        {
            return String.Format("{0} às {1}", dateTime, dateTime);
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

        public static int GetPersonAge(DateTime dateOfBirth)
        {
            DateTime now = DateTime.Today;
            int age = now.Year - dateOfBirth.Year;
            if (dateOfBirth > now.AddYears(-age)) age--;

            return age;
        }

        public static String GetPersonAgeInWords(DateTime dateOfBirth, bool @short = false)
        {
            DateTime currentDate = DateTimeHelper.GetTimeZoneNow();

            if (currentDate < dateOfBirth)
                return "ainda não nascida";

            TimeSpan difference = currentDate.Subtract(dateOfBirth);

            // This is to convert the timespan to datetime object
            DateTime age = DateTime.MinValue + difference;

            // Min value is 01/01/0001
            // Actual age is say 24 yrs, 9 months and 3 days represented as timespan
            // Min Valye + actual age = 25 yrs , 10 months and 4 days.
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
