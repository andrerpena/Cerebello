using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Data.Objects;
using System.Linq;
using Cerebello.Model;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code.Access
{
    /// <summary>
    /// ObjectContext wrapper with global rules of access to objects stored in the database.
    /// These rules are the most relaxed access rules.
    /// Most of them will only check for objects being of the same practice.
    /// Some may restrict a little more.
    /// </summary>
    public class CerebelloEntitiesAccessFilterWrapper : IDisposable
    {
        private readonly CerebelloEntities db;
        private User user;
        private Practice practice;

        public CerebelloEntitiesAccessFilterWrapper([NotNull] CerebelloEntities db)
        {
            if (db == null) throw new ArgumentNullException("db");
            this.db = db;
        }

        public User SetCurrentUserById(int userId)
        {
            this.user = this.db.Users.Include("Practice").SingleOrDefault(u => u.Id == userId);

            if (this.user != null)
                this.practice = this.user.Practice;

            this.AccountDisabled = this.practice == null || this.practice.AccountDisabled;

            return this.user;
        }

        private static readonly ConcurrentDictionary<Type, bool> hasPracticeId = new ConcurrentDictionary<Type, bool>();

        public int SaveChanges()
        {
            // checking changed elements to see if there is something wrong
            foreach (var objectStateEntry in this.db.ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Modified))
            {
                var obj = objectStateEntry.Entity;
                bool isPracticeIdInvalid = false;
                if (obj != null)
                {
                    var type = obj.GetType();
                    if (hasPracticeId.GetOrAdd(type, t => t.GetProperty("PracticeId") != null && t.GetProperty("PracticeId").PropertyType == typeof(int)))
                    {
                        dynamic dyn = obj;
                        if ((int)dyn.PracticeId != this.practice.Id)
                            isPracticeIdInvalid = true;
                    }
                }

                if (isPracticeIdInvalid)
                    throw new Exception("Invalid value for 'PracticeId' property.");
            }

            return this.db.SaveChanges();
        }

        public DbConnection Connection { get { return this.db.Connection; } }

        public bool AccountDisabled { get; set; }

        public FilteredObjectSetWrapper<AccountContract> AccountContracts
        {
            get { return new FilteredObjectSetWrapper<AccountContract>(this.db.AccountContracts, s => s.Where(ac => ac.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<MedicineActiveIngredient> ActiveIngredients
        {
            get { return new FilteredObjectSetWrapper<MedicineActiveIngredient>(this.db.MedicineActiveIngredients, s => s.Where(ai => ai.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<Address> Addresses
        {
            get { return new FilteredObjectSetWrapper<Address>(this.db.Addresses, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<Administrator> Administrators
        {
            get { return new FilteredObjectSetWrapper<Administrator>(this.db.Administrators, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        // todo: rename table to Anamnesis, and plural is Anamneses (like Diagnosis and Diagnoses)
        public FilteredObjectSetWrapper<Anamnese> Anamnese
        {
            get { return new FilteredObjectSetWrapper<Anamnese>(this.db.Anamnese, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<PhysicalExamination> PhysicalExaminations
        {
            get { return new FilteredObjectSetWrapper<PhysicalExamination>(this.db.PhysicalExaminations, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<Appointment> Appointments
        {
            get { return new FilteredObjectSetWrapper<Appointment>(this.db.Appointments, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<CFG_DayOff> CFG_DayOff
        {
            get { return new FilteredObjectSetWrapper<CFG_DayOff>(this.db.CFG_DayOff, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<CFG_Documents> CFG_Documents
        {
            get { return new FilteredObjectSetWrapper<CFG_Documents>(this.db.CFG_Documents, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<CFG_Schedule> CFG_Schedule
        {
            get { return new FilteredObjectSetWrapper<CFG_Schedule>(this.db.CFG_Schedule, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<ChatMessage> ChatMessages
        {
            get { return new FilteredObjectSetWrapper<ChatMessage>(this.db.ChatMessages, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<Diagnosis> Diagnoses
        {
            get { return new FilteredObjectSetWrapper<Diagnosis>(this.db.Diagnoses, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<File> Files
        {
            get { return new FilteredObjectSetWrapper<File>(this.db.Files, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<PatientFile> PatientFiles
        {
            get { return new FilteredObjectSetWrapper<PatientFile>(this.db.PatientFiles, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<Doctor> Doctors
        {
            get { return new FilteredObjectSetWrapper<Doctor>(this.db.Doctors, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<ExaminationRequest> ExaminationRequests
        {
            get { return new FilteredObjectSetWrapper<ExaminationRequest>(this.db.ExaminationRequests, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<ExaminationResult> ExaminationResults
        {
            get { return new FilteredObjectSetWrapper<ExaminationResult>(this.db.ExaminationResults, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public ObjectSet<GLB_Token> GLB_Token
        {
            get { return this.db.GLB_Token; }
        }

        public FilteredObjectSetWrapper<HealthInsurance> HealthInsurances
        {
            get { return new FilteredObjectSetWrapper<HealthInsurance>(this.db.HealthInsurances, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public ObjectSet<Holiday> Holidays
        {
            get { return this.db.Holidays; }
        }

        public FilteredObjectSetWrapper<Laboratory> Laboratories
        {
            get { return new FilteredObjectSetWrapper<Laboratory>(this.db.Laboratories, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<Leaflet> Leaflets
        {
            get { return new FilteredObjectSetWrapper<Leaflet>(this.db.Leaflets, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<MedicalCertificate> MedicalCertificates
        {
            get { return new FilteredObjectSetWrapper<MedicalCertificate>(this.db.MedicalCertificates, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<MedicalCertificateField> MedicalCertificateFields
        {
            get { return new FilteredObjectSetWrapper<MedicalCertificateField>(this.db.MedicalCertificateFields, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<Medicine> Medicines
        {
            get { return new FilteredObjectSetWrapper<Medicine>(this.db.Medicines, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<ModelMedicalCertificate> ModelMedicalCertificates
        {
            get { return new FilteredObjectSetWrapper<ModelMedicalCertificate>(this.db.ModelMedicalCertificates, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<ModelMedicalCertificateField> ModelMedicalCertificateFields
        {
            get { return new FilteredObjectSetWrapper<ModelMedicalCertificateField>(this.db.ModelMedicalCertificateFields, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<Patient> Patients
        {
            get { return new FilteredObjectSetWrapper<Patient>(this.db.Patients, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<Person> Persons
        {
            get { return new FilteredObjectSetWrapper<Person>(this.db.People, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public ObjectSet<Practice> Practices
        {
            get { return this.db.Practices; }
        }

        public FilteredObjectSetWrapper<Receipt> Receipts
        {
            get { return new FilteredObjectSetWrapper<Receipt>(this.db.Receipts, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<ReceiptMedicine> ReceiptMedicines
        {
            get { return new FilteredObjectSetWrapper<ReceiptMedicine>(this.db.ReceiptMedicines, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<Secretary> Secretaries
        {
            get { return new FilteredObjectSetWrapper<Secretary>(this.db.Secretaries, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<DiagnosticHypothesis> DiagnosticHypotheses
        {
            get { return new FilteredObjectSetWrapper<DiagnosticHypothesis>(this.db.DiagnosticHypotheses, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        /// <summary>
        /// Gets the users of the current account, but only if the account is enabled.
        /// When disabled, none will be returned. This kills app usage when account is disabled.
        /// </summary>
        public FilteredObjectSetWrapper<User> Users
        {
            get { return new FilteredObjectSetWrapper<User>(this.db.Users, s => s.Where(a => a.PracticeId == this.user.PracticeId && !this.AccountDisabled)); }
        }

        public FilteredObjectSetWrapper<Notification> Notifications
        {
            get { return new FilteredObjectSetWrapper<Notification>(this.db.Notifications, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public FilteredObjectSetWrapper<DropboxInfo> DropboxInfos
        {
            get { return new FilteredObjectSetWrapper<DropboxInfo>(this.db.DropboxInfoes, s => s.Where(a => a.PracticeId == this.user.PracticeId)); }
        }

        public ObjectSet<SYS_ActiveIngredient> SYS_ActiveIngredient
        {
            get { return this.db.SYS_ActiveIngredient; }
        }

        public ObjectSet<SYS_Cid10> SYS_Cid10
        {
            get { return this.db.SYS_Cid10; }
        }

        public ObjectSet<SYS_ContractType> SYS_ContractType
        {
            get { return this.db.SYS_ContractType; }
        }

        public ObjectSet<SYS_Holiday> SYS_Holiday
        {
            get { return this.db.SYS_Holiday; }
        }

        public ObjectSet<SYS_Laboratory> SYS_Laboratory
        {
            get { return this.db.SYS_Laboratory; }
        }

        public ObjectSet<SYS_Leaflet> SYS_Leaflet
        {
            get { return this.db.SYS_Leaflet; }
        }

        public ObjectSet<SYS_MedicalEntity> SYS_MedicalEntity
        {
            get { return this.db.SYS_MedicalEntity; }
        }

        public ObjectSet<SYS_MedicalProcedure> SYS_MedicalProcedure
        {
            get { return this.db.SYS_MedicalProcedure; }
        }

        public ObjectSet<SYS_MedicalSpecialty> SYS_MedicalSpecialty
        {
            get { return this.db.SYS_MedicalSpecialty; }
        }

        public ObjectSet<SYS_Medicine> SYS_Medicine
        {
            get { return this.db.SYS_Medicine; }
        }

        public void Dispose()
        {
            this.db.Dispose();
        }
    }
}
