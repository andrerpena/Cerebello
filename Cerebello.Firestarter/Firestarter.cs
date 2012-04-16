using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cerebello.Model;
using CerebelloWebRole.Models;
using CerebelloWebRole.Code;

namespace Cerebello.Firestarter
{
    public class Firestarter
    {
        /// <summary>
        /// Crates a fake user, doctor and practice
        /// </summary>
        public static Doctor CreateFakeUserAndPractice(CerebelloEntities db)
        {
            var entity = new MedicalEntity()
            {
                Name = "CRMMG"
            };

            db.MedicalEntities.AddObject(entity);

            var specialty = new MedicalSpecialty()
            {
                Name = "Psiquiatria"
            };

            db.MedicalSpecialties.AddObject(specialty);

            User user = new User()
            {
                Person = new Person()
                {
                    DateOfBirth = new DateTime(1984, 08, 12),
                    FullName = "Gregory House",
                    UrlIdentifier = "gregoryhouse",
                    Gender = (int) TypeGender.Male,
                    CreatedOn = DateTime.UtcNow,
                },

                LastActiveOn = DateTime.UtcNow,
                Password = "aThARLVPRzyS7yAb4WGDDsppzrA=",
                PasswordSalt = "nnKvjK+67w7OflE9Ri4MQw==",
                Email = "andrerpena@gmail.com",
                GravatarEmailHash = "574700aef74b21d386ba1250b77d20c6"
            };
            user.Person.Emails.Add(new Email() { Address = "andrerpena@gmail.com" });

            db.Users.AddObject(user);

            Doctor doctor = new Doctor()
            {
                CRM = "12345",
                MedicalSpecialty = specialty,
                MedicalEntity = entity
            };

            License license = new License()
            {
                IsDeleted = false,
                IsSuspended = false,
                Type = (int)TypeLicense.Commercial
            };

            Practice practice = new Practice()
            {
                Name = "Consultório do Dr. House",
                UrlIdentifier = "consultoriodrhourse",
                CreatedOn = DateTime.UtcNow,
                Owner = user,
                License = license
            };

            db.Practices.AddObject(practice);

            UserPractice practiceUser = new UserPractice()
            {
                Practice = practice,
                User = user,
                Doctor = doctor
            };

            db.UserPractices.AddObject(practiceUser);

            return doctor;
        }

        /// <summary>
        /// Creates fake patients
        /// </summary>
        public static void CreateFakePatients(Doctor doctor, CerebelloEntities db)
        {
            // patient 1
            Patient patient = new Patient()
            {
                Person = new Person()
                {
                    FullName = "Pedro Paulo Machado",
                    Gender = (int)TypeGender.Male,
                    DateOfBirth = new DateTime(1982, 10, 12),
                    MaritalStatus = (int)TypeMaritalStatus.Casado,
                    BirthPlace = "Brasileiro",
                    CPF = "87324128910",
                    CPFOwner = (int)TypeCPFOwner.PatientItself,
                    Profession = "Encarregado de Obras",
                    UrlIdentifier = StringHelper.GenerateUrlIdentifier("Pedro Paulo Machado"),
                    CreatedOn = DateTime.UtcNow
                },
                Doctor = doctor
            };
            patient.Person.Emails.Add(new Email() { Address = "paulomachado@gmail.com" });
            patient.Person.Addresses.Add(new Address()
            {
                CEP = "602500330",
                StateProvince = "RJ",
                City = "Rio de Janeiro",
                Neighborhood = "Jacarepaguá",
                Street = "Rua Estrada do Pau Ferro 329",
                Complement = ""
            });

            db.Patients.AddObject(patient);


            // patient 2
            patient = new Patient()
            {
                Person = new Person()
                {
                    FullName = "Laura Gonzaga Deniz",
                    Gender = (int)TypeGender.Female,
                    DateOfBirth = new DateTime(1984, 8, 22),
                    MaritalStatus = (int)TypeMaritalStatus.Solteiro,
                    BirthPlace = "Brasileira",
                    CPF = "72889321041",
                    CPFOwner = (int)TypeCPFOwner.PatientItself,
                    Profession = "Psicóloga",
                    UrlIdentifier = StringHelper.GenerateUrlIdentifier("Laura Gonzaga Deniz"),
                    CreatedOn = DateTime.UtcNow
                },
                Doctor = doctor
            };
            patient.Person.Emails.Add(new Email() { Address = "lauragonzagadeniz@gmail.com" });
            patient.Person.Addresses.Add(new Address()
            {
                CEP = "600330250",
                StateProvince = "RJ",
                City = "Rio de Janeiro",
                Neighborhood = "Lapa",
                Street = "Rua Comendador Braga nº 890/1210",
                Complement = ""
            });

            db.Patients.AddObject(patient);

            // patient 3
            patient = new Patient()
            {
                Person = new Person()
                {
                    FullName = "Antonieta Moraes Sobrinho",
                    Gender = (int)TypeGender.Female,
                    DateOfBirth = new DateTime(1955, 12, 2),
                    MaritalStatus = (int)TypeMaritalStatus.Solteiro,
                    BirthPlace = "Brasileira",
                    CPF = "10472932188",
                    CPFOwner = (int)TypeCPFOwner.PatientItself,
                    Profession = "Funcionária Pública",
                    UrlIdentifier = "antonieta_moraes_sobrinho",
                    CreatedOn = DateTime.UtcNow
                },
                Doctor = doctor
            };

            patient.Person.Emails.Add(new Email() { Address = "lauragonzagadeniz@gmail.com" });
            patient.Person.Addresses.Add(new Address()
            {
                CEP = "600330250",
                StateProvince = "RJ",
                City = "Rio de Janeiro",
                Neighborhood = "Lapa",
                Street = "Rua Comendador Braga nº 890/1210",
                Complement = ""
            });

            db.Patients.AddObject(patient);
        }

        /// <summary>
        /// Aplica as configuraçoes de dados iniciais quando um médico é criado
        /// Este método deve ser MANTIDO
        /// </summary>
        /// <param name="doctor"></param>
        /// <param name="db"></param>
        public static void SetupDoctor(Doctor doctor, CerebelloEntities db)
        {
            doctor.CFG_Schedule = new CFG_Schedule()
            {
                AppointmentTime = 30,
                Sunday = false,
                Monday = true,
                Tuesday = true,
                Wednesday = true,
                Thursday = true,
                Friday = true,
                Saturday = false,
                MondayWorkdayStartTime = "09:00",
                MondayWorkdayEndTime = "18:00",
                MondayLunchStartTime = "12:00",
                MondayLunchEndTime = "13:00",
                TuesdayWorkdayStartTime = "09:00",
                TuesdayWorkdayEndTime = "18:00",
                TuesdayLunchStartTime = "12:00",
                TuesdayLunchEndTime = "13:00",
                WednesdayWorkdayStartTime = "09:00",
                WednesdayWorkdayEndTime = "18:00",
                WednesdayLunchStartTime = "12:00",
                WednesdayLunchEndTime = "13:00",
                ThursdayWorkdayStartTime = "09:00",
                ThursdayWorkdayEndTime = "18:00",
                ThursdayLunchStartTime = "12:00",
                ThursdayLunchEndTime = "13:00",
                FridayWorkdayStartTime = "09:00",
                FridayWorkdayEndTime = "18:00",
                FridayLunchStartTime = "12:00",
                FridayLunchEndTime = "13:00",
            };

            doctor.CFG_Documents = new CFG_Documents()
            {
                Header1 = doctor.UserPractice.User.Person.FullName,
                Header2 = "[Especialidade Médica]",
                FooterLeft1 = "[Endereço]",
                FooterLeft2 = "[Telefones]",
                FooterRight1 = "[Cidade]",
                FooterRight2 = "[CRM]"
            };
        }

        public static void SetupUserData(Doctor doctor, CerebelloEntities db)
        {
            doctor.CFG_Schedule = new CFG_Schedule()
            {
                AppointmentTime = 30,
                Sunday = false,
                Monday = true,
                Tuesday = true,
                Wednesday = true,
                Thursday = true,
                Friday = true,
                Saturday = false,
                MondayWorkdayStartTime = "09:00",
                MondayWorkdayEndTime = "18:00",
                MondayLunchStartTime = "12:00",
                MondayLunchEndTime = "13:00",
                TuesdayWorkdayStartTime = "09:00",
                TuesdayWorkdayEndTime = "18:00",
                TuesdayLunchStartTime = "12:00",
                TuesdayLunchEndTime = "13:00",
                WednesdayWorkdayStartTime = "09:00",
                WednesdayWorkdayEndTime = "18:00",
                WednesdayLunchStartTime = "12:00",
                WednesdayLunchEndTime = "13:00",
                ThursdayWorkdayStartTime = "09:00",
                ThursdayWorkdayEndTime = "18:00",
                ThursdayLunchStartTime = "12:00",
                ThursdayLunchEndTime = "13:00",
                FridayWorkdayStartTime = "09:00",
                FridayWorkdayEndTime = "18:00",
                FridayLunchStartTime = "12:00",
                FridayLunchEndTime = "13:00",
            };

            doctor.CFG_Documents = new CFG_Documents()
            {
                Header1 = doctor.UserPractice.User.Person.FullName,
                Header2 = "[Especialidade Médica]",
                FooterLeft1 = "[Endereço]",
                FooterLeft2 = "[Telefones]",
                FooterRight1 = "[Cidade]",
                FooterRight2 = "[CRM]"
            };
        }

        public static void SetupDB(CerebelloEntities db)
        {
            // System data

        }
    }
}
