using System;
using System.Collections.Generic;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Cerebello.Firestarter.Helpers;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Access;

namespace Cerebello.Firestarter
{
    public static class Firestarter
    {
        private const string DEFAULT_TIMEZONE_ID = "E. South America Standard Time";

        public static Random Random { get; set; }

        static Firestarter()
        {
            Random = new Random();
        }

        public static DateTime ConvertFromDefaultToUtc(DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById(DEFAULT_TIMEZONE_ID));
        }

        /// <summary>
        /// Crates a fake user, doctor and practice.
        /// </summary>
        public static Doctor Create_CrmMg_Psiquiatria_DrHouse_Andre(CerebelloEntities db, bool useTrialContract = true)
        {
            // Creating data infrastructure.
            var entity = GetMedicalEntity_Crm(db);
            var specialty = GetMedicalSpecialty_Psiquiatra(db);

            // Creating practice.
            var practice = CreatePractice_DrHouse(db, useTrialContract);

            // Creating person, user and doctor objects to be used by André.
            var andre = CreateAdministratorDoctor_Andre(db, entity, specialty, practice);

            // In this case, André is the owner of the account.
            var user = andre.Users.First();
            user.IsOwner = true;
            user.Practice.Owner = user;
            db.SaveChanges();

            return andre;
        }

        /// <summary>
        /// Crates a fake user, doctor and practice.
        /// </summary>
        public static List<Doctor> Create_CrmMg_Psiquiatria_DrHouse_Andre_Miguel(CerebelloEntities db, bool useTrialContract = true)
        {
            // Creating data infrastructure.
            var entity = GetMedicalEntity_Crm(db);
            var specialty = GetMedicalSpecialty_Psiquiatra(db);

            // Creating practice.
            var practice = CreatePractice_DrHouse(db, useTrialContract);

            // Creating person, user and doctor objects to be used by André.
            var andre = CreateAdministratorDoctor_Andre(db, entity, specialty, practice);
            var miguel = CreateAdministratorDoctor_Miguel(db, entity, specialty, practice);
            var result = new List<Doctor> { andre, miguel, };

            // In this case, André is the owner of the account.
            var user = andre.Users.First();
            user.IsOwner = true;
            user.Practice.Owner = user;
            db.SaveChanges();

            return result;
        }

        /// <summary>
        /// Crates a fake user, doctor and practice.
        /// </summary>
        public static List<Doctor> Create_CrmMg_Psiquiatria_DrHouse_Andre_Miguel_Thomas(CerebelloEntities db, bool useTrialContract = true)
        {
            // Creating data infrastructure.
            var entity = GetMedicalEntity_Crm(db);
            var specialty = GetMedicalSpecialty_Psiquiatra(db);

            // Creating practice.
            var practice = CreatePractice_DrHouse(db, useTrialContract);

            // Creating person, user and doctor objects to be used by André.
            var andre = CreateAdministratorDoctor_Andre(db, entity, specialty, practice);
            var miguel = CreateAdministratorDoctor_Miguel(db, entity, specialty, practice);
            var thomas = CreateDoctor_ThomasGray(db, entity, specialty, practice);
            var result = new List<Doctor> { andre, miguel, thomas, };

            // In this case, André is the owner of the account.
            var user = andre.Users.First();
            user.IsOwner = true;
            user.Practice.Owner = user;
            db.SaveChanges();

            return result;
        }

        /// <summary>
        /// Crates a fake user, doctor and practice.
        /// </summary>
        public static Doctor Create_CrmMg_Psiquiatria_DraMarta_Marta(CerebelloEntities db, bool useTrialContract = true)
        {
            // Creating data infrastructure.
            var entity = GetMedicalEntity_Crm(db);
            var specialty = GetMedicalSpecialty_Psiquiatra(db);

            // Creating practice.
            var practice = CreatePractice_DraMarta(db, useTrialContract);

            var marta = CreateDoctor_MartaCura(db, entity, specialty, practice);

            return marta;
        }

        public static SYS_MedicalSpecialty GetMedicalSpecialty_Psiquiatra(CerebelloEntities db)
        {
            var result = db.SYS_MedicalSpecialty.Single(s => s.Name == "Psiquiatra");
            return result;
        }

        public static SYS_MedicalSpecialty GetMedicalSpecialty_Fonoaudiólogo(CerebelloEntities db)
        {
            var result = db.SYS_MedicalSpecialty.Single(s => s.Name == "Fonoaudiólogo");
            return result;
        }

        public static SYS_MedicalEntity GetMedicalEntity_Crm(CerebelloEntities db)
        {
            return db.SYS_MedicalEntity.Single(me => me.Code == "CRM");
        }

        public static SYS_MedicalEntity GetMedicalEntity_Psicologia(CerebelloEntities db)
        {
            return db.SYS_MedicalEntity.Single(me => me.Code == "CRP");
        }

        public static SYS_MedicalEntity GetMedicalEntity_Fono(CerebelloEntities db)
        {
            return db.SYS_MedicalEntity.Single(me => me.Code == "CRFA");
        }

        /// <summary>
        /// Returns UTC date/time plus the debug time offset.
        /// </summary>
        public static DateTime UtcNow
        {
            get { return DateTime.UtcNow + DebugConfig.CurrentTimeOffset; }
        }

        /// <summary>
        /// Returns current system date/time plus the debug time offset.
        /// </summary>
        public static DateTime Now
        {
            get { return DateTime.Now + DebugConfig.CurrentTimeOffset; }
        }

        public static Doctor CreateAdministratorDoctor_Miguel(
            CerebelloEntities db,
            SYS_MedicalEntity entity,
            SYS_MedicalSpecialty specialty,
            Practice practice,
            bool useDefaultPassword = false)
        {
            const string pwdSalt = "oHdC62UZE6Hwts91+Xy88Q==";
            var pwdHash = CipherHelper.Hash("masban", pwdSalt);
            if (useDefaultPassword)
                pwdHash = CipherHelper.Hash(CerebelloWebRole.Code.Constants.DEFAULT_PASSWORD, pwdSalt);

            // Creating user.
            var user = new User()
                {
                    UserName = "masbicudo",
                    UserNameNormalized = "masbicudo",
                    LastActiveOn = Firestarter.UtcNow,
                    Password = pwdHash,
                    PasswordSalt = pwdSalt,
                    Practice = practice,
                };

            db.Users.AddObject(user);

            //db.SaveChanges(); // cannot save changes here, because user.Person is not nullable.

            // Creating person.
            var person = new Person()
                {
                    DateOfBirth = ConvertFromDefaultToUtc(new DateTime(1984, 05, 04)),
                    FullName = "Júlio Cezar Almeida",
                    Gender = (int)TypeGender.Male,
                    CreatedOn = Firestarter.UtcNow,
                    Email = "masbicudo@gmail.com",
                    EmailGravatarHash = GravatarHelper.GetGravatarHash("masbicudo@gmail.com"),
                    PracticeId = practice.Id,
                };

            user.Person = person;

            db.SaveChanges();

            // Creating doctor.
            var doctor = new Doctor()
                {
                    Id = 2,
                    CRM = "98765",
                    MedicalSpecialtyCode = specialty.Code,
                    MedicalSpecialtyName = specialty.Name,
                    MedicalEntityCode = entity.Code,
                    MedicalEntityName = entity.Name,
                    MedicalEntityJurisdiction = "MG",
                    UrlIdentifier = "phillaustin",
                    PracticeId = practice.Id,
                };

            BusHelper.FillNewDoctorUtilityBelt(doctor);

            user.Doctor = doctor;

            db.SaveChanges();

            // Creating admin.
            Administrator admin = new Administrator()
                {
                    Id = 2,
                    PracticeId = practice.Id,
                };

            user.Administrator = admin;

            db.SaveChanges();

            // Creating e-mail.
            user.Person.Email = "masbicudo@gmail.com";

            db.SaveChanges();

            return doctor;
        }

        public static Doctor CreateAdministratorDoctor_Andre(
            CerebelloEntities db, SYS_MedicalEntity entity, SYS_MedicalSpecialty specialty, Practice practice)
        {
            var person = new Person()
                {
                    DateOfBirth = ConvertFromDefaultToUtc(new DateTime(1984, 08, 12)),
                    FullName = "André Pena",
                    Gender = (int)TypeGender.Male,
                    CreatedOn = Firestarter.UtcNow,
                    Email = "andrerpena@gmail.com",
                    EmailGravatarHash = GravatarHelper.GetGravatarHash("andrerpena@gmail.com"),
                    PracticeId = practice.Id,
                };

            db.People.AddObject(person);

            db.SaveChanges();

            var user = new User()
                {
                    UserName = "andrerpena",
                    UserNameNormalized = "andrerpena",
                    Person = person,
                    LastActiveOn = Firestarter.UtcNow,
                    Password = "aThARLVPRzyS7yAb4WGDDsppzrA=",
                    PasswordSalt = "nnKvjK+67w7OflE9Ri4MQw==",
                    Practice = practice,
                };

            db.Users.AddObject(user);

            db.SaveChanges();

            var doctor = new Doctor()
                {
                    CRM = "12345",
                    MedicalSpecialtyCode = specialty.Code,
                    MedicalSpecialtyName = specialty.Name,
                    MedicalEntityCode = entity.Code,
                    MedicalEntityName = entity.Name,
                    MedicalEntityJurisdiction = "MG",
                    UrlIdentifier = "gregoryhouse",
                    PracticeId = practice.Id,
                };

            BusHelper.FillNewDoctorUtilityBelt(doctor);

            db.Doctors.AddObject(doctor);

            db.SaveChanges();

            user.Doctor = doctor;
            user.Person.Email = "andrerpena@gmail.com";

            db.SaveChanges();

            var admin = new Administrator
                {
                    PracticeId = practice.Id,
                };
            user.Administrator = admin;

            db.SaveChanges();

            return doctor;
        }

        public static Doctor CreateDoctor_MartaCura(
            CerebelloEntities db, SYS_MedicalEntity entity, SYS_MedicalSpecialty specialty, Practice practice)
        {
            Person person = new Person()
                {
                    DateOfBirth = ConvertFromDefaultToUtc(new DateTime(1967, 04, 20)),
                    FullName = "Marta Cura",
                    Gender = (int)TypeGender.Female,
                    CreatedOn = Firestarter.UtcNow,
                    Email = "martacura@fakemail.com",
                    EmailGravatarHash = GravatarHelper.GetGravatarHash("martacura@fakemail.com"),
                    PracticeId = practice.Id,
                };

            db.People.AddObject(person);

            db.SaveChanges();

            User user = new User()
                {
                    UserName = "martacura",
                    UserNameNormalized = "martacura",
                    Person = person,
                    LastActiveOn = Firestarter.UtcNow,
                    PasswordSalt = "ELc81TnRE+Eb+e5/D69opg==",
                    Password = "lLqJ7FjmEQF7q4rxWIGnX+AXdqQ=",

                    Practice = practice,
                };

            db.Users.AddObject(user);

            db.SaveChanges();

            Doctor doctor = new Doctor()
                {
                    Id = 4,
                    CRM = "74653",
                    MedicalSpecialtyCode = specialty.Code,
                    MedicalSpecialtyName = specialty.Name,
                    MedicalEntityCode = entity.Code,
                    MedicalEntityName = entity.Name,
                    MedicalEntityJurisdiction = "MG",
                    UrlIdentifier = "martacura",
                    PracticeId = practice.Id,
                };

            BusHelper.FillNewDoctorUtilityBelt(doctor);

            db.Doctors.AddObject(doctor);

            db.SaveChanges();

            user.Doctor = doctor;

            db.SaveChanges();

            return doctor;
        }

        public static Doctor CreateDoctor_ThomasGray(
            CerebelloEntities db, SYS_MedicalEntity entity, SYS_MedicalSpecialty specialty, Practice practice)
        {
            var person = new Person
            {
                DateOfBirth = ConvertFromDefaultToUtc(new DateTime(1967, 04, 20)),
                FullName = "Miguel Angelo Santos",
                Gender = (int)TypeGender.Male,
                CreatedOn = Firestarter.UtcNow,
                Email = "thomasgray@fakemail.com",
                EmailGravatarHash = GravatarHelper.GetGravatarHash("thomasgray@fakemail.com"),
                PracticeId = practice.Id,
            };

            db.People.AddObject(person);

            db.SaveChanges();

            var user = new User()
            {
                UserName = "thomasgray",
                UserNameNormalized = "thomasgray",
                Person = person,
                LastActiveOn = Firestarter.UtcNow,
                PasswordSalt = "DAGjFT7iMXxfJQAFYFRa+w==",
                Password = "39ltFvGLs/oC71jd1ngWnTzar2A=", // pwd: 'tgray'
                Practice = practice,
            };

            db.Users.AddObject(user);

            db.SaveChanges();

            var doctor = new Doctor()
            {
                Id = 4,
                CRM = "102938",
                MedicalSpecialtyCode = specialty.Code,
                MedicalSpecialtyName = specialty.Name,
                MedicalEntityCode = entity.Code,
                MedicalEntityName = entity.Name,
                MedicalEntityJurisdiction = "RJ",
                UrlIdentifier = "thomasgray",
                PracticeId = practice.Id,
            };

            BusHelper.FillNewDoctorUtilityBelt(doctor);

            db.Doctors.AddObject(doctor);

            db.SaveChanges();

            user.Doctor = doctor;

            db.SaveChanges();

            return doctor;
        }

        /// <summary>
        /// Creates the secretary Milena.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="practice"></param>
        /// <returns></returns>
        public static Secretary CreateSecretary_Milena(CerebelloEntities db, Practice practice, bool useDefaultPassword = false)
        {
            var pwdSalt = "egt/lzoRIw/M7XJsK3C0jw==";
            var pwdHash = CipherHelper.Hash("milena", pwdSalt);
            if (useDefaultPassword)
                pwdHash = CipherHelper.Hash(Constants.DEFAULT_PASSWORD, pwdSalt);

            // Creating user.
            User user = new User()
                {
                    UserName = "milena",
                    UserNameNormalized = "milena",
                    LastActiveOn = Firestarter.UtcNow,
                    PasswordSalt = pwdSalt,
                    Password = pwdHash,
                    Practice = practice,
                };

            db.Users.AddObject(user);

            // Creating person.
            Person person = new Person()
                {
                    DateOfBirth = ConvertFromDefaultToUtc(new DateTime(1984, 05, 04)),
                    FullName = "Milena Santos",
                    Gender = (int)TypeGender.Female,
                    CreatedOn = Firestarter.UtcNow,
                    PracticeId = practice.Id,
                };

            user.Person = person;

            db.SaveChanges();

            // Creating secretary.
            Secretary secreatry = new Secretary
                {
                    PracticeId = practice.Id,
                };

            user.Secretary = secreatry;

            db.SaveChanges();

            return secreatry;
        }

        /// <summary>
        /// Creates the secretary Maricleusa.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="practice"></param>
        /// <returns></returns>
        public static Secretary CreateSecretary_Maricleusa(CerebelloEntities db, Practice practice, bool useDefaultPassword = false)
        {
            var pwdSalt = "HC7p4NIf+1JYZmndKMggog==";
            var pwdHash = CipherHelper.Hash("mary123", pwdSalt);
            if (useDefaultPassword)
                pwdHash = CipherHelper.Hash(CerebelloWebRole.Code.Constants.DEFAULT_PASSWORD, pwdSalt);

            // Creating user.
            User user = new User()
                {
                    UserName = "maricleusa",
                    UserNameNormalized = "maricleusa",
                    LastActiveOn = Firestarter.UtcNow,
                    PasswordSalt = pwdSalt,
                    Password = pwdHash,
                    Practice = practice,
                };

            db.Users.AddObject(user);

            // Creating person.
            Person person = new Person()
                {
                    DateOfBirth = ConvertFromDefaultToUtc(new DateTime(1974, 10, 12)),
                    FullName = "Maricleusa Souza",
                    Gender = (int)TypeGender.Female,
                    CreatedOn = Firestarter.UtcNow,
                    PracticeId = practice.Id,
                };

            user.Person = person;

            db.SaveChanges();

            // Creating secretary.
            Secretary secreatry = new Secretary
                {
                    Id = 4,
                    PracticeId = practice.Id,
                };

            user.Secretary = secreatry;

            db.SaveChanges();

            return secreatry;
        }

        /// <summary>
        /// Creates a new practice and returns it.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="contract"></param>
        /// <returns></returns>
        public static Practice CreatePractice_DrHouse(CerebelloEntities db, bool useTrialContract = true)
        {
            var practice = new Practice()
                {
                    Name = "Consultório do Dr. House",
                    UrlIdentifier = "consultoriodrhouse",
                    CreatedOn = new DateTime(2007, 07, 03, 0, 0, 0, DateTimeKind.Utc),
                    WindowsTimeZoneId = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time").Id,
                    VerificationDate = new DateTime(2007, 07, 12, 0, 0, 0, DateTimeKind.Utc),
                };

            db.Practices.AddObject(practice);

            db.SaveChanges();

            if (useTrialContract)
                SetupPracticeWithTrialContract(db, practice);

            return practice;
        }

        /// <summary>
        /// Creates a new practice and returns it.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="useTrialContract"></param>
        /// <returns></returns>
        public static Practice CreatePractice_DraMarta(CerebelloEntities db, bool useTrialContract = true)
        {
            var practice = new Practice()
                {
                    Name = "Consultório da Dra. Marta",
                    UrlIdentifier = "dramarta",
                    CreatedOn = new DateTime(2009, 02, 27, 0, 0, 0, DateTimeKind.Utc),
                    WindowsTimeZoneId = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time").Id,
                    VerificationDate = new DateTime(2009, 02, 27, 0, 0, 0, DateTimeKind.Utc),
                };

            db.Practices.AddObject(practice);

            db.SaveChanges();

            if (useTrialContract)
                SetupPracticeWithTrialContract(db, practice);

            return practice;
        }

        /// <summary>
        /// Sets up a practice's contract with a default trial contract.
        /// </summary>
        /// <param name="db"> Data context to create the account contract with. </param>
        /// <param name="practice"> Practice that owns the account contract. </param>
        /// <returns> The created AccountContract. </returns>
        private static AccountContract SetupPracticeWithTrialContract(CerebelloEntities db, Practice practice)
        {
            var accountContract = practice.AccountContract = new AccountContract
                {
                    Practice = practice,

                    ContractTypeId = (int)ContractTypes.TrialContract,
                    IsTrial = true,
                    IssuanceDate = new DateTime(2012, 01, 25),
                    StartDate = new DateTime(2012, 02, 10),
                    EndDate = null,

                    DoctorsLimit = null,
                    PatientsLimit = 50,

                    // no billings
                    BillingAmount = null,
                    BillingDueDay = null,
                    BillingPaymentMethod = null,
                    BillingPeriodCount = null,
                    BillingPeriodSize = null,
                    BillingPeriodType = null,
                    BillingDiscountAmount = null,
                };

            practice.AccountContract.CustomText = StringHelper.ReflectionReplace(
                practice.AccountContract.SYS_ContractType.CustomTemplateText,
                practice.AccountContract);

            db.AccountContracts.AddObject(accountContract);

            db.SaveChanges();

            return accountContract;
        }

        /// <summary>
        /// Creates fake patients
        /// </summary>
        public static List<Patient> CreateFakePatients(Doctor doctor, CerebelloEntities db, int count = 40)
        {
            return FakePatientsFactory.CreateFakePatients(doctor, db, count);
        }

        /// <summary>
        /// Applies the initial configuration when a doctor is created.
        /// This method must be kept.
        /// </summary>
        /// <param name="doctor"></param>
        /// <param name="db"></param>
        /// <param name="seed"> </param>
        public static void SetupDoctor(Doctor doctor, CerebelloEntities db, int seed = 7)
        {
            #region CFG_Schedule

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

                    PracticeId = doctor.PracticeId,
                };

            #endregion

            db.SaveChanges();

            #region CFG_Documents

            doctor.CFG_Documents = new CFG_Documents()
                {
                    Header1 = doctor.Users.First().Person.FullName,
                    Header2 = "[Especialidade Médica]",
                    FooterLeft1 = "[Endereço]",
                    FooterLeft2 = "[Telefones]",
                    FooterRight1 = "[Cidade]",
                    FooterRight2 = "[CRM]",

                    PracticeId = doctor.PracticeId,
                };

            #endregion

            db.SaveChanges();

            CreateFakeHealthInsurances(db, doctor, seed);
        }

        public static Appointment CreateFakeAppointment(
            CerebelloEntities db, DateTime createdOn, Doctor doc, DateTime start, TimeSpan duration, string desc, User creator = null)
        {
            creator = creator ?? doc.Users.First();

            Appointment result;

            db.Appointments.AddObject(
                result = new Appointment
                    {
                        CreatedById = creator.Id,
                        CreatedOn = createdOn,
                        DoctorId = doc.Id,
                        Start = start,
                        End = start + duration,
                        Description = desc,
                        Type = (int)TypeAppointment.GenericAppointment,
                        HealthInsuranceId =
                            (doc.HealthInsurances.FirstOrDefault(hi => hi.IsActive)
                             ?? doc.HealthInsurances.First(hi => !hi.IsActive)).Id,

                        PracticeId = doc.PracticeId,
                    });

            db.SaveChanges();

            return result;
        }

        public static Appointment CreateFakeAppointment(
            CerebelloEntities db, DateTime createdOn, Doctor doc, DateTime start, TimeSpan duration, Patient patient, User creator = null)
        {
            creator = creator ?? doc.Users.First();

            Appointment result;

            db.Appointments.AddObject(
                result = new Appointment
                    {
                        CreatedById = creator.Id,
                        CreatedOn = createdOn,
                        DoctorId = doc.Id,
                        Start = start,
                        End = start + duration,
                        Patient = patient,
                        Type = (int)TypeAppointment.MedicalAppointment,
                        HealthInsurance = doc.HealthInsurances.First(),

                        PracticeId = doc.PracticeId,
                    });

            db.SaveChanges();

            return result;
        }

        public static void Initialize_SYS_Cid10(CerebelloEntities db, int maxCount = int.MaxValue, Action<int, int> progress = null)
        {
            // read CID10.xml as an embedded resource
            XmlReaderSettings settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse };
            XDocument xmlDocument = null;

            using (var xmlFileStream = new FileStream(@"CID10.xml", FileMode.Open, FileAccess.Read))
            using (var reader = XmlReader.Create(xmlFileStream, settings))
                xmlDocument = XDocument.Load(reader);

            var cid10Nodes = (from e in xmlDocument.Descendants()
                              where
                                  e.Name == "nome" &&
                                  (e.Parent.Attribute("codcat") != null ||
                                   e.Parent.Attribute("codsubcat") != null)
                              select new
                                  {
                                      Name = e.Value,
                                      CodCat =
                                  e.Parent.Attribute("codcat") != null
                                      ? e.Parent.Attribute("codcat").Value
                                      : null,
                                      CodSubCat =
                                  e.Parent.Attribute("codsubcat") != null
                                      ? e.Parent.Attribute("codsubcat").Value
                                      : null
                                  }).ToList();

            var max = Math.Min(maxCount, cid10Nodes.Count);

            var count = 0;
            foreach (var entry in cid10Nodes)
            {
                if (count >= maxCount)
                    break;

                if (progress != null) progress(count, max);

                db.SYS_Cid10.AddObject(
                    new SYS_Cid10()
                        {
                            Name = entry.Name,
                            Cat = entry.CodCat,
                            SubCat = entry.CodSubCat
                        });

                if (count % 100 == 0)
                    db.SaveChanges();

                count++;
            }

            if (progress != null) progress(count, max);

            db.SaveChanges();
        }

        public static void Initialize_SYS_Contracts(CerebelloEntities db)
        {
            // Contratos de uso para o cerebello.com.br
            db.SYS_ContractType.AddObject(
                new SYS_ContractType
                    {
                        Id = (int)ContractTypes.TrialContract,
                        CreatedOn = new DateTime(2012, 09, 18),
                        IsTrial = true,
                        Name = "Contrato de teste",
                        UrlIdentifier = "TrialContract",
                    });

            db.SYS_ContractType.AddObject(
                new SYS_ContractType
                    {
                        Id = (int)ContractTypes.ProfessionalContract,
                        CreatedOn = new DateTime(2012, 09, 18),
                        IsTrial = false,
                        Name = "Contrato de assinatura do plano Profissional Básico",
                        UrlIdentifier = ContractTypes.ProfessionalContract.ToString(),
                    });

            db.SaveChanges();
        }

        public static void Initialize_SYS_MedicalEntity(CerebelloEntities db)
        {
            // Conselhos profissionais, segundo o TISS - Tabelas de domínio (Versão 2.02.03)
            var tissConselhoProfissional = new ListOfTuples<string, string>
                {
                    { "CRAS", "Conselho Regional de Assistência Social" },
                    { "COREN", "Conselho Federal de Enfermagem" },
                    { "CRF", "Conselho Regional de Farmácia" },
                    { "CRFA", "Conselho Regional de Fonoaudiologia" },
                    { "CREFITO", "Conselho Regional de Fisioterapia e Terapia Ocupacional" },
                    { "CRM", "Conselho Regional de Medicina" },
                    { "CRV", "Conselho Regional de Medicina Veterinária" },
                    { "CRN", "Conselho Regional de Nutrição" },
                    { "CRO", "Conselho Regional de Odontologia" },
                    { "CRP", "Conselho Regional de Psicologia" },
                    { "OUT", "Outros Conselhos" },
                };

            foreach (var eachTuple in tissConselhoProfissional)
                db.SYS_MedicalEntity.AddObject(new SYS_MedicalEntity { Code = eachTuple.Item1, Name = eachTuple.Item2 });

            db.SaveChanges();
        }

        public static void Initialize_SYS_MedicalSpecialty(CerebelloEntities db)
        {

            // Especialidades - CBO-S TISS - Tabelas de domínio (Versão 2.02.03)
            var tissEspecialidades = new ListOfTuples<string, string>
                {
                    {"1312.05", "Diretor clínico"},
                    {"1312.05", "Diretor de departamento de saúde"},
                    {"1312.05", "Diretor de divisão médica"},
                    {"1312.05", "Diretor de serviços de saúde"},
                    {"1312.05", "Diretor de serviços médicos"},
                    {"1312.05", "Diretor de unidade assistencial"},
                    {"1312.05", "Diretor de unidade de saúde"},
                    {"1312.05", "Diretor de unidade hospitalar"},
                    {"1312.05", "Diretor médico-hospitalar"},
                    {"1312.10", "Administrador de ambulatório"},
                    {"1312.10", "Gerente de ambulatório"},
                    {"1312.10", "Gerente de enfermagem"},
                    {"1312.10", "Gerente de nutrição em unidades de saúde"},
                    {"1312.10", "Gerente de serviços de saúde"},
                    {"1311.20", "Gerente de serviços sociais"},
                    {"2011", "PROFISSIONAIS DA BIOTECNOLOGIA"},
                    {"2011.15", "Geneticista"},
                    {"2030.10", "Entomologista"},
                    {"2030.10", "Entomólogo"},
                    {"2030.10", "Ofiologista"},
                    {"2030.10", "Ornitólogo"},
                    {"2033.10", "Pesquisador de medicina básica"},
                    {"2030.15", "Bacteriologista"},
                    {"2030.20", "Fisiologista (exceto médico)"},
                    {"2030.25", "Fenologista"},
                    {"2131.50", "Físico hospitalar"},
                    {"2131.50", "Físico médico"},
                    {"2211.05", "Biologista"},
                    {"2211.05", "Biomédico"},
                    {"2231", "MÉDICOS"},
                    {"2232", "CIRURGIÕES-DENTISTAS"},
                    {"2231.01", "Médico acupunturista"},
                    {"2231.02", "Médico alergista"},
                    {"2231.02", "Médico alergista e imunologista"},
                    {"2231.02", "Médico imunologista"},
                    {"2231.03", "Médico anatomopatologista"},
                    {"2231.03", "Patologista"},
                    {"2231.04", "Anestesiologista"},
                    {"2231.04", "Anestesista"},
                    {"2235", "ENFERMEIROS"},
                    {"2231.04", "Médico anestesiologista"},
                    {"2231.04", "Médico anestesista"},
                    {"2231.05", "Angiologista"},
                    {"2232.04", "Cirurgião dentista - auditor"},
                    {"2231.05", "Médico angiologista"},
                    {"2236", "PROFISSIONAIS DA FISIOTERAPIA E AFINS"},
                    {"2231.06", "Cardiologista"},
                    {"2231.06", "Médico cardiologista"},
                    {"2231.06", "Médico do coração"},
                    {"2237", "NUTRICIONISTAS"},
                    {"2231.07", "Cirurgião cardiovascular"},
                    {"2238", "FONOAUDIÓLOGOS"},
                    {"2231.07", "Médico cirurgião cardiovascular"},
                    {"2233.05", "Médico veterinário"},
                    {"2233.05", "Médico veterinário de saúde pública"},
                    {"2233.05", "Médico veterinário sanitarista"},
                    {"2231.08", "Cirurgião de cabeça e pescoço"},
                    {"2234.05", "Farmacêutico"},
                    {"2234.05", "Farmacêutico homeopata"},
                    {"2234.05", "Farmacêutico hospitalar"},
                    {"2231.08", "Médico cirurgião de cabeça e pescoço"},
                    {"2232.08", "Cirurgião dentista - clínico geral"},
                    {"2231.09", "Cirurgião do aparelho digestivo"},
                    {"2231.09", "Cirurgião gastroenterológico"},
                    {"2232.08", "Dentista"},
                    {"2235.05", "Enfermeiro"},
                    {"2231.09", "Médico cirurgião do aparelho digestivo"},
                    {"2232.08", "Odontologista"},
                    {"2232.08", "Odontólogo"},
                    {"2231.10", "Cirurgião geral"},
                    {"2236.05", "Fisioterapeuta"},
                    {"2236.05", "Fisioterapeuta acupunturista"},
                    {"2231.10", "Médico cirurgião"},
                    {"2231.10", "Médico cirurgião geral"},
                    {"2237.05", "Auxiliar de dietista"},
                    {"2237.05", "Auxiliar de nutrição e dietética"},
                    {"2231.11", "Cirurgião pediátrico"},
                    {"2231.11", "Médico cirurgião pediátrico"},
                    {"2231.12", "Cirurgião plástico"},
                    {"2231.12", "Médico cirurgião plástico"},
                    {"2232.12", "Cirurgião dentista - endodontista"},
                    {"2231.13", "Cirurgião torácico"},
                    {"2231.13", "Médico cirurgião torácico"},
                    {"2232.12", "Odontólogo-endodontista"},
                    {"2235.10", "Enfermeiro auditor"},
                    {"2231.14", "Médico citopatologista"},
                    {"2241.05", "Avaliador físico"},
                    {"2231.15", "Clínico geral"},
                    {"2231.15", "Médico clínico"},
                    {"2231.15", "Médico clínico geral"},
                    {"2231.15", "Médico especialista em clínica médica"},
                    {"2231.15", "Médico especialista em medicina interna"},
                    {"2231.15", "Médico internista"},
                    {"2231.16", "Médico comunitário"},
                    {"2231.16", "Médico de"},
                    {"2231.16", "Médico de saúde da família"},
                    {"2237.10", "Nutricionista"},
                    {"2237.10", "Nutricionista (saúde pública)"},
                    {"2232.16", "Cirurgião dentista - epidemiologista"},
                    {"2231.17", "Dermatologista"},
                    {"2238.10", "Fonoaudiólogo"},
                    {"2231.17", "Hansenólogo"},
                    {"2231.17", "Médico dermatologista"},
                    {"2231.18", "Médico do trabalho"},
                    {"2235.15", "Enfermeiro de bordo"},
                    {"2231.19", "Médico em eletroencefalografia"},
                    {"2231.20", "Médico em endoscopia"},
                    {"2231.20", "Médico endoscopista"},
                    {"2236.15", "Ortoptista"},
                    {"2232.20", "Cirurgião dentista - estomatologista"},
                    {"2231.21", "Médico do tráfego"},
                    {"2231.21", "Médico em medicina de tráfego"},
                    {"2231.22", "Intensivista"},
                    {"2231.22", "Médico em medicina intensiva"},
                    {"2231.23", "Médico em medicina nuclear"},
                    {"2231.23", "Médico nuclear"},
                    {"2235.20", "Enfermeiro de centro cirúrgico"},
                    {"2231.24", "Imagenologista"},
                    {"2235.20", "Instrumentador cirúrgico (enfermeiro)"},
                    {"2231.24", "Médico angioradiologista"},
                    {"2231.24", "Médico densitometrista"},
                    {"2231.24", "Médico em diagnóstico por imagem"},
                    {"2231.24", "Médico em radiologia e diagnóstico por imagem"},
                    {"2231.24", "Médico neuroradiologista"},
                    {"2231.24", "Médico radiologista"},
                    {"2231.24", "Médico radiologista intervencionista"},
                    {"2231.24", "Radiologista"},
                    {"2231.24", "Ultra-sonografista"},
                    {"2232.24", "Cirurgião dentista - implantodontista"},
                    {"2231.25", "Médico endocrinologista"},
                    {"2231.25", "Médico endocrinologista e metabologista"},
                    {"2231.25", "Médico metabolista"},
                    {"2231.25", "Metabolista"},
                    {"2231.25", "Metabologista"},
                    {"2236.20", "Peripatologista"},
                    {"2236.20", "Terapeuta ocupacional"},
                    {"2231.26", "Fisiatra"},
                    {"2231.26", "Médico fisiatra"},
                    {"2231.27", "Foniatra"},
                    {"2231.27", "Médico foniatra"},
                    {"2231.28", "Médico gastroenterologista"},
                    {"2232.28", "Cirurgião dentista - odontogeriatra"},
                    {"2232.28", "Dentista de idosos"},
                    {"2232.28", "Dentista de terceira idade"},
                    {"2235.25", "Enfermeiro de terapia intensiva"},
                    {"2235.25", "Enfermeiro intensivista"},
                    {"2231.29", "Médico alopata"},
                    {"2231.29", "Médico em medicina interna"},
                    {"2231.29", "Médico generalista"},
                    {"2231.29", "Médico militar"},
                    {"2231.30", "Médico geneticista"},
                    {"2231.31", "Geriatra"},
                    {"2231.31", "Gerontologista"},
                    {"2231.31", "Gerontólogo"},
                    {"2231.31", "Médico geriatra"},
                    {"2231.32", "Cirurgião ginecológico"},
                    {"2231.32", "Ginecologista"},
                    {"2231.32", "Médico de mulheres"},
                    {"2231.32", "Médico ginecologista"},
                    {"2231.32", "Médico ginecologista e obstetra"},
                    {"2231.32", "Médico obstetra"},
                    {"2232.32", "Cirurgião dentista - odontologista legal"},
                    {"2231.33", "Hematologista"},
                    {"2231.33", "Médico hematologista"},
                    {"2235.30", "Enfermeiro do trabalho"},
                    {"2231.34", "Hemoterapeuta"},
                    {"2231.34", "Médico em hemoterapia"},
                    {"2231.34", "Médico hemoterapeuta"},
                    {"2231.35", "Médico homeopata"},
                    {"2231.36", "Infectologista"},
                    {"2231.36", "Médico de doenças infecciosas e parasitárias"},
                    {"2231.36", "Médico infectologista"},
                    {"2232.36", "Cirurgião dentista - odontopediatra"},
                    {"2232.36", "Dentista de criança"},
                    {"2231.37", "Médico legista"},
                    {"2232.36", "Odontopediatra"},
                    {"2231.38", "Cirurgião de mama"},
                    {"2231.38", "Cirurgião mastologista"},
                    {"2231.38", "Mastologista"},
                    {"2231.38", "Médico mastologista"},
                    {"2235.35", "Enfermeiro nefrologista"},
                    {"2231.39", "Médico nefrologista"},
                    {"2231.40", "Médico neurocirurgião"},
                    {"2231.40", "Médico neurocirurgião pediátrico"},
                    {"2231.40", "Neurocirurgião"},
                    {"2231.40", "Neurocirurgião pediátrico"},
                    {"2232.40", "Cirurgião dentista - ortopedista e ortodontista"},
                    {"2232.40", "Dentista de aparelho"},
                    {"2231.41", "Médico neurofisiologista"},
                    {"2231.41", "Neurofisiologista"},
                    {"2232.40", "Ortodontista"},
                    {"2232.40", "Ortodontólogo"},
                    {"2232.40", "Ortopedista maxilar"},
                    {"2231.42", "Médico neurologista"},
                    {"2231.42", "Médico neuropediatra"},
                    {"2231.42", "Neurologista"},
                    {"2231.42", "Neuropediatra"},
                    {"2231.43", "Médico nutrologista"},
                    {"2231.43", "Médico nutrólogo"},
                    {"2231.43", "Nutrologista"},
                    {"2231.44", "Cirurgião oftalmológico"},
                    {"2235.40", "Enfermeiro de berçário"},
                    {"2235.40", "Enfermeiro neonatologista"},
                    {"2231.44", "Médico oftalmologista"},
                    {"2231.44", "Oftalmologista"},
                    {"2232.44", "Cirurgião dentista - patologista bucal"},
                    {"2231.45", "Médico cancerologista"},
                    {"2231.45", "Médico oncologista"},
                    {"2231.45", "Oncologista"},
                    {"2231.46", "Cirurgião de mão"},
                    {"2231.46", "Cirurgião ortopedista"},
                    {"2231.46", "Cirurgião traumatologista"},
                    {"2231.46", "Médico cirurgião de mão"},
                    {"2231.46", "Médico de medicina esportiva"},
                    {"2231.46", "Médico ortopedista"},
                    {"2231.46", "Médico ortopedista e traumatologista"},
                    {"2231.46", "Médico traumatologista"},
                    {"2231.46", "Ortopedista"},
                    {"2231.46", "Traumatologista"},
                    {"2231.47", "Cirurgião otorrinolaringologista"},
                    {"2231.47", "Médico otorrinolaringologista"},
                    {"2231.47", "Otorrino"},
                    {"2231.47", "Otorrinolaringologista"},
                    {"2231.48", "Médico laboratorista"},
                    {"2231.48", "Médico patologista"},
                    {"2231.48", "Médico patologista clínico"},
                    {"2231.48", "Patologista clínico"},
                    {"2232.48", "Cirurgião dentista - periodontista"},
                    {"2232.48", "Dentista de gengivas"},
                    {"2235.45", "Enfermeira parteira"},
                    {"2235.45", "Enfermeiro obstétrico"},
                    {"2231.49", "Médico de criança"},
                    {"2231.49", "Médico pediatra"},
                    {"2231.49", "Neonatologista"},
                    {"2231.49", "Pediatra"},
                    {"2232.48", "Periodontista"},
                    {"2231.50", "Médico perito"},
                    {"2231.51", "Médico pneumologista"},
                    {"2231.51", "Médico pneumotisiologista"},
                    {"2231.51", "Pneumologista"},
                    {"2231.51", "Pneumotisiologista"},
                    {"2231.51", "Tisiologista"},
                    {"2231.52", "Cirurgião proctologista"},
                    {"2231.52", "Médico proctologista"},
                    {"2231.52", "Proctologista"},
                    {"2232.52", "Cirurgião dentista - protesiólogo bucomaxilofacial"},
                    {"2231.53", "Médico psicanalista"},
                    {"2231.53", "Médico psicoterapeuta"},
                    {"2231.53", "Médico psiquiatra"},
                    {"2231.53", "Neuropsiquiatra"},
                    {"2232.52", "Protesista bucomaxilofacial"},
                    {"2231.53", "Psiquiatra"},
                    {"2235.50", "Enfermeiro psiquiátrico"},
                    {"2231.54", "Médico em radioterapia"},
                    {"2231.54", "Médico radioterapeuta"},
                    {"2231.55", "Médico reumatologista"},
                    {"2231.55", "Reumatologista"},
                    {"2231.56", "Epidemiologista"},
                    {"2231.56", "Médico de saúde pública"},
                    {"2231.56", "Médico epidemiologista"},
                    {"2231.56", "Médico higienista"},
                    {"2231.56", "Médico sanitarista"},
                    {"2231.57", "Andrologista"},
                    {"2232.56", "Cirurgião dentista - protesista"},
                    {"2231.57", "Cirurgião urológico"},
                    {"2231.57", "Cirurgião urologista"},
                    {"2231.57", "Médico urologista"},
                    {"2232.56", "Odontólogo protesista"},
                    {"2232.56", "Protesista"},
                    {"2231.57", "Urologista"},
                    {"2235.55", "Enfermeiro puericultor e pediátrico"},
                    {"2232.60", "Cirurgião dentista - radiologista"},
                    {"2232.60", "Odontoradiologista"},
                    {"2235.60", "Enfermeiro de saúde publica"},
                    {"2235.60", "Enfermeiro sanitarista"},
                    {"2232.64", "Cirurgião dentista - reabilitador oral"},
                    {"2232.68", "Cirurgião dentista - traumatologista bucomaxilofacial"},
                    {"2232.68", "Cirurgião oral e maxilofacial"},
                    {"2232.68", "Odontólogo (cirurgia e traumatologia bucomaxilofacial)"},
                    {"2232.72", "Cirurgião dentista de saúde coletiva"},
                    {"2232.72", "Dentista de sáude coletiva"},
                    {"2232.72", "Odontologista social"},
                    {"2232.72", "Odontólogo de saúde coletiva"},
                    {"2232.72", "Odontólogo de saúde pública"},
                    {"2394.25", "Psicopedagogo"},
                    {"2515", "PSICÓLOGOS E PSICANALISTAS"},
                    {"2515.05", "Psicólogo da educação"},
                    {"2515.05", "Psicólogo educacional"},
                    {"2515.05", "Psicólogo escolar"},
                    {"2516.05", "Assistente social"},
                    {"2515.10", "Psicólogo acupunturista"},
                    {"2515.10", "Psicólogo clínico"},
                    {"2515.10", "Psicólogo da saúde"},
                    {"2515.10", "Psicoterapeuta"},
                    {"2515.10", "Terapeuta"},
                    {"2521.05", "Administrador"},
                    {"2515.15", "Psicólogo desportivo"},
                    {"2515.15", "Psicólogo do esporte"},
                    {"2515.20", "Psicólogo hospitalar"},
                    {"2515.25", "Psicólogo criminal"},
                    {"2515.25", "Psicólogo forense"},
                    {"2515.25", "Psicólogo jurídico"},
                    {"2515.30", "Psicólogo social"},
                    {"2515.35", "Psicólogo do trânsito"},
                    {"2515.40", "Psicólogo do trabalho"},
                    {"2515.40", "Psicólogo organizacional"},
                    {"2515.45", "Neuropsicólogo"},
                    {"2515.50", "Psicanalista"},
                    {"3011.05", "Laboratorista - exclusive análises clínicas"},
                    {"3135.05", "Técnico em laboratório óptico"},
                    {"3134.10", "Técnico em instrumentação"},
                    {"3225", "TÉCNICOS EM PRÓTESES ORTOPÉDICAS"},
                    {"3221.05", "Acupunturista"},
                    {"3221.05", "Fitoterapeuta"},
                    {"3221.05", "Terapeuta naturalista"},
                    {"3221.05", "Terapeuta oriental"},
                    {"3222.05", "Técnico de enfermagem"},
                    {"3222.05", "Técnico de enfermagem socorrista"},
                    {"3222.05", "Técnico em hemotransfusão"},
                    {"3223.05", "Óptico oftálmico"},
                    {"3223.05", "optico optometrista"},
                    {"3223.05", "optico protesista"},
                    {"3224.05", "Técnico em higiene dental"},
                    {"3225.05", "Protesista (técnico)"},
                    {"3225.05", "Técnico ortopédico"},
                    {"3226.05", "Técnico em imobilizações do aparelho locomotor"},
                    {"3226.05", "Técnico em imobilizações gessadas"},
                    {"3222.10", "Técnico de enfermagem de terapia intensiva"},
                    {"3222.10", "Técnico em hemodiálise"},
                    {"3222.10", "Técnico em UTI"},
                    {"3224.10", "Protético dentário"},
                    {"3221.15", "Homeopata (exceto médico)"},
                    {"3221.15", "Terapeuta crâneo-sacral"},
                    {"3221.15", "Terapeuta holístico"},
                    {"3221.15", "Terapeuta manual"},
                    {"3221.15", "Terapeuta mio-facial"},
                    {"3222.15", "Técnico de enfermagem do trabalho"},
                    {"3222.15", "Técnico de enfermagem em saúde ocupacional"},
                    {"3222.15", "Técnico de enfermagem ocupacional"},
                    {"3224.15", "Atendente de clínica dentária"},
                    {"3224.15", "Atendente de Consultório Dentário"},
                    {"3224.15", "Atendente de gabinete dentário"},
                    {"3224.15", "Atendente de serviço odontólogico"},
                    {"3224.15", "Atendente odontológico"},
                    {"3224.15", "Auxiliar de dentista"},
                    {"3222.20", "Técnico de enfermagem em saúde mental"},
                    {"3222.20", "Técnico de enfermagem psiquiátrica"},
                    {"3224.20", "Auxiliar de Prótese Dentária"},
                    {"3241.05", "Operador de eletroencefalógrafo"},
                    {"3222.25", "Instrumentador cirúrgico"},
                    {"3222.25", "Instrumentador em cirurgia"},
                    {"3222.25", "Instrumentadora cirúrgica"},
                    {"3242.05", "Técnico de laboratório de análises clínicas"},
                    {"3242.05", "Técnico em patologia clínica"},
                    {"3241.10", "Operador de eletrocardiógrafo"},
                    {"3251", "TÉCNICO EM FARMÁCIA E EM MANIPULAÇÃO FARMACÊUTICA"},
                    {"3222.30", "Auxiliar de enfermagem"},
                    {"3222.30", "Auxiliar de enfermagem de central de material esterelizado (CME)"},
                    {"3222.30", "Auxiliar de enfermagem de centro cirúrgico"},
                    {"3222.30", "Auxiliar de enfermagem de clínica médica"},
                    {"3222.30", "Auxiliar de enfermagem de hospital"},
                    {"3222.30", "Auxiliar de enfermagem de saúde pública"},
                    {"3222.30", "Auxiliar de enfermagem em hemodiálise"},
                    {"3222.30", "Auxiliar de enfermagem em home care"},
                    {"3222.30", "Auxiliar de enfermagem em nefrologia"},
                    {"3222.30", "Auxiliar de enfermagem em saúde mental"},
                    {"3222.30", "Auxiliar de enfermagem socorrista"},
                    {"3222.30", "Auxiliar de ginecologia"},
                    {"3222.30", "Auxiliar de hipodermia"},
                    {"3222.30", "Auxiliar de obstetrícia"},
                    {"3222.30", "Auxiliar de oftalmologia"},
                    {"3222.30", "Auxiliar em hemotransfusão"},
                    {"3242.10", "Auxiliar técnico de laboratório de análises clínicas"},
                    {"3242.10", "Auxiliar técnico em patologia clínica"},
                    {"3251.05", "Auxiliar técnico em laboratório de farmácia"},
                    {"3241.15", "Técnico em hemodinâmica"},
                    {"3241.15", "Técnico em mamografia"},
                    {"3241.15", "Técnico em radiologia"},
                    {"3241.15", "Técnico em radiologia e imagenologia"},
                    {"3241.15", "Técnico em radiologia médica"},
                    {"3241.15", "Técnico em radiologia odontológica"},
                    {"3241.15", "Técnico em tomografia"},
                    {"3222.35", "Auxiliar de enfermagem do trabalho"},
                    {"3222.35", "Auxiliar de enfermagem em saúde ocupacional"},
                    {"3222.35", "Auxiliar de enfermagem ocupacional"},
                    {"3251.10", "Técnico em laboratório de farmácia"},
                    {"3222.40", "Auxiliar de saúde (navegação marítima)"},
                    {"3222.40", "Auxiliar de saúde marítimo"},
                    {"3253.10", "Técnico em imunobiológicos"},
                    {"3251.15", "Técnico em Farmácia"},
                    {"3522", "AGENTES DA SAÚDE E DO MEIO AMBIENTE"},
                    {"3522.10", "Agente de saneamento"},
                    {"3522.10", "Agente de saúde pública"},
                    {"4110.10", "Assistente administrativo"},
                    {"4110.10", "Assistente técnico - no serviço público"},
                    {"4110.10", "Assistente técnico administrativo"},
                    {"4151.20", "Fitotecário"},
                    {"4221.05", "Atendente de clínica veterinária"},
                    {"4221.05", "Atendente de consultório veterinário"},
                    {"4221.10", "Atendente de ambulatório"},
                    {"4221.10", "Atendente de clínica médica"},
                    {"4221.10", "Atendente de consultório médico"},
                    {"4221.15", "Atendente de seguro saúde"},
                    {"5151", "AGENTES COMUNITÁRIOS DE SAÚDE E AFINS"},
                    {"5152", "AUXILIARES DE LABORATÓRIO DA SAÚDE"},
                    {"5132.20", "Cozinheiro de hospital"},
                    {"5151.05", "Agente de saúde"},
                    {"5151.05", "Visitador de saúde"},
                    {"5151.05", "Visitador de saúde em domicílio"},
                    {"5151.10", "Atendente de berçário"},
                    {"5151.10", "Atendente de centro cirúrgico"},
                    {"5151.10", "Atendente de enfermagem"},
                    {"5151.10", "Atendente de enfermagem no serviço doméstico"},
                    {"5151.10", "Atendente de hospital"},
                    {"5151.10", "Atendente de serviço de saúde"},
                    {"5151.10", "Atendente de serviço médico"},
                    {"5151.10", "Atendente hospitalar"},
                    {"5151.10", "Atendente-enfermeiro"},
                    {"5152.10", "Auxiliar de farmácia de manipulação"},
                    {"5134.30", "Copeiro de hospital"},
                    {"5151.15", "Assistente de parto"},
                    {"5151.20", "Auxiliar de sanitarista"},
                    {"5151.20", "Imunizador"},
                    {"5162.10", "Acompanhante de idosos"},
                    {"5168.05", "Radioestesista"},
                    {"5161.15", "Auxiliar de estética"},
                    {"5161.35", "Massoterapeuta"},
                    {"5193.05", "Auxiliar de enfermagem veterinária"},
                    {"5193.05", "Auxiliar de veterinário"},
                    {"5193.05", "Enfermeiro veterinário"},
                    {"5211.30", "Atendente de farmácia - balconista"},
                    {"6233.15", "Auxiliar de incubação"},
                    {"6233.15", "Operador de incubadora"},
                    {"7411.05", "Instrumentista de precisão"},
                    {"7664.20", "Auxiliar de radiologia (revelação fotográfica)"},
                    {"7823.10", "Motorista de ambulância"},
                    {"9151.05", "Instrumentista de laboratório (manutenção)"},
                    {"9153.05", "Técnico em manutenção de equipamentos e instrumentos médicohospitalares"},
                };

            foreach (var eachTuple in tissEspecialidades)
                db.SYS_MedicalSpecialty.AddObject(new SYS_MedicalSpecialty { Code = eachTuple.Item1, Name = eachTuple.Item2 });

            db.SaveChanges();
        }

        public static SYS_MedicalProcedure[] CreateFakeMedicalProcedures(CerebelloEntities db)
        {
            // Predictable set of medical procedures.
            // The official set o procedures is much larger, and may change over time.
            // These fake medical procedures, must not change over time, unless it is of extreme necessity.
            // In this case, care must be taken so that tests don't break.

            var tissConselhoProfissional = new ListOfTuples<string, string>
                {
                    {"4.03.04.36-1", "Hemograma com contagem de plaquetas ou frações"},
                    {"4.01.03.23-4", "Eletrencefalograma em vigília, e sono espontâneo ou induzido"},
                    {"4.01.03.55-2", "Posturografia"},
                    {"3.07.15.26-1", "Retirada de corpo estranho - tratamento cirúrgico"},
                    {"1.01.06.01-4", "Aconselhamento genético"},
                    {"2.01.01.22-8", "Acompanhamento clínico ambulatorial pós-transplante de medula óssea"},
                    {"2.01.03.45-0", "Paraplegia e tetraplegia"},
                    {"2.01.03.46-8", "Parkinson"},
                    {"3.01.01.26-3", "Dermoabrasão de lesões cutâneas"},
                    {"3.03.11.01-2", "Biópsia de músculos"},
                    {"3.03.11.02-0", "Cirurgia com sutura ajustável"},
                };

            foreach (var eachTuple in tissConselhoProfissional)
                db.SYS_MedicalProcedure.AddObject(new SYS_MedicalProcedure { Code = eachTuple.Item1, Name = eachTuple.Item2 });

            db.SaveChanges();

            return db.SYS_MedicalProcedure.ToArray();
        }

        private class ListOfTuples<T1, T2> : List<Tuple<T1, T2>>
        {
            public void Add(T1 t1, T2 t2)
            {
                this.Add(new Tuple<T1, T2>(t1, t2));
            }
        }

        private static readonly object locker = new object();
        private static Cbhpm cbhpm;

        public static void Initialize_SYS_MedicalProcedures(
            CerebelloEntities db, string pathOfTxt, int maxCount = int.MaxValue, Action<int, int> progress = null)
        {
            // Adding CBHPM medical procedures.
            if (cbhpm == null)
                lock (locker)
                    if (cbhpm == null)
                    {
                        var cbhpm0 = Cbhpm.LoadData(pathOfTxt);

                        // ensures that the instance returned from LoadData is well written,
                        // and only then, it assigns the static variable.
                        System.Threading.Thread.MemoryBarrier();
                        cbhpm = cbhpm0;
                    }

            var max = Math.Min(maxCount, cbhpm.Items.Values.OfType<Cbhpm.Proc>().Count());

            int count = 0;
            foreach (var eachCbhpmProc in cbhpm.Items.Values.OfType<Cbhpm.Proc>())
            {
                if (count >= maxCount)
                    break;

                if (progress != null) progress(count, max);

                var item = db.SYS_MedicalProcedure.CreateObject();
                item.Code = eachCbhpmProc.Codigo;
                item.Name = eachCbhpmProc.Nome;
                db.SYS_MedicalProcedure.AddObject(item);

                if (count % 100 == 0)
                    db.SaveChanges();

                count++;
            }

            if (progress != null) progress(count, max);

            db.SaveChanges();
        }

        /// <summary>
        /// Clears all data in the database.
        /// </summary>
        /// <param name="db"></param>
        public static void ClearAllData(CerebelloEntities db)
        {
            db.ExecuteStoreCommand(@"EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'");
            db.ExecuteStoreCommand(@"sp_MSForEachTable '
                         SET QUOTED_IDENTIFIER ON;
                         IF OBJECTPROPERTY(object_id(''?''), ''TableHasForeignRef'') = 1
                         DELETE FROM ?
                         else 
                         TRUNCATE TABLE ?
                     '");
            db.ExecuteStoreCommand(@"sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL'");
            db.ExecuteStoreCommand(@"sp_MSForEachTable ' 
                         SET QUOTED_IDENTIFIER ON;
                         IF OBJECTPROPERTY(object_id(''?''), ''TableHasIdentity'') = 1 
                         DBCC CHECKIDENT (''?'', RESEED, 0) 
                     ' ");
        }

        public static bool CreateDatabaseIfNeeded(CerebelloEntities db)
        {
            var sqlConn1 = (SqlConnection)((EntityConnection)db.Connection).StoreConnection;
            var sqlConn2 = new SqlConnectionStringBuilder(sqlConn1.ConnectionString) { InitialCatalog = "" };
            var connStr = sqlConn2.ToString();
            var dbName = sqlConn1.Database;

            // creates the database
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                try
                {
                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = string.Format(@"CREATE DATABASE {0}", dbName);
                        command.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 5170)
                        return true;

                    return false;
                }

                return true;
            }
        }

        public static void DropAllTables(CerebelloEntities db)
        {
            // http://stackoverflow.com/questions/536350/sql-server-2005-drop-all-the-tables-stored-procedures-triggers-constriants-a
            var script =
                @"
/* Drop all non-system stored procs */
DECLARE @name VARCHAR(128)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'P' AND category = 0 ORDER BY [name])

WHILE @name is not null
BEGIN
    SELECT @SQL = 'DROP PROCEDURE [dbo].[' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT 'Dropped Procedure: ' + @name
    SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'P' AND category = 0 AND [name] > @name ORDER BY [name])
END
GO

/* Drop all views */
DECLARE @name VARCHAR(128)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'V' AND category = 0 ORDER BY [name])

WHILE @name IS NOT NULL
BEGIN
    SELECT @SQL = 'DROP VIEW [dbo].[' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT 'Dropped View: ' + @name
    SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'V' AND category = 0 AND [name] > @name ORDER BY [name])
END
GO

/* Drop all functions */
DECLARE @name VARCHAR(128)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] IN (N'FN', N'IF', N'TF', N'FS', N'FT') AND category = 0 ORDER BY [name])

WHILE @name IS NOT NULL
BEGIN
    SELECT @SQL = 'DROP FUNCTION [dbo].[' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT 'Dropped Function: ' + @name
    SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] IN (N'FN', N'IF', N'TF', N'FS', N'FT') AND category = 0 AND [name] > @name ORDER BY [name])
END
GO

/* Drop all Foreign Key constraints */
DECLARE @name VARCHAR(128)
DECLARE @constraint VARCHAR(254)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' ORDER BY TABLE_NAME)

WHILE @name is not null
BEGIN
    SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    WHILE @constraint IS NOT NULL
    BEGIN
        SELECT @SQL = 'ALTER TABLE [dbo].[' + RTRIM(@name) +'] DROP CONSTRAINT [' + RTRIM(@constraint) +']'
        EXEC (@SQL)
        PRINT 'Dropped FK Constraint: ' + @constraint + ' on ' + @name
        SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' AND CONSTRAINT_NAME <> @constraint AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    END
SELECT @name = (SELECT TOP 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' ORDER BY TABLE_NAME)
END
GO

/* Drop all Primary Key constraints */
DECLARE @name VARCHAR(128)
DECLARE @constraint VARCHAR(254)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' ORDER BY TABLE_NAME)

WHILE @name IS NOT NULL
BEGIN
    SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    WHILE @constraint is not null
    BEGIN
        SELECT @SQL = 'ALTER TABLE [dbo].[' + RTRIM(@name) +'] DROP CONSTRAINT [' + RTRIM(@constraint)+']'
        EXEC (@SQL)
        PRINT 'Dropped PK Constraint: ' + @constraint + ' on ' + @name
        SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND CONSTRAINT_NAME <> @constraint AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    END
SELECT @name = (SELECT TOP 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' ORDER BY TABLE_NAME)
END
GO

/* Drop all tables */
DECLARE @name VARCHAR(128)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'U' AND category = 0 ORDER BY [name])

WHILE @name IS NOT NULL
BEGIN
    SELECT @SQL = 'DROP TABLE [dbo].[' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT 'Dropped Table: ' + @name
    SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'U' AND category = 0 AND [name] > @name ORDER BY [name])
END
GO
";

            ExecuteScript(db, script);
        }

        /// <summary>
        /// Executes a SQL script containing GO statements.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="script"></param>
        public static void ExecuteScript(CerebelloEntities db, string script)
        {
            var scripts = SqlHelper.SplitScript(script);
            ExecuteScriptList(db, scripts);
        }

        /// <summary>
        /// Executes a SQL script containing GO statements.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="scripts"></param>
        public static void ExecuteScriptList(CerebelloEntities db, IEnumerable<string> scripts)
        {
            foreach (var eachScript in scripts)
                db.ExecuteStoreCommand(eachScript);
        }

        public enum AttachLocalDatabaseResult
        {
            Ok,
            AlreadyAttached,
            NotFound,
            Failed,
        }

        /// <summary>
        /// Attaches the given database.
        /// </summary>
        internal static AttachLocalDatabaseResult AttachLocalDatabase(CerebelloEntities db)
        {
            var sqlConn1 = (SqlConnection)((EntityConnection)db.Connection).StoreConnection;
            var sqlConn2 = new SqlConnectionStringBuilder(sqlConn1.ConnectionString) { InitialCatalog = "" };
            var connStr = sqlConn2.ToString();
            var dbName = sqlConn1.Database;

            // attaches the database
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                try
                {
                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText =
                            string.Format(@"SELECT CASE WHEN db_id('{0}') IS NULL THEN 0 ELSE 1 END", dbName);

                        var dbExists = (int)command.ExecuteScalar() == 1;

                        if (dbExists)
                            return AttachLocalDatabaseResult.AlreadyAttached;

                        command.CommandText =
                            string.Format(@"
                                IF db_id('{0}') IS NULL
                                    CREATE DATABASE {0}
                                        ON ( FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL10_50.SQLEXPRESS\MSSQL\DATA\{0}.mdf' )
                                        FOR ATTACH ;", dbName);

                        command.ExecuteNonQuery();

                        return AttachLocalDatabaseResult.Ok;
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 5120) // file not found
                        return AttachLocalDatabaseResult.NotFound;

                    // probably the database exists already because a previous test failed.. let's move on
                    return AttachLocalDatabaseResult.Failed;
                }
            }
        }

        /// <summary>
        /// Creates a backup from the given database.
        /// </summary>
        internal static string CreateBackup(CerebelloEntities db, string backupName = "default")
        {
            var sqlConn1 = (SqlConnection)((EntityConnection)db.Connection).StoreConnection;
            var sqlConn2 = new SqlConnectionStringBuilder(sqlConn1.ConnectionString) { InitialCatalog = "" };
            var connStr = sqlConn2.ToString();
            var dbName = sqlConn1.Database;

            // attaches the database
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                string fileName;
                try
                {
                    using (var command = conn.CreateCommand())
                    {
                        fileName = string.Format("{0}_{1}.Bak", dbName, backupName);

                        string bakFile = Path.Combine(
                            @"C:\Program Files\Microsoft SQL Server\MSSQL10_50.SQLEXPRESS\MSSQL",
                            string.Format(@"Backup\{0}", fileName));

                        command.CommandText = string.Format(@"
                            BACKUP DATABASE {0}
                            TO DISK = N'{2}'
                               WITH FORMAT,
                                  MEDIANAME = '{0}_{1}_bak',
                                  NAME = 'Full Backup of {0}';
                            ", dbName, backupName, bakFile);

                        command.ExecuteNonQuery();

                        return fileName;
                    }
                }
                catch (SqlException ex)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Restores a backup for the given database.
        /// </summary>
        internal static string RestoreBackup(CerebelloEntities db, string backupName = "default")
        {
            var sqlConn1 = (SqlConnection)((EntityConnection)db.Connection).StoreConnection;
            var sqlConn2 = new SqlConnectionStringBuilder(sqlConn1.ConnectionString) { InitialCatalog = "" };
            var connStr = sqlConn2.ToString();
            var dbName = sqlConn1.Database;

            // attaches the database
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                string fileName;
                try
                {
                    using (var command = conn.CreateCommand())
                    {
                        fileName = string.Format("{0}_{1}.Bak", dbName, backupName);

                        string bakFile = Path.Combine(
                            @"C:\Program Files\Microsoft SQL Server\MSSQL10_50.SQLEXPRESS\MSSQL",
                            string.Format(@"Backup\{0}", fileName));

                        command.CommandText = string.Format(@"
                            USE master
                            ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                            ", dbName);

                        command.ExecuteNonQuery();

                        command.CommandText = string.Format(@"
                            USE master
                            RESTORE DATABASE {0}
                            FROM DISK = N'{1}'
                            ", dbName, bakFile);

                        command.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    return null;
                }

                return fileName;
            }
        }

        public static bool BackupExists(CerebelloEntities db, string backupName = "default")
        {
            var sqlConn1 = (SqlConnection)((EntityConnection)db.Connection).StoreConnection;
            var sqlConn2 = new SqlConnectionStringBuilder(sqlConn1.ConnectionString) { InitialCatalog = "" };
            var connStr = sqlConn2.ToString();
            var dbName = sqlConn1.Database;

            // attaches the database
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                try
                {
                    using (var command = conn.CreateCommand())
                    {
                        var fileName = string.Format("{0}_{1}.Bak", dbName, backupName);

                        var bakFile = Path.Combine(
                            @"C:\Program Files\Microsoft SQL Server\MSSQL10_50.SQLEXPRESS\MSSQL",
                            string.Format(@"Backup\{0}", fileName));

                        command.CommandText = string.Format(@"
                            USE master
                            RESTORE VERIFYONLY
                            FROM DISK = N'{0}'
                            ", bakFile);

                        command.ExecuteNonQuery();
                    }

                    return true;
                }
                catch (SqlException ex)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Detaches the given database.
        /// </summary>
        internal static void DetachLocalDatabase(CerebelloEntities db)
        {
            var sqlConn1 = (SqlConnection)((EntityConnection)db.Connection).StoreConnection;
            var sqlConn2 = new SqlConnectionStringBuilder(sqlConn1.ConnectionString) { InitialCatalog = "" };
            var connStr = sqlConn2.ToString();

            SqlConnection.ClearAllPools();

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand("", conn)
                    {
                        CommandText = string.Format(@"sp_detach_db {0}", sqlConn1.Database)
                    };
                cmd.ExecuteNonQuery();
            }
        }

        public static Practice CreatePractice(CerebelloEntities db, string name, User owner, string urlIdentifier)
        {
            var practice = new Practice
                {
                    Name = name,
                    CreatedOn = Firestarter.UtcNow.AddDays(-1),
                    WindowsTimeZoneId = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time").Id,
                    VerificationDate = Firestarter.UtcNow,
                    UrlIdentifier = urlIdentifier,
                };

            db.Practices.AddObject(practice);
            db.SaveChanges();

            var sec = owner.Secretary;
            var adm = owner.Administrator;
            var doc = owner.Doctor;

            owner.Secretary = null;
            owner.Administrator = null;
            owner.Doctor = null;

            practice.Owner = owner;
            owner.Practice = practice;
            db.SaveChanges();

            owner.Secretary = sec;
            owner.Administrator = adm;
            owner.Doctor = doc;

            db.SaveChanges();

            SetupPracticeWithTrialContract(db, practice);

            return practice;
        }

        public static User CreateUser(CerebelloEntities db, Practice practice, string username, string password, string name)
        {
            var pwdSalt = "C2p4NIf+1JYZmNHdKMgpop==";
            var pwdHash = CipherHelper.Hash(password, pwdSalt);
            if (string.IsNullOrEmpty(password))
                pwdHash = CipherHelper.Hash(Constants.DEFAULT_PASSWORD, pwdSalt);

            // Creating user.
            var user = new User()
                {
                    UserName = username,
                    UserNameNormalized = StringHelper.NormalizeUserName(username),
                    LastActiveOn = Firestarter.UtcNow,
                    PasswordSalt = pwdSalt,
                    Password = pwdHash,
                    Practice = practice,
                };

            if (user.Practice != null)
                db.Users.AddObject(user);

            // Creating person.
            var person = new Person()
                {
                    DateOfBirth = ConvertFromDefaultToUtc(new DateTime(1974, 10, 12)),
                    FullName = name,
                    Gender = (int)TypeGender.Female,
                    CreatedOn = Firestarter.UtcNow,
                    PracticeId = practice.Id,
                };

            user.Person = person;

            if (user.Practice != null)
                db.SaveChanges();

            return user;
        }

        public static void CreateFakeHealthInsurances(CerebelloEntities db, Doctor doctor, int seed)
        {
            if ((seed & 1) != 0)
                doctor.HealthInsurances.Add(
                    new HealthInsurance
                        {
                            IsActive = (seed & 2) != 0,

                            Name = "Unimed - Juiz de Fora",
                            NewAppointmentValue = 20m,
                            ReturnAppointmentValue = 0m,
                            ReturnTimeInterval = 30,

                            PracticeId = doctor.PracticeId,
                        });

            if ((seed & 4) != 0)
                doctor.HealthInsurances.Add(
                    new HealthInsurance
                        {
                            IsActive = (seed & 8) != 0,

                            Name = "Cross",
                            NewAppointmentValue = 15m,
                            ReturnAppointmentValue = 0m,
                            ReturnTimeInterval = 20,

                            PracticeId = doctor.PracticeId,
                        });

            if ((seed & 16) != 0)
                doctor.HealthInsurances.Add(
                    new HealthInsurance
                        {
                            IsActive = (seed & 32) != 0,

                            Name = "Plasc",
                            NewAppointmentValue = 20m,
                            ReturnAppointmentValue = 5m,
                            ReturnTimeInterval = 40,

                            PracticeId = doctor.PracticeId,
                        });

            if ((seed & 64) != 0)
                doctor.HealthInsurances.Add(
                    new HealthInsurance
                        {
                            IsActive = (seed & 128) != 0,

                            Name = "Allianz Saúde",
                            NewAppointmentValue = 30m,
                            ReturnAppointmentValue = 5m,
                            ReturnTimeInterval = 60,

                            PracticeId = doctor.PracticeId,
                        });

            if ((seed & 256) != 0)
                doctor.HealthInsurances.Add(
                    new HealthInsurance
                        {
                            IsActive = (seed & 512) != 0,

                            Name = "Porto Seguro",
                            NewAppointmentValue = 16m,
                            ReturnAppointmentValue = 1m,
                            ReturnTimeInterval = 28,

                            PracticeId = doctor.PracticeId,
                        });

            db.SaveChanges();
        }

        public static void CreateFakeAppointments(CerebelloEntities db, Doctor doctor, int seed, int count = 300)
        {
            var practice = db.Practices.First(p => p.Id == doctor.PracticeId);
            var startingTime = PracticeController.ConvertToLocalDateTime(practice, Firestarter.UtcNow.AddDays(-20));
            var practiceNow = PracticeController.ConvertToLocalDateTime(practice, Firestarter.UtcNow);
            var user = doctor.Users.First();
            var dbWrapper = new CerebelloEntitiesAccessFilterWrapper(db);
            dbWrapper.SetCurrentUserById(user.Id);
            var patients = db.Patients.Where(p => p.DoctorId == doctor.Id).ToList();

            if (patients.Count < 1)
                throw new Exception("Cannot create fake appointments. There are no patients");

            var helthInsurances = db.HealthInsurances
                .Where(hi => hi.IsActive && hi.DoctorId == doctor.Id)
                .ToList();
            if (helthInsurances.Count < 1)
                throw new Exception("Cannot create fake appointments. There are no helth insurances");

            var random = new Random(seed);
            var creator = db.Users.First();

            for (var i = 0; i < count; i++)
            {
                // get random patient
                var nextFreeTime = ScheduleController.FindNextFreeTimeInPracticeLocalTime(dbWrapper, doctor, startingTime);

                // there's a 10% chance I will not schedule this appointment, so this will be a free slot
                if (random.Next(1, 11) != 1)
                {
                    TypeAppointmentStatus status;
                    if (nextFreeTime.Item1 < practiceNow)
                        // if the appointment is in the past, there's a 20% chance it didn't accomplish
                        status = random.Next(1, 11) < 9 ? TypeAppointmentStatus.Accomplished : TypeAppointmentStatus.NotAccomplished;
                    else
                        status = TypeAppointmentStatus.Undefined;


                    db.Appointments.AddObject(
                        new Appointment
                        {
                            CreatedById = creator.Id,
                            CreatedOn = Firestarter.UtcNow,
                            DoctorId = doctor.Id,
                            Start = nextFreeTime.Item1.ToUniversalTime(),
                            End = nextFreeTime.Item2.ToUniversalTime(),
                            Patient = patients[random.Next(0, patients.Count - 1)],
                            Type = (int)TypeAppointment.MedicalAppointment,
                            HealthInsurance = helthInsurances[random.Next(0, helthInsurances.Count - 1)],
                            PracticeId = doctor.PracticeId,
                            Status = (int)status
                        });

                }

                startingTime = nextFreeTime.Item2;
            }

            db.SaveChanges();
        }

        /// <summary>
        /// Recreates the specified so that we can debug using IIS.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userName"></param>
        internal static void RecreateUser(CerebelloEntities db, string userName)
        {
            try
            {
                ExecuteScript(db, string.Format(@"
                    USE [cerebello]
                    GO

                    IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'{0}')
                    DROP USER [{0}]
                    GO

                    USE [cerebello]
                    GO

                    CREATE USER [{0}] FOR LOGIN [{0}]
                    GO

                    USE [cerebello]
                    GO

                    EXEC sp_addrolemember N'db_owner', N'{0}'
                    GO
                    ", userName));
            }
            catch
            {
                // if cannot create user, maybe it already exists, or the user does not exists in Windows
            }
        }
    }
}

