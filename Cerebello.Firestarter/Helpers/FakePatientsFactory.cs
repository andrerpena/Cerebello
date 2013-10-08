using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Helpers;
using CerebelloWebRole.Code;

namespace Cerebello.Firestarter.Helpers
{
    /// <summary>
    /// Helps creating fake users, specially when large ammounts are needed as for unit testing searches.
    /// </summary>
    /// <remarks>
    /// This class is static because Firestarter should be the entry point
    /// </remarks>
    internal static class FakePatientsFactory
    {
        /// <summary>
        /// Creates and saves the given number of fake patients
        /// </summary>
        public static List<Patient> CreateFakePatients(Doctor doctor, CerebelloEntities db, int count)
        {
            var maleFirstNames = new[]
                {
                    "Jacob",
                    "Mason",
                    "Ethan",
                    "Noah",
                    "William",
                    "Liam",
                    "Jayden",
                    "Michael",
                    "Alexander",
                    "Aiden",
                    "Daniel",
                    "Matthew",
                    "Elijah",
                    "James",
                    "Anthony",
                    "Benjamin",
                    "Joshua",
                    "Andrew",
                    "David",
                    "Joseph"
                };

            var femaleFirstNames = new[]
                {
                    "Sophia",
                    "Emma",
                    "Isabella",
                    "Olivia",
                    "Ava",
                    "Emily",
                    "Abigail",
                    "Mia",
                    "Madison",
                    "Elizabeth",
                    "Chloe",
                    "Ella",
                    "Avery",
                    "Addison",
                    "Aubrey",
                    "Lily",
                    "Natalie",
                    "Sofia",
                    "Zoey"
                };

            var surnames = new[]
                {
                    "Smith",
                    "Johnson",
                    "Williams",
                    "Jones",
                    "Brown",
                    "Davis",
                    "Miller",
                    "Wilson",
                    "Moore",
                    "Taylor",
                    "Anderson",
                    "Thomas",
                    "Jackson",
                    "White",
                    "Harris",
                    "Martin",
                    "Thompson",
                    "Garcia",
                    "Martinez",
                    "Robinson"
                };
            
            using (var rc = RandomContext.Create())
            {
                var random = rc.Random;
                var result = new List<Patient>();

                for (var i = 0; i < count; i++)
                {
                    // gender (random upper bound is exclusive)
                    var gender = random.Next(0, 2);

                    // first name
                    var possibleFirstNames = gender == 0 ? maleFirstNames : femaleFirstNames;
                    var firstName = possibleFirstNames[random.Next(possibleFirstNames.Length)];

                    // middle name
                    var middleName = surnames[random.Next(surnames.Length)];

                    // last name
                    var lastName = surnames[random.Next(surnames.Length)];

                    var birthDate = new DateTime(
                        random.Next(1950, 2000),
                        random.Next(1, 13),
                        random.Next(1, 29),
                        00,
                        00,
                        000,
                        DateTimeKind.Utc);

                    var patient = new Patient
                        {
                            Person = new Person
                                {
                                    FirstName = firstName,
                                    MiddleName = middleName,
                                    LastName = lastName,
                                    Gender = (short)gender,
                                    DateOfBirth = birthDate,
                                    MaritalStatus = (short?)random.Next(0, 4),
                                    SSN = "87324128910",
                                    CreatedOn = Firestarter.UtcNow,
                                    PracticeId = doctor.PracticeId,
                                },

                            Doctor = doctor,
                            PracticeId = doctor.PracticeId,
                        };

                    // it's important do remove diacritics because StmpClient crashes on non-ascii characters
                    patient.Person.Email =  StringHelper.RemoveDiacritics((firstName + PersonHelper.GetFullName(firstName, middleName, lastName)).ToLower() + "@fakemail.com");
                    Debug.Assert(patient.Person.Address == null);
                    patient.Person.Address = 
                        new Address
                            {
                                AddressLine1 = "Rua " + surnames[random.Next(surnames.Length)] + " " + surnames[random.Next(surnames.Length)],
                                AddressLine2 = "",
                                City = "Juiz de Fora",
                                StateProvince = "MG",
                                ZipCode = random.Next(36000000, 37000000).ToString(CultureInfo.InvariantCulture),
                                PracticeId = doctor.PracticeId
                            };

                    result.Add(patient);
                    db.Patients.AddObject(patient);
                }

                db.SaveChanges();
                return result;
            }
        }
    }
}
