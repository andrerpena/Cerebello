using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cerebello.Model;
using CerebelloWebRole.Models;
using CerebelloWebRole.Code;

namespace Cerebello.Firestarter
{
    public static class Firestarter
    {
        /// <summary>
        /// Crates a fake user, doctor and practice.
        /// </summary>
        public static Doctor CreateFakeUserAndPractice_1(CerebelloEntities db)
        {
            // Creating data infrastructure.
            var entity = CreateMedicalEntity_CrmMg(db);
            var specialty = CreateSpecialty_Psiquiatria(db);

            // Creating practice.
            var practice = CreatePractice_DrHouse(db);

            // Creating person, user and doctor objects to be used by André.
            var andre = CreateDoctor_Andre(db, entity, specialty, practice);

            return andre;
        }

        /// <summary>
        /// Crates a fake user, doctor and practice.
        /// </summary>
        public static List<Doctor> CreateFakeUserAndPractice_2(CerebelloEntities db)
        {
            // Creating data infrastructure.
            var entity = CreateMedicalEntity_CrmMg(db);
            var specialty = CreateSpecialty_Psiquiatria(db);

            // Creating practice.
            var practice = CreatePractice_DrHouse(db);

            // Creating person, user and doctor objects to be used by André.
            var result = new List<Doctor>
            {
                CreateDoctor_Andre(db, entity, specialty, practice),
                CreateAdministratorDoctor_Miguel(db, entity, specialty, practice),
            };

            return result;
        }

        public static MedicalSpecialty CreateSpecialty_Psiquiatria(CerebelloEntities db)
        {
            var specialty = new MedicalSpecialty()
            {
                Name = "Psiquiatria"
            };

            db.MedicalSpecialties.AddObject(specialty);
            return specialty;
        }

        public static MedicalEntity CreateMedicalEntity_CrmMg(CerebelloEntities db)
        {
            var entity = new MedicalEntity()
            {
                Name = "CRMMG"
            };

            db.MedicalEntities.AddObject(entity);
            return entity;
        }

        public static Doctor CreateAdministratorDoctor_Miguel(CerebelloEntities db, MedicalEntity entity, MedicalSpecialty specialty, Practice practice)
        {
            // Creating user.
            User user = new User()
            {
                UserName = "masbicudo",
                LastActiveOn = DateTime.UtcNow,
                Password = "IupHtucomYn3+1AlTL585GX3Ucs=", // pwd = "masban"
                PasswordSalt = "oHdC62UZE6Hwts91+Xy88Q==",
                Email = "masbicudo@gmail.com",
                GravatarEmailHash = "b209e81c82e45437da92af24ddc97360",
                Practice = practice
            };

            db.Users.AddObject(user);

            //db.SaveChanges(); // cannot save changes here, because user.Person is not nullable.

            // Creating person.
            Person person = new Person()
            {
                DateOfBirth = new DateTime(1984, 05, 04),
                FullName = "Phill Austin",
                UrlIdentifier = "phillaustin",
                Gender = (int)TypeGender.Male,
                CreatedOn = DateTime.UtcNow,
            };

            user.Person = person;

            db.SaveChanges();

            // Creating doctor.
            Doctor doctor = new Doctor()
            {
                Id = 2,
                CRM = "98765",
                MedicalSpecialty = specialty,
                MedicalEntity = entity
            };

            user.Doctor = doctor;

            db.SaveChanges();

            // Creating admin.
            Administrator admin = new Administrator()
            {
                Id = 2,
            };

            user.Doctor = doctor;

            db.SaveChanges();

            // Creating e-mail.
            user.Person.Emails.Add(new Email()
            {
                Address = "masbicudo@gmail.com"
            });

            db.SaveChanges();

            return doctor;
        }

        public static Doctor CreateDoctor_Andre(CerebelloEntities db, MedicalEntity entity, MedicalSpecialty specialty, Practice practice)
        {
            Person person = new Person()
            {
                DateOfBirth = new DateTime(1984, 08, 12),
                FullName = "Gregory House",
                UrlIdentifier = "gregoryhouse",
                Gender = (int)TypeGender.Male,
                CreatedOn = DateTime.UtcNow,
            };

            db.People.AddObject(person);

            db.SaveChanges();

            User user = new User()
            {
                UserName = "andrerpena",
                Person = person,
                LastActiveOn = DateTime.UtcNow,
                Password = "aThARLVPRzyS7yAb4WGDDsppzrA=",
                PasswordSalt = "nnKvjK+67w7OflE9Ri4MQw==",
                Email = "andrerpena@gmail.com",
                GravatarEmailHash = "574700aef74b21d386ba1250b77d20c6",
                Practice = practice
            };

            db.Users.AddObject(user);

            db.SaveChanges();

            Doctor doctor = new Doctor()
            {
                Id = 1,
                CRM = "12345",
                MedicalSpecialty = specialty,
                MedicalEntity = entity
            };

            db.Doctors.AddObject(doctor);

            db.SaveChanges();

            user.Doctor = doctor;
            user.Person.Emails.Add(new Email() { Address = "andrerpena@gmail.com" });

            db.SaveChanges();

            return doctor;
        }

        /// <summary>
        /// Creates the secretary Milena, with Id = 3.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="practice"></param>
        /// <returns></returns>
        public static Secretary CreateSecretary_Milena(CerebelloEntities db, Practice practice)
        {
            // Creating user.
            User user = new User()
            {
                UserName = "milena",
                LastActiveOn = DateTime.UtcNow,
                PasswordSalt = "egt/lzoRIw/M7XJsK3C0jw==", // pwd = "milena"
                Password = "TSRG03R6Atzl48oIPaaK20SiyKg=",
                Practice = practice
            };

            db.Users.AddObject(user);

            //db.SaveChanges(); // cannot save changes here, because user.Person is not nullable.

            // Creating person.
            Person person = new Person()
            {
                DateOfBirth = new DateTime(1984, 05, 04),
                FullName = "Menininha Santos",
                UrlIdentifier = "meninasantos",
                Gender = (int)TypeGender.Female,
                CreatedOn = DateTime.UtcNow,
            };

            user.Person = person;

            db.SaveChanges();

            // Creating secretary.
            Secretary secreatry = new Secretary()
            {
                Id = 3,
            };

            user.Secretary = secreatry;

            db.SaveChanges();

            return secreatry;
        }

        /// <summary>
        /// Creates a new practice and returns it.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static Practice CreatePractice_DrHouse(CerebelloEntities db)
        {
            var practice = new Practice()
            {
                Name = "Consultório do Dr. House",
                UrlIdentifier = "consultoriodrhourse",
                CreatedOn = DateTime.UtcNow
            };

            db.Practices.AddObject(practice);

            db.SaveChanges();
            return practice;
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
                Header1 = doctor.Users.First().Person.FullName,
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
                Header1 = doctor.Users.First().Person.FullName,
                Header2 = "[Especialidade Médica]",
                FooterLeft1 = "[Endereço]",
                FooterLeft2 = "[Telefones]",
                FooterRight1 = "[Cidade]",
                FooterRight2 = "[CRM]"
            };
        }

        public static void InitializeDatabaseWithSystemData(CerebelloEntities db)
        {
            // System data

        }

        /// <summary>
        /// Clears all data in the database.
        /// </summary>
        /// <param name="db"></param>
        public static void ClearAllData(CerebelloEntities db)
        {
            db.ExecuteStoreCommand(@"EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'");
            db.ExecuteStoreCommand(@"sp_MSForEachTable '
                         IF OBJECTPROPERTY(object_id(''?''), ''TableHasForeignRef'') = 1
                         DELETE FROM ?
                         else 
                         TRUNCATE TABLE ?
                     '");
            db.ExecuteStoreCommand(@"sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL'");
            db.ExecuteStoreCommand(@"sp_MSForEachTable ' 
                         IF OBJECTPROPERTY(object_id(''?''), ''TableHasIdentity'') = 1 
                         DBCC CHECKIDENT (''?'', RESEED, 0) 
                     ' ");
        }
    }
}
