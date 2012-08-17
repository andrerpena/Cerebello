using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cerebello.Model;
using CerebelloWebRole.Models;
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
            var maleFirstNames = new string[] {
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
                "Nelson",
                "Osvaldo",
                "Patrício",
                "Roberto",
                "Ronan",
                "Thiago"
            };

            var femaleFirstNames = new string[] {
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

            var middleNames = new string[] {
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

            var professions = new string[] {
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

            var random = new Random();
            List<Patient> result = new List<Patient>();

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

                DateTime birthDate = new DateTime(random.Next(1950, 2000), random.Next(1, 13), random.Next(1, 29), 0, 0, 0, DateTimeKind.Utc);

                var patient = new Patient()
                {
                    Person = new Person()
                    {
                        FullName = firstName + " " + string.Join(" ", chosenMiddleNames),
                        Gender = (short)gender,
                        DateOfBirth = birthDate,
                        MaritalStatus = (short?)random.Next(0, 4),
                        BirthPlace = "Brasileiro(a)",
                        CPF = "87324128910",
                        CPFOwner = (int)TypeCPFOwner.PatientItself,
                        Profession = professions[random.Next(professions.Length)],
                        CreatedOn = DateTime.UtcNow
                    },

                    Doctor = doctor
                };

                var practiceId = doctor.Users.FirstOrDefault().PracticeId;

                var urlId = Firestarter.GetUniquePatientUrlId(db, patient.Person.FullName, practiceId);
                if (urlId == null)
                {
                    throw new Exception(
                        // Todo: this message is also used in the AuthenticationController.
                        "Quantidade máxima de homônimos excedida.");
                }
                patient.Person.UrlIdentifier = urlId;

                patient.Person.Emails.Add(new Email() { Address = firstName + string.Join("", chosenMiddleNames) + "@gmail.com" });
                patient.Person.Addresses.Add(new Address()
                {
                    CEP = random.Next(36000000, 37000000).ToString(),
                    StateProvince = "MG",
                    City = "Juiz de Fora",
                    Neighborhood = "Centro",
                    Street = "Rua " + middleNames[random.Next(middleNames.Length)] + " " + middleNames[random.Next(middleNames.Length)],
                    Complement = ""
                });

                result.Add(patient);
                db.Patients.AddObject(patient);
            }

            db.SaveChanges();
            return result;
        }
    }
}
