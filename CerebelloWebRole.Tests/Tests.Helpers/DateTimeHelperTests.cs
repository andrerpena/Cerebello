using System;
using CerebelloWebRole.Code;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests.Helpers
{
    [TestClass]
    public class DateTimeHelperTests
    {
        #region ConvertToUtcDateTime
        [TestMethod]
        public void ConvertToUtcDateTime_InvalidTimeWhenChangingToDST()
        {
            var timeZoneInfo = GetTestTimeZoneInfoWithDst();

            var dateTime = new DateTime(2012, 10, 20, 00, 30, 00);
            var dateTime2 = DateTimeHelper.ConvertToUtcDateTime(dateTime, timeZoneInfo);
            Assert.AreEqual(new DateTime(2012, 10, 20, 03, 00, 00), dateTime2);
        }

        [TestMethod]
        public void ConvertToUtcDateTime_ConvertToUtcIsDst()
        {
            var timeZoneInfo = GetTestTimeZoneInfoWithDst();

            var dateTime = new DateTime(2012, 10, 22, 19, 00, 00);
            var dateTime2 = DateTimeHelper.ConvertToUtcDateTime(dateTime, timeZoneInfo);
            Assert.AreEqual(new DateTime(2012, 10, 22, 21, 00, 00), dateTime2);
        }

        [TestMethod]
        public void ConvertToUtcDateTime_ConvertToUtcIsNormalTime()
        {
            var timeZoneInfo = GetTestTimeZoneInfoWithDst();

            var dateTime = new DateTime(2012, 10, 19, 19, 00, 00);
            var dateTime2 = DateTimeHelper.ConvertToUtcDateTime(dateTime, timeZoneInfo);
            Assert.AreEqual(new DateTime(2012, 10, 19, 22, 00, 00), dateTime2);
        }

        private static TimeZoneInfo GetTestTimeZoneInfoWithDst()
        {
            var timeZoneInfo = TimeZoneInfo.CreateCustomTimeZone(
                "TEST",
                TimeSpan.FromHours(-3.0),
                "Test Time Zone",
                "Test Time Zone",
                "Test Time Zone Day-light Saving Time",
                new[]
                    {
                        TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                            new DateTime(2012, 01, 01),
                            new DateTime(2012, 12, 31),
                            TimeSpan.FromHours(1.0),
                            TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
                                new DateTime(1, 1, 1, 00, 00, 00),
                                10,
                                3,
                                DayOfWeek.Saturday
                                ),
                            TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
                                new DateTime(1, 1, 1, 00, 00, 00),
                                02,
                                3,
                                DayOfWeek.Saturday
                                )
                            )
                    });
            return timeZoneInfo;
        }
        #endregion

        #region GetPersonAgeInWords
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetPersonAgeInWords_WhenDateOfBirthIsLocal()
        {
            // this will be local (baam!);
            var dateOfBirth = DateTime.Now.AddYears(-1);
            var now = DateTime.UtcNow;

            // must trigger exception because 
            DateTimeHelper.GetPersonAgeInWords(dateOfBirth, now);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetPersonAgeInWords_WhenCurrentDateIsLocal()
        {
            var dateOfBirth = DateTime.UtcNow.AddYears(-1);
            // this will be local (baam!);
            var now = DateTime.Now;

            DateTimeHelper.GetPersonAgeInWords(dateOfBirth, now);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetPersonAgeInWords_WhenCurrentDateIsLowerThanDateOfBirth()
        {
            var dateOfBirth = DateTime.UtcNow;
            var now = DateTime.UtcNow.AddYears(-1);

            DateTimeHelper.GetPersonAgeInWords(dateOfBirth, now);
        }

        [TestMethod]
        public void GetPersonAgeInWords_ShortWhenYearIs0()
        {
            var dateOfBirth = DateTime.UtcNow.AddMonths(-1);
            var now = DateTime.UtcNow;

            var result = DateTimeHelper.GetPersonAgeInWords(dateOfBirth, now, true);
            Assert.AreEqual("0 anos", result);
        }

        [TestMethod]
        public void GetPersonAgeInWords_ShortWhenYearIs1()
        {
            var dateOfBirth = DateTime.UtcNow.AddMonths(-13);
            var now = DateTime.UtcNow;

            var result = DateTimeHelper.GetPersonAgeInWords(dateOfBirth, now, true);
            Assert.AreEqual("1 ano", result);
        }

        [TestMethod]
        public void GetPersonAgeInWords_ShortWhenYearIsMoreThan1()
        {
            var dateOfBirth = DateTime.UtcNow.AddMonths(-25);
            var now = DateTime.UtcNow;

            var result = DateTimeHelper.GetPersonAgeInWords(dateOfBirth, now, true);
            Assert.AreEqual("2 anos", result);
        }

        [TestMethod]
        public void GetPersonAgeInWords_NotShortWhenYearsIs0()
        {
            var dateOfBirth = new DateTime(1984, 1, 1);
            var now = dateOfBirth.AddMonths(10);

            var result = DateTimeHelper.GetPersonAgeInWords(dateOfBirth, now);
            Assert.AreEqual("0 anos, 10 meses e 1 dia", result);
        }

        [TestMethod]
        public void GetPersonAgeInWords_NotShortWhenYearsIs1()
        {
            var dateOfBirth = new DateTime(1984, 1, 1);
            var now = dateOfBirth.AddMonths(13);

            var result = DateTimeHelper.GetPersonAgeInWords(dateOfBirth, now);
            Assert.AreEqual("1 ano, 1 mês e 1 dia", result);
        }

        [TestMethod]
        public void GetPersonAgeInWords_NotShortWhenYearsIsMoreThan1()
        {
            var dateOfBirth = new DateTime(1984, 1, 1);
            var now = dateOfBirth.AddMonths(25);

            var result = DateTimeHelper.GetPersonAgeInWords(dateOfBirth, now);
            Assert.AreEqual("2 anos, 1 mês e 1 dia", result);
        }

        [TestMethod]
        public void GetPersonAgeInWords_NotShortWhenMonthsIs0()
        {
            var dateOfBirth = new DateTime(1984, 1, 1);
            var now = dateOfBirth.AddDays(25);

            var result = DateTimeHelper.GetPersonAgeInWords(dateOfBirth, now);
            Assert.AreEqual("0 anos, 0 meses e 25 dias", result);
        }

        [TestMethod]
        public void GetPersonAgeInWords_NotShortWhenMonthsIs1()
        {
            var dateOfBirth = new DateTime(1984, 1, 1);
            var now = dateOfBirth.AddDays(35);

            var result = DateTimeHelper.GetPersonAgeInWords(dateOfBirth, now);
            Assert.AreEqual("0 anos, 1 mês e 4 dias", result);
        }

        [TestMethod]
        public void GetPersonAgeInWords_NotShortWhenMonthsIsMoreThan1()
        {
            var dateOfBirth = new DateTime(1984, 1, 1);
            var now = dateOfBirth.AddDays(65);

            var result = DateTimeHelper.GetPersonAgeInWords(dateOfBirth, now);
            Assert.AreEqual("0 anos, 2 meses e 6 dias", result);
        }

        [TestMethod]
        public void GetPersonAgeInWords_NotShortWhenMonthsDaysIs0()
        {
            var dateOfBirth = new DateTime(1984, 1, 1);
            var now = dateOfBirth;

            var result = DateTimeHelper.GetPersonAgeInWords(dateOfBirth, now);
            Assert.AreEqual("0 anos, 0 meses e 0 dias", result);
        }

        [TestMethod]
        public void GetPersonAgeInWords_NotShortWhenMonthsDaysIs1()
        {
            var dateOfBirth = new DateTime(1984, 1, 1);
            var now = dateOfBirth.AddDays(1);

            var result = DateTimeHelper.GetPersonAgeInWords(dateOfBirth, now);
            Assert.AreEqual("0 anos, 0 meses e 1 dia", result);
        }

        [TestMethod]
        public void GetPersonAgeInWords_NotShortWhenMonthsDaysIsMoreThan1()
        {
            var dateOfBirth = new DateTime(1984, 1, 1);
            var now = dateOfBirth.AddDays(2);

            var result = DateTimeHelper.GetPersonAgeInWords(dateOfBirth, now);
            Assert.AreEqual("0 anos, 0 meses e 2 dias", result);
        }
        #endregion
    }
}
