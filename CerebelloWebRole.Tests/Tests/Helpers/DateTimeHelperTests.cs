using System;
using CerebelloWebRole.Code;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests.Helpers
{
    [TestClass]
    public class DateTimeHelperTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetPersonAgeInWords_WhenDateOfBirthIsLocal()
        {
            // this will be local (baam!);
            var dateOfBirth = DateTime.Now.AddYears(-1);
            var now = DateTime.UtcNow ;

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
    }
}
