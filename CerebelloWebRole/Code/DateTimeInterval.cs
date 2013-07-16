using System;

namespace CerebelloWebRole.Code
{
    public struct DateTimeInterval
    {
        private readonly DateTime start;
        private readonly DateTime end;

        public static DateTimeInterval FromDateAndDays(DateTime date, double days)
        {
            return new DateTimeInterval(date, date.AddDays(days));
        }

        public DateTimeInterval(DateTime start, DateTime end)
        {
            this.start = start;
            this.end = end;
        }

        /// <summary>
        /// Inclusive start of interval.
        /// </summary>
        public DateTime Start
        {
            get { return this.start; }
        }

        /// <summary>
        /// Exclusive end of interval.
        /// </summary>
        public DateTime End
        {
            get { return this.end; }
        }

        public bool Contains(DateTime dateTime)
        {
            return this.start <= dateTime && dateTime < this.end;
        }
    }
}