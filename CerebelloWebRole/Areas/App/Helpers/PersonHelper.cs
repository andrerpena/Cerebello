using System;
using System.Linq;
using Cerebello.Model;
using JetBrains.Annotations;

namespace CerebelloWebRole.Areas.App.Helpers
{
    public class PersonHelper
    {
        /// <summary>
        /// Gets the full name for a person
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public static string GetFullName(string firstName, string middleName, string lastName)
        {
            var names = new[] { (firstName ?? "").Trim(), (middleName ?? "").Trim(), (middleName ?? "").Trim() };
            return String.Join(" ", names.Where(s => !string.IsNullOrEmpty(s)));
        }

        /// <summary>
        /// Gets the full name for a person
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public static string GetFullName([NotNull] Person person)
        {
            if (person == null) throw new ArgumentNullException("person");
            return GetFullName(person.FirstName, person.MiddleName, person.LastName);
        }
    }
}