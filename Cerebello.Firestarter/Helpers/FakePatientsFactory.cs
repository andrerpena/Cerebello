using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Cerebello.Model;
using CerebelloWebRole.Models;

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
                    "André",
                    "Anderson",
                    "Alan",
                    "Artur",
                    "Bruno",
                    "Carlos",
                    "Daniel",
                    "Danilo",
                    "Ernani",
                    "Fabiano",
                    "Fábio",
                    "Guilherme",
                    "Hélcio",
                    "Jorge",
                    "Leonardo",
                    "Marcelo",
                    "Miguel", // po cara! eu tb!
                    "Nelson",
                    "Osvaldo",
                    "Patrício",
                    "Roberto",
                    "Ronan",
                    "Thiago"
                };

            var femaleFirstNames = new[]
                {
                    "Alice",
                    "Aline",
                    "Bianca",
                    "Bruna",
                    "Carla",
                    "Daniela",
                    "Elaine",
                    "Fabíola",
                    "Fabiana",
                    "Giovana",
                    "Íngridi",
                    "Jaqueline",
                    "Larissa",
                    "Marcela",
                    "Natália",
                    "Paula",
                    "Quelen",
                    "Renata",
                    "Silvana",
                    "Tatiana",
                    "Valquíria",
                    "Zilá"
                };

            var middleNames = new[]
                {
                    "Albuquerque",
                    "Almeida",
                    "Bastos",
                    "Queiróz",
                    "Teixeira",
                    "Silva",
                    "Rodrigues",
                    "Santos",
                    "Pena",
                    "Bicudo",
                    "Gonçalves",
                    "Machado",
                    "Vianna",
                    "Souza",
                    "Moreira",
                    "Vieira",
                    "Correia",
                    "Reis",
                    "Delgado"
                };

            var professions = new[]
                {
                    "Pedreiro(a)",
                    "Arquiteto(a)",
                    "Programador(a)",
                    "Economista",
                    "Engenheiro(a)",
                    "Padeiro(a)",
                    "Eletricista",
                    "Vendedor(a)",
                    "Consultor(a)",
                    "Biólogo(a)",
                    "Carpinteiro(a)",
                    "Advogado(a)"
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

                    // middle names
                    var chosenMiddleNames = new string[random.Next(2, 4)];
                    for (var j = 0; j < chosenMiddleNames.Length; j++)
                        chosenMiddleNames[j] = middleNames[random.Next(middleNames.Length)];

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
                                    FullName = firstName + " " + string.Join(" ", chosenMiddleNames),
                                    Gender = (short)gender,
                                    DateOfBirth = birthDate,
                                    MaritalStatus = (short?)random.Next(0, 4),
                                    BirthPlace = "Brasileiro(a)",
                                    CPF = "87324128910",
                                    CPFOwner = (int)TypeCpfOwner.PatientItself,
                                    Profession = professions[random.Next(professions.Length)],
                                    CreatedOn = DateTime.UtcNow,
                                    PracticeId = doctor.PracticeId,
                                },

                            Doctor = doctor,
                            PracticeId = doctor.PracticeId,
                        };

                    patient.Person.Email = firstName + string.Join("", chosenMiddleNames) + "@gmail.com";
                    Debug.Assert(patient.Person.Address == null);
                    patient.Person.Address = new Address
                        {
                            CEP = random.Next(36000000, 37000000).ToString(CultureInfo.InvariantCulture),
                            StateProvince = "MG",
                            City = "Juiz de Fora",
                            Neighborhood = "Centro",
                            Street =
                                "Rua " + middleNames[random.Next(middleNames.Length)] + " " + middleNames[random.Next(middleNames.Length)],
                            Complement = "",
                            PracticeId = doctor.PracticeId,
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
