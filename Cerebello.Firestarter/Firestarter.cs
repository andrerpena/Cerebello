using System;
using System.Collections.Generic;
using System.Linq;
using Cerebello.Model;
using CerebelloWebRole.Code;
using CerebelloWebRole.Models;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data.EntityClient;
using Cerebello.Firestarter.Helpers;
using System.IO;

namespace Cerebello.Firestarter
{
    public static class Firestarter
    {
        /// <summary>
        /// Crates a fake user, doctor and practice.
        /// </summary>
        public static Doctor Create_CrmMg_Psiquiatria_DrHouse_Andre(CerebelloEntities db)
        {
            // Creating data infrastructure.
            var entity = GetMedicalEntity_Crm(db);
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
        public static List<Doctor> Create_CrmMg_Psiquiatria_DrHouse_Andre_Miguel(CerebelloEntities db)
        {
            // Creating data infrastructure.
            var entity = GetMedicalEntity_Crm(db);
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

        /// <summary>
        /// Crates a fake user, doctor and practice.
        /// </summary>
        public static Doctor Create_CrmMg_Psiquiatria_DraMarta_Marta(CerebelloEntities db)
        {
            // Creating data infrastructure.
            var entity = GetMedicalEntity_Crm(db);
            var specialty = CreateSpecialty_Psiquiatria(db);

            // Creating practice.
            var practice = CreatePractice_DraMarta(db);

            var marta = CreateDoctor_MartaCura(db, entity, specialty, practice);

            return marta;
        }

        public static SYS_MedicalSpecialty CreateSpecialty_Psiquiatria(CerebelloEntities db)
        {
            var specialty = new SYS_MedicalSpecialty()
            {
                Name = "Psiquiatria"
            };

            db.SYS_MedicalSpecialty.AddObject(specialty);
            return specialty;
        }

        public static SYS_MedicalEntity GetMedicalEntity_Crm(CerebelloEntities db)
        {
            return db.SYS_MedicalEntity.Where(me => me.Code == "CRM").Single();
        }

        public static Doctor CreateAdministratorDoctor_Miguel(CerebelloEntities db, SYS_MedicalEntity entity, SYS_MedicalSpecialty specialty, Practice practice, bool useDefaultPassword = false)
        {
            var pwdSalt = "oHdC62UZE6Hwts91+Xy88Q==";
            var pwdHash = CipherHelper.Hash("masban", pwdSalt);
            if (useDefaultPassword)
                pwdHash = CipherHelper.Hash(CerebelloWebRole.Code.Constants.DEFAULT_PASSWORD, pwdSalt);

            // Creating user.
            User user = new User()
            {
                UserName = "masbicudo",
                UserNameNormalized = "masbicudo",
                LastActiveOn = DateTime.UtcNow,
                Password = pwdHash,
                PasswordSalt = pwdSalt,
                Email = "masbicudo@gmail.com",
                GravatarEmailHash = "b209e81c82e45437da92af24ddc97360",
                Practice = practice,
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
                SYS_MedicalSpecialty = specialty,
                SYS_MedicalEntity = entity,
                MedicalEntityJurisdiction = "MG",
            };

            user.Doctor = doctor;

            db.SaveChanges();

            // Creating admin.
            Administrator admin = new Administrator()
            {
                Id = 2,
            };

            user.Administrator = admin;

            db.SaveChanges();

            // Creating e-mail.
            user.Person.Emails.Add(new Email()
            {
                Address = "masbicudo@gmail.com"
            });

            db.SaveChanges();

            return doctor;
        }

        public static Doctor CreateDoctor_Andre(CerebelloEntities db, SYS_MedicalEntity entity, SYS_MedicalSpecialty specialty, Practice practice)
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
                UserNameNormalized = "andrerpena",
                Person = person,
                LastActiveOn = DateTime.UtcNow,
                Password = "aThARLVPRzyS7yAb4WGDDsppzrA=",
                PasswordSalt = "nnKvjK+67w7OflE9Ri4MQw==",
                Email = "andrerpena@gmail.com",
                GravatarEmailHash = "574700aef74b21d386ba1250b77d20c6",
                Practice = practice,
            };

            db.Users.AddObject(user);

            db.SaveChanges();

            Doctor doctor = new Doctor()
            {
                Id = 1,
                CRM = "12345",
                SYS_MedicalSpecialty = specialty,
                SYS_MedicalEntity = entity,
                MedicalEntityJurisdiction = "MG",
            };

            db.Doctors.AddObject(doctor);

            db.SaveChanges();

            user.Doctor = doctor;
            user.Person.Emails.Add(new Email() { Address = "andrerpena@gmail.com" });

            db.SaveChanges();

            return doctor;
        }

        public static Doctor CreateDoctor_MartaCura(CerebelloEntities db, SYS_MedicalEntity entity, SYS_MedicalSpecialty specialty, Practice practice)
        {
            Person person = new Person()
            {
                DateOfBirth = new DateTime(1967, 04, 20),
                FullName = "Marta Cura",
                UrlIdentifier = "martacura",
                Gender = (int)TypeGender.Female,
                CreatedOn = DateTime.UtcNow,
            };

            db.People.AddObject(person);

            db.SaveChanges();

            User user = new User()
            {
                UserName = "martacura",
                UserNameNormalized = "martacura",
                Person = person,
                LastActiveOn = DateTime.UtcNow,
                PasswordSalt = "ELc81TnRE+Eb+e5/D69opg==",
                Password = "lLqJ7FjmEQF7q4rxWIGnX+AXdqQ=",
                Email = "martacura@gmail.com",
                Practice = practice,
            };

            db.Users.AddObject(user);

            db.SaveChanges();

            Doctor doctor = new Doctor()
            {
                Id = 4,
                CRM = "74653",
                SYS_MedicalSpecialty = specialty,
                SYS_MedicalEntity = entity,
                MedicalEntityJurisdiction = "MG",
            };

            db.Doctors.AddObject(doctor);

            db.SaveChanges();

            user.Doctor = doctor;
            user.Person.Emails.Add(new Email() { Address = "martacura@gmail.com" });

            db.SaveChanges();

            return doctor;
        }

        /// <summary>
        /// Creates the secretary Milena, with Id = 3.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="practice"></param>
        /// <returns></returns>
        public static Secretary CreateSecretary_Milena(CerebelloEntities db, Practice practice, bool useDefaultPassword = false)
        {
            var pwdSalt = "egt/lzoRIw/M7XJsK3C0jw==";
            var pwdHash = CipherHelper.Hash("milena", pwdSalt);
            if (useDefaultPassword)
                pwdHash = CipherHelper.Hash(CerebelloWebRole.Code.Constants.DEFAULT_PASSWORD, pwdSalt);

            // Creating user.
            User user = new User()
            {
                UserName = "milena",
                UserNameNormalized = "milena",
                LastActiveOn = DateTime.UtcNow,
                PasswordSalt = pwdSalt,
                Password = pwdHash,
                Practice = practice,
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
        /// Creates a new practice and returns it.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static Practice CreatePractice_DraMarta(CerebelloEntities db)
        {
            var practice = new Practice()
            {
                Name = "Consultório da Dra. Marta",
                UrlIdentifier = "dramarta",
                CreatedOn = DateTime.UtcNow
            };

            db.Practices.AddObject(practice);

            db.SaveChanges();
            return practice;
        }

        /// <summary>
        /// Creates fake patients
        /// </summary>
        public static List<Patient> CreateFakePatients(Doctor doctor, CerebelloEntities db, int count = 70)
        {
            return FakePatientsFactory.CreateFakePatients(doctor, db, count);
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

        public static Appointment CreateFakeAppointments(CerebelloEntities db, DateTime createdOn, Doctor doc, DateTime start, TimeSpan duration, string desc, User creator = null)
        {
            creator = creator ?? doc.Users.First();

            Appointment result;

            db.Appointments.AddObject(result = new Appointment
            {
                CreatedById = creator.Id,
                CreatedOn = createdOn,
                DoctorId = doc.Id,
                Start = start,
                End = start + duration,
                Description = desc,
            });

            db.SaveChanges();

            return result;
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
                { "1312.05", "Diretor clínico" },
                { "1312.05", "Diretor de departamento de saúde" },
                { "1312.05", "Diretor de divisão médica" },
                { "1312.05", "Diretor de serviços de saúde" },
                { "1312.05", "Diretor de serviços médicos" },
                { "1312.05", "Diretor de unidade assistencial" },
                { "1312.05", "Diretor de unidade de saúde" },
                { "1312.05", "Diretor de unidade hospitalar" },
                { "1312.05", "Diretor médico-hospitalar" },
                { "1312.10", "Administrador de ambulatório" },
                { "1312.10", "Gerente de ambulatório" },
                { "1312.10", "Gerente de enfermagem" },
                { "1312.10", "Gerente de nutrição em unidades de saúde" },
                { "1312.10", "Gerente de serviços de saúde" },
                { "1311.20", "Gerente de serviços sociais" },
                { "2011", "PROFISSIONAIS DA BIOTECNOLOGIA" },
                { "2011.15", "Geneticista" },
                { "2030.10", "Entomologista" },
                { "2030.10", "Entomólogo" },
                { "2030.10", "Ofiologista" },
                { "2030.10", "Ornitólogo" },
                { "2033.10", "Pesquisador de medicina básica" },
                { "2030.15", "Bacteriologista" },
                { "2030.20", "Fisiologista (exceto médico)" },
                { "2030.25", "Fenologista" },
                { "2131.50", "Físico hospitalar" },
                { "2131.50", "Físico médico" },
                { "2211.05", "Biologista" },
                { "2211.05", "Biomédico" },
                { "2231", "MÉDICOS" },
                { "2232", "CIRURGIÕES-DENTISTAS" },
                { "2231.01", "Médico acupunturista" },
                { "2231.02", "Médico alergista" },
                { "2231.02", "Médico alergista e imunologista" },
                { "2231.02", "Médico imunologista" },
                { "2231.03", "Médico anatomopatologista" },
                { "2231.03", "Patologista" },
                { "2231.04", "Anestesiologista" },
                { "2231.04", "Anestesista" },
                { "2235", "ENFERMEIROS" },
                { "2231.04", "Médico anestesiologista" },
                { "2231.04", "Médico anestesista" },
                { "2231.05", "Angiologista" },
                { "2232.04", "Cirurgião dentista - auditor" },
                { "2231.05", "Médico angiologista" },
                { "2236", "PROFISSIONAIS DA FISIOTERAPIA E AFINS" },
                { "2231.06", "Cardiologista" },
                { "2231.06", "Médico cardiologista" },
                { "2231.06", "Médico do coração" },
                { "2237", "NUTRICIONISTAS" },
                { "2231.07", "Cirurgião cardiovascular" },
                { "2238", "FONOAUDIÓLOGOS" },
                { "2231.07", "Médico cirurgião cardiovascular" },
                { "2233.05", "Médico veterinário" },
                { "2233.05", "Médico veterinário de saúde pública" },
                { "2233.05", "Médico veterinário sanitarista" },
                { "2231.08", "Cirurgião de cabeça e pescoço" },
                { "2234.05", "Farmacêutico" },
                { "2234.05", "Farmacêutico homeopata" },
                { "2234.05", "Farmacêutico hospitalar" },
                { "2231.08", "Médico cirurgião de cabeça e pescoço" },
                { "2232.08", "Cirurgião dentista - clínico geral" },
                { "2231.09", "Cirurgião do aparelho digestivo" },
                { "2231.09", "Cirurgião gastroenterológico" },
                { "2232.08", "Dentista" },
                { "2235.05", "Enfermeiro" },
                { "2231.09", "Médico cirurgião do aparelho digestivo" },
                { "2232.08", "Odontologista" },
                { "2232.08", "Odontólogo" },
                { "2231.10", "Cirurgião geral" },
                { "2236.05", "Fisioterapeuta" },
                { "2236.05", "Fisioterapeuta acupunturista" },
                { "2231.10", "Médico cirurgião" },
                { "2231.10", "Médico cirurgião geral" },
                { "2237.05", "Auxiliar de dietista" },
                { "2237.05", "Auxiliar de nutrição e dietética" },
                { "2231.11", "Cirurgião pediátrico" },
                { "2231.11", "Médico cirurgião pediátrico" },
                { "2231.12", "Cirurgião plástico" },
                { "2231.12", "Médico cirurgião plástico" },
                { "2232.12", "Cirurgião dentista - endodontista" },
                { "2231.13", "Cirurgião torácico" },
                { "2231.13", "Médico cirurgião torácico" },
                { "2232.12", "Odontólogo-endodontista" },
                { "2235.10", "Enfermeiro auditor" },
                { "2231.14", "Médico citopatologista" },
                { "2241.05", "Avaliador físico" },
                { "2231.15", "Clínico geral" },
                { "2231.15", "Médico clínico" },
                { "2231.15", "Médico clínico geral" },
                { "2231.15", "Médico especialista em clínica médica" },
                { "2231.15", "Médico especialista em medicina interna" },
                { "2231.15", "Médico internista" },
                { "2231.16", "Médico comunitário" },
                { "2231.16", "Médico de" },
                { "2231.16", "Médico de saúde da família" },
                { "2237.10", "Nutricionista" },
                { "2237.10", "Nutricionista (saúde pública)" },
                { "2232.16", "Cirurgião dentista - epidemiologista" },
                { "2231.17", "Dermatologista" },
                { "2238.10", "Fonoaudiólogo" },
                { "2231.17", "Hansenólogo" },
                { "2231.17", "Médico dermatologista" },
                { "2231.18", "Médico do trabalho" },
                { "2235.15", "Enfermeiro de bordo" },
                { "2231.19", "Médico em eletroencefalografia" },
                { "2231.20", "Médico em endoscopia" },
                { "2231.20", "Médico endoscopista" },
                { "2236.15", "Ortoptista" },
                { "2232.20", "Cirurgião dentista - estomatologista" },
                { "2231.21", "Médico do tráfego" },
                { "2231.21", "Médico em medicina de tráfego" },
                { "2231.22", "Intensivista" },
                { "2231.22", "Médico em medicina intensiva" },
                { "2231.23", "Médico em medicina nuclear" },
                { "2231.23", "Médico nuclear" },
                { "2235.20", "Enfermeiro de centro cirúrgico" },
                { "2231.24", "Imagenologista" },
                { "2235.20", "Instrumentador cirúrgico (enfermeiro)" },
                { "2231.24", "Médico angioradiologista" },
                { "2231.24", "Médico densitometrista" },
                { "2231.24", "Médico em diagnóstico por imagem" },
                { "2231.24", "Médico em radiologia e diagnóstico por imagem" },
                { "2231.24", "Médico neuroradiologista" },
                { "2231.24", "Médico radiologista" },
                { "2231.24", "Médico radiologista intervencionista" },
                { "2231.24", "Radiologista" },
                { "2231.24", "Ultra-sonografista" },
                { "2232.24", "Cirurgião dentista - implantodontista" },
                { "2231.25", "Médico endocrinologista" },
                { "2231.25", "Médico endocrinologista e metabologista" },
                { "2231.25", "Médico metabolista" },
                { "2231.25", "Metabolista" },
                { "2231.25", "Metabologista" },
                { "2236.20", "Peripatologista" },
                { "2236.20", "Terapeuta ocupacional" },
                { "2231.26", "Fisiatra" },
                { "2231.26", "Médico fisiatra" },
                { "2231.27", "Foniatra" },
                { "2231.27", "Médico foniatra" },
                { "2231.28", "Médico gastroenterologista" },
                { "2232.28", "Cirurgião dentista - odontogeriatra" },
                { "2232.28", "Dentista de idosos" },
                { "2232.28", "Dentista de terceira idade" },
                { "2235.25", "Enfermeiro de terapia intensiva" },
                { "2235.25", "Enfermeiro intensivista" },
                { "2231.29", "Médico alopata" },
                { "2231.29", "Médico em medicina interna" },
                { "2231.29", "Médico generalista" },
                { "2231.29", "Médico militar" },
                { "2231.30", "Médico geneticista" },
                { "2231.31", "Geriatra" },
                { "2231.31", "Gerontologista" },
                { "2231.31", "Gerontólogo" },
                { "2231.31", "Médico geriatra" },
                { "2231.32", "Cirurgião ginecológico" },
                { "2231.32", "Ginecologista" },
                { "2231.32", "Médico de mulheres" },
                { "2231.32", "Médico ginecologista" },
                { "2231.32", "Médico ginecologista e obstetra" },
                { "2231.32", "Médico obstetra" },
                { "2232.32", "Cirurgião dentista - odontologista legal" },
                { "2231.33", "Hematologista" },
                { "2231.33", "Médico hematologista" },
                { "2235.30", "Enfermeiro do trabalho" },
                { "2231.34", "Hemoterapeuta" },
                { "2231.34", "Médico em hemoterapia" },
                { "2231.34", "Médico hemoterapeuta" },
                { "2231.35", "Médico homeopata" },
                { "2231.36", "Infectologista" },
                { "2231.36", "Médico de doenças infecciosas e parasitárias" },
                { "2231.36", "Médico infectologista" },
                { "2232.36", "Cirurgião dentista - odontopediatra" },
                { "2232.36", "Dentista de criança" },
                { "2231.37", "Médico legista" },
                { "2232.36", "Odontopediatra" },
                { "2231.38", "Cirurgião de mama" },
                { "2231.38", "Cirurgião mastologista" },
                { "2231.38", "Mastologista" },
                { "2231.38", "Médico mastologista" },
                { "2235.35", "Enfermeiro nefrologista" },
                { "2231.39", "Médico nefrologista" },
                { "2231.40", "Médico neurocirurgião" },
                { "2231.40", "Médico neurocirurgião pediátrico" },
                { "2231.40", "Neurocirurgião" },
                { "2231.40", "Neurocirurgião pediátrico" },
                { "2232.40", "Cirurgião dentista - ortopedista e ortodontista" },
                { "2232.40", "Dentista de aparelho" },
                { "2231.41", "Médico neurofisiologista" },
                { "2231.41", "Neurofisiologista" },
                { "2232.40", "Ortodontista" },
                { "2232.40", "Ortodontólogo" },
                { "2232.40", "Ortopedista maxilar" },
                { "2231.42", "Médico neurologista" },
                { "2231.42", "Médico neuropediatra" },
                { "2231.42", "Neurologista" },
                { "2231.42", "Neuropediatra" },
                { "2231.43", "Médico nutrologista" },
                { "2231.43", "Médico nutrólogo" },
                { "2231.43", "Nutrologista" },
                { "2231.44", "Cirurgião oftalmológico" },
                { "2235.40", "Enfermeiro de berçário" },
                { "2235.40", "Enfermeiro neonatologista" },
                { "2231.44", "Médico oftalmologista" },
                { "2231.44", "Oftalmologista" },
                { "2232.44", "Cirurgião dentista - patologista bucal" },
                { "2231.45", "Médico cancerologista" },
                { "2231.45", "Médico oncologista" },
                { "2231.45", "Oncologista" },
                { "2231.46", "Cirurgião de mão" },
                { "2231.46", "Cirurgião ortopedista" },
                { "2231.46", "Cirurgião traumatologista" },
                { "2231.46", "Médico cirurgião de mão" },
                { "2231.46", "Médico de medicina esportiva" },
                { "2231.46", "Médico ortopedista" },
                { "2231.46", "Médico ortopedista e traumatologista" },
                { "2231.46", "Médico traumatologista" },
                { "2231.46", "Ortopedista" },
                { "2231.46", "Traumatologista" },
                { "2231.47", "Cirurgião otorrinolaringologista" },
                { "2231.47", "Médico otorrinolaringologista" },
                { "2231.47", "Otorrino" },
                { "2231.47", "Otorrinolaringologista" },
                { "2231.48", "Médico laboratorista" },
                { "2231.48", "Médico patologista" },
                { "2231.48", "Médico patologista clínico" },
                { "2231.48", "Patologista clínico" },
                { "2232.48", "Cirurgião dentista - periodontista" },
                { "2232.48", "Dentista de gengivas" },
                { "2235.45", "Enfermeira parteira" },
                { "2235.45", "Enfermeiro obstétrico" },
                { "2231.49", "Médico de criança" },
                { "2231.49", "Médico pediatra" },
                { "2231.49", "Neonatologista" },
                { "2231.49", "Pediatra" },
                { "2232.48", "Periodontista" },
                { "2231.50", "Médico perito" },
                { "2231.51", "Médico pneumologista" },
                { "2231.51", "Médico pneumotisiologista" },
                { "2231.51", "Pneumologista" },
                { "2231.51", "Pneumotisiologista" },
                { "2231.51", "Tisiologista" },
                { "2231.52", "Cirurgião proctologista" },
                { "2231.52", "Médico proctologista" },
                { "2231.52", "Proctologista" },
                { "2232.52", "Cirurgião dentista - protesiólogo bucomaxilofacial" },
                { "2231.53", "Médico psicanalista" },
                { "2231.53", "Médico psicoterapeuta" },
                { "2231.53", "Médico psiquiatra" },
                { "2231.53", "Neuropsiquiatra" },
                { "2232.52", "Protesista bucomaxilofacial" },
                { "2231.53", "Psiquiatra" },
                { "2235.50", "Enfermeiro psiquiátrico" },
                { "2231.54", "Médico em radioterapia" },
                { "2231.54", "Médico radioterapeuta" },
                { "2231.55", "Médico reumatologista" },
                { "2231.55", "Reumatologista" },
                { "2231.56", "Epidemiologista" },
                { "2231.56", "Médico de saúde pública" },
                { "2231.56", "Médico epidemiologista" },
                { "2231.56", "Médico higienista" },
                { "2231.56", "Médico sanitarista" },
                { "2231.57", "Andrologista" },
                { "2232.56", "Cirurgião dentista - protesista" },
                { "2231.57", "Cirurgião urológico" },
                { "2231.57", "Cirurgião urologista" },
                { "2231.57", "Médico urologista" },
                { "2232.56", "Odontólogo protesista" },
                { "2232.56", "Protesista" },
                { "2231.57", "Urologista" },
                { "2235.55", "Enfermeiro puericultor e pediátrico" },
                { "2232.60", "Cirurgião dentista - radiologista" },
                { "2232.60", "Odontoradiologista" },
                { "2235.60", "Enfermeiro de saúde publica" },
                { "2235.60", "Enfermeiro sanitarista" },
                { "2232.64", "Cirurgião dentista - reabilitador oral" },
                { "2232.68", "Cirurgião dentista - traumatologista bucomaxilofacial" },
                { "2232.68", "Cirurgião oral e maxilofacial" },
                { "2232.68", "Odontólogo (cirurgia e traumatologia bucomaxilofacial)" },
                { "2232.72", "Cirurgião dentista de saúde coletiva" },
                { "2232.72", "Dentista de sáude coletiva" },
                { "2232.72", "Odontologista social" },
                { "2232.72", "Odontólogo de saúde coletiva" },
                { "2232.72", "Odontólogo de saúde pública" },
                { "2394.25", "Psicopedagogo" },
                { "2515", "PSICÓLOGOS E PSICANALISTAS" },
                { "2515.05", "Psicólogo da educação" },
                { "2515.05", "Psicólogo educacional" },
                { "2515.05", "Psicólogo escolar" },
                { "2516.05", "Assistente social" },
                { "2515.10", "Psicólogo acupunturista" },
                { "2515.10", "Psicólogo clínico" },
                { "2515.10", "Psicólogo da saúde" },
                { "2515.10", "Psicoterapeuta" },
                { "2515.10", "Terapeuta" },
                { "2521.05", "Administrador" },
                { "2515.15", "Psicólogo desportivo" },
                { "2515.15", "Psicólogo do esporte" },
                { "2515.20", "Psicólogo hospitalar" },
                { "2515.25", "Psicólogo criminal" },
                { "2515.25", "Psicólogo forense" },
                { "2515.25", "Psicólogo jurídico" },
                { "2515.30", "Psicólogo social" },
                { "2515.35", "Psicólogo do trânsito" },
                { "2515.40", "Psicólogo do trabalho" },
                { "2515.40", "Psicólogo organizacional" },
                { "2515.45", "Neuropsicólogo" },
                { "2515.50", "Psicanalista" },
                { "3011.05", "Laboratorista - exclusive análises clínicas" },
                { "3135.05", "Técnico em laboratório óptico" },
                { "3134.10", "Técnico em instrumentação" },
                { "3225", "TÉCNICOS EM PRÓTESES ORTOPÉDICAS" },
                { "3221.05", "Acupunturista" },
                { "3221.05", "Fitoterapeuta" },
                { "3221.05", "Terapeuta naturalista" },
                { "3221.05", "Terapeuta oriental" },
                { "3222.05", "Técnico de enfermagem" },
                { "3222.05", "Técnico de enfermagem socorrista" },
                { "3222.05", "Técnico em hemotransfusão" },
                { "3223.05", "Óptico oftálmico" },
                { "3223.05", "optico optometrista" },
                { "3223.05", "optico protesista" },
                { "3224.05", "Técnico em higiene dental" },
                { "3225.05", "Protesista (técnico)" },
                { "3225.05", "Técnico ortopédico" },
                { "3226.05", "Técnico em imobilizações do aparelho locomotor" },
                { "3226.05", "Técnico em imobilizações gessadas" },
                { "3222.10", "Técnico de enfermagem de terapia intensiva" },
                { "3222.10", "Técnico em hemodiálise" },
                { "3222.10", "Técnico em UTI" },
                { "3224.10", "Protético dentário" },
                { "3221.15", "Homeopata (exceto médico)" },
                { "3221.15", "Terapeuta crâneo-sacral" },
                { "3221.15", "Terapeuta holístico" },
                { "3221.15", "Terapeuta manual" },
                { "3221.15", "Terapeuta mio-facial" },
                { "3222.15", "Técnico de enfermagem do trabalho" },
                { "3222.15", "Técnico de enfermagem em saúde ocupacional" },
                { "3222.15", "Técnico de enfermagem ocupacional" },
                { "3224.15", "Atendente de clínica dentária" },
                { "3224.15", "Atendente de Consultório Dentário" },
                { "3224.15", "Atendente de gabinete dentário" },
                { "3224.15", "Atendente de serviço odontólogico" },
                { "3224.15", "Atendente odontológico" },
                { "3224.15", "Auxiliar de dentista" },
                { "3222.20", "Técnico de enfermagem em saúde mental" },
                { "3222.20", "Técnico de enfermagem psiquiátrica" },
                { "3224.20", "Auxiliar de Prótese Dentária" },
                { "3241.05", "Operador de eletroencefalógrafo" },
                { "3222.25", "Instrumentador cirúrgico" },
                { "3222.25", "Instrumentador em cirurgia" },
                { "3222.25", "Instrumentadora cirúrgica" },
                { "3242.05", "Técnico de laboratório de análises clínicas" },
                { "3242.05", "Técnico em patologia clínica" },
                { "3241.10", "Operador de eletrocardiógrafo" },
                { "3251", "TÉCNICO EM FARMÁCIA E EM MANIPULAÇÃO FARMACÊUTICA" },
                { "3222.30", "Auxiliar de enfermagem" },
                { "3222.30", "Auxiliar de enfermagem de central de material esterelizado (CME)" },
                { "3222.30", "Auxiliar de enfermagem de centro cirúrgico" },
                { "3222.30", "Auxiliar de enfermagem de clínica médica" },
                { "3222.30", "Auxiliar de enfermagem de hospital" },
                { "3222.30", "Auxiliar de enfermagem de saúde pública" },
                { "3222.30", "Auxiliar de enfermagem em hemodiálise" },
                { "3222.30", "Auxiliar de enfermagem em home care" },
                { "3222.30", "Auxiliar de enfermagem em nefrologia" },
                { "3222.30", "Auxiliar de enfermagem em saúde mental" },
                { "3222.30", "Auxiliar de enfermagem socorrista" },
                { "3222.30", "Auxiliar de ginecologia" },
                { "3222.30", "Auxiliar de hipodermia" },
                { "3222.30", "Auxiliar de obstetrícia" },
                { "3222.30", "Auxiliar de oftalmologia" },
                { "3222.30", "Auxiliar em hemotransfusão" },
                { "3242.10", "Auxiliar técnico de laboratório de análises clínicas" },
                { "3242.10", "Auxiliar técnico em patologia clínica" },
                { "3251.05", "Auxiliar técnico em laboratório de farmácia" },
                { "3241.15", "Técnico em hemodinâmica" },
                { "3241.15", "Técnico em mamografia" },
                { "3241.15", "Técnico em radiologia" },
                { "3241.15", "Técnico em radiologia e imagenologia" },
                { "3241.15", "Técnico em radiologia médica" },
                { "3241.15", "Técnico em radiologia odontológica" },
                { "3241.15", "Técnico em tomografia" },
                { "3222.35", "Auxiliar de enfermagem do trabalho" },
                { "3222.35", "Auxiliar de enfermagem em saúde ocupacional" },
                { "3222.35", "Auxiliar de enfermagem ocupacional" },
                { "3251.10", "Técnico em laboratório de farmácia" },
                { "3222.40", "Auxiliar de saúde (navegação marítima)" },
                { "3222.40", "Auxiliar de saúde marítimo" },
                { "3253.10", "Técnico em imunobiológicos" },
                { "3251.15", "Técnico em Farmácia" },
                { "3522", "AGENTES DA SAÚDE E DO MEIO AMBIENTE" },
                { "3522.10", "Agente de saneamento" },
                { "3522.10", "Agente de saúde pública" },
                { "4110.10", "Assistente administrativo" },
                { "4110.10", "Assistente técnico - no serviço público" },
                { "4110.10", "Assistente técnico administrativo" },
                { "4151.20", "Fitotecário" },
                { "4221.05", "Atendente de clínica veterinária" },
                { "4221.05", "Atendente de consultório veterinário" },
                { "4221.10", "Atendente de ambulatório" },
                { "4221.10", "Atendente de clínica médica" },
                { "4221.10", "Atendente de consultório médico" },
                { "4221.15", "Atendente de seguro saúde" },
                { "5151", "AGENTES COMUNITÁRIOS DE SAÚDE E AFINS" },
                { "5152", "AUXILIARES DE LABORATÓRIO DA SAÚDE" },
                { "5132.20", "Cozinheiro de hospital" },
                { "5151.05", "Agente de saúde" },
                { "5151.05", "Visitador de saúde" },
                { "5151.05", "Visitador de saúde em domicílio" },
                { "5151.10", "Atendente de berçário" },
                { "5151.10", "Atendente de centro cirúrgico" },
                { "5151.10", "Atendente de enfermagem" },
                { "5151.10", "Atendente de enfermagem no serviço doméstico" },
                { "5151.10", "Atendente de hospital" },
                { "5151.10", "Atendente de serviço de saúde" },
                { "5151.10", "Atendente de serviço médico" },
                { "5151.10", "Atendente hospitalar" },
                { "5151.10", "Atendente-enfermeiro" },
                { "5152.10", "Auxiliar de farmácia de manipulação" },
                { "5134.30", "Copeiro de hospital" },
                { "5151.15", "Assistente de parto" },
                { "5151.20", "Auxiliar de sanitarista" },
                { "5151.20", "Imunizador" },
                { "5162.10", "Acompanhante de idosos" },
                { "5168.05", "Radioestesista" },
                { "5161.15", "Auxiliar de estética" },
                { "5161.35", "Massoterapeuta" },
                { "5193.05", "Auxiliar de enfermagem veterinária" },
                { "5193.05", "Auxiliar de veterinário" },
                { "5193.05", "Enfermeiro veterinário" },
                { "5211.30", "Atendente de farmácia - balconista" },
                { "6233.15", "Auxiliar de incubação" },
                { "6233.15", "Operador de incubadora" },
                { "7411.05", "Instrumentista de precisão" },
                { "7664.20", "Auxiliar de radiologia (revelação fotográfica)" },
                { "7823.10", "Motorista de ambulância" },
                { "9151.05", "Instrumentista de laboratório (manutenção)" },
                { "9153.05", "Técnico em manutenção de equipamentos e instrumentos médicohospitalares" },
            };

            foreach (var eachTuple in tissEspecialidades)
                db.SYS_MedicalSpecialty.AddObject(new SYS_MedicalSpecialty { Code = eachTuple.Item1, Name = eachTuple.Item2 });

            db.SaveChanges();
        }

        public static SYS_MedicalProcedure[] CreateFakeMedicalProcedures(CerebelloEntities db)
        {
            var tissConselhoProfissional = new ListOfTuples<string, string>
            {
                { "4.03.04.36-1", "Hemograma com contagem de plaquetas ou frações" },
                { "4.01.03.23-4", "Eletrencefalograma em vigília, e sono espontâneo ou induzido" },
                { "4.01.03.55-2", "Posturografia" },
                { "3.07.15.26-1", "Retirada de corpo estranho - tratamento cirúrgico" },
                { "1.01.06.01-4", "Aconselhamento genético" },
                { "2.01.01.22-8", "Acompanhamento clínico ambulatorial pós-transplante de medula óssea" },
                { "2.01.03.45-0", "Paraplegia e tetraplegia" },
                { "2.01.03.46-8", "Parkinson" },
                { "3.01.01.26-3", "Dermoabrasão de lesões cutâneas" },
                { "3.03.11.01-2", "Biópsia de músculos" },
                { "3.03.11.02-0", "Cirurgia com sutura ajustável" },
            };

            foreach (var eachTuple in tissConselhoProfissional)
                db.SYS_MedicalProcedure.AddObject(new SYS_MedicalProcedure { Code = eachTuple.Item1, Name = eachTuple.Item2 });

            db.SaveChanges();

            return db.SYS_MedicalProcedure.ToArray();
        }

        class ListOfTuples<T1, T2> : List<Tuple<T1, T2>>
        {
            public void Add(T1 t1, T2 t2)
            {
                this.Add(new Tuple<T1, T2>(t1, t2));
            }
        }

        static object locker = new object();
        static Cbhpm cbhpm;

        public static void Initialize_SYS_MedicalProcedures(CerebelloEntities db, string pathOfTxt, int maxCount = int.MaxValue, Action<int, int> progress = null)
        {
            progress = progress ?? ((x, y) => { });

            // Adding CBHPM medical procedures.
            if (cbhpm == null)
                lock (locker)
                    if (cbhpm == null)
                        cbhpm = Cbhpm.LoadData(pathOfTxt);

            var max = Math.Min(maxCount, cbhpm.Items.Values.OfType<Cbhpm.Proc>().Count());

            int count = 0;
            foreach (var eachCbhpmProc in cbhpm.Items.Values.OfType<Cbhpm.Proc>())
            {
                if (count >= maxCount)
                    break;

                progress(count, max);

                var item = db.SYS_MedicalProcedure.CreateObject();
                item.Code = eachCbhpmProc.Codigo;
                item.Name = eachCbhpmProc.Nome;
                db.SYS_MedicalProcedure.AddObject(item);

                if (count % 100 == 0)
                    db.SaveChanges();

                count++;
            }

            progress(count, max);

            db.SaveChanges();
        }

        /// <summary>
        /// Clears all data in the database.
        /// </summary>
        /// <param name="db"></param>
        public static void ClearAllData(CerebelloEntities db, bool repopulateSysTablesWithDefaults = false, string rootCerebelloPath = null)
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

            if (repopulateSysTablesWithDefaults)
            {
                Console.WriteLine("Initialize_SYS_MedicalEntity");
                Firestarter.Initialize_SYS_MedicalEntity(db);
                Console.WriteLine("Initialize_SYS_MedicalSpecialty");
                Firestarter.Initialize_SYS_MedicalEntity(db);
                Console.WriteLine("Initialize_SYS_MedicalProcedures");
                Firestarter.Initialize_SYS_MedicalProcedures(
                    db,
                    Path.Combine(rootCerebelloPath, @"DB\cbhpm_2010.txt"));
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
            var scripts = Regex.Split(script, @"(?<=(?:[\r\n]|^)+\s*)GO(?=\s*(?:[\r\n]|$))", RegexOptions.IgnoreCase);

            foreach (var eachScript in scripts)
                db.ExecuteStoreCommand(eachScript);
        }

        /// <summary>
        /// Attaches the given database.
        /// </summary>
        internal static bool AttachLocalDatabase(CerebelloEntities db)
        {
            var sqlConn1 = (SqlConnection)((EntityConnection)db.Connection).StoreConnection;
            var sqlConn2 = new SqlConnectionStringBuilder(sqlConn1.ConnectionString);
            sqlConn2.InitialCatalog = "";
            var connStr = sqlConn2.ToString();
            var dbName = sqlConn1.Database;

            // attaches the database
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                try
                {
                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText =
                        string.Format(@"CREATE DATABASE CerebelloTEST ON 
                    ( FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL10_50.SQLEXPRESS\MSSQL\DATA\{0}.mdf' )
                     FOR ATTACH ;", dbName);

                        command.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    // probably the database exists already because a previous test failed.. let's move on
                    return false;
                }
                conn.Close();

                return true;
            }
        }

        /// <summary>
        /// Detaches the given database.
        /// </summary>
        internal static void DetachLocalDatabase(CerebelloEntities db)
        {
            var sqlConn1 = (SqlConnection)((EntityConnection)db.Connection).StoreConnection;
            var sqlConn2 = new SqlConnectionStringBuilder(sqlConn1.ConnectionString);
            sqlConn2.InitialCatalog = "";
            var connStr = sqlConn2.ToString();
            var dbName = sqlConn1.Database;

            SqlConnection.ClearAllPools();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("", conn);
                cmd.CommandText = @"sp_detach_db CerebelloTEST";
                cmd.ExecuteNonQuery();
            }
        }

        public static void InitializeDatabaseWithSystemData(CerebelloEntities db, int medicalProceduresMaxCount = 0, string rootCerebelloPath = null, Action<int, int> progress = null)
        {
            Firestarter.Initialize_SYS_MedicalEntity(db);
            Firestarter.Initialize_SYS_MedicalSpecialty(db);
            if (medicalProceduresMaxCount > 0)
                Firestarter.Initialize_SYS_MedicalProcedures(
                    db,
                    Path.Combine(rootCerebelloPath, @"DB\cbhpm_2010.txt"),
                    medicalProceduresMaxCount,
                    progress);
        }
    }
}
