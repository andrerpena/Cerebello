using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using System.Xml.Serialization;
using AutoMapper;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Helpers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using SmartRecords;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class ReportController : DoctorController
    {
        [SelfPermission]
        public ContentResult ExportDoctorXml()
        {
            var xml = ExportDoctorXml(this.db, this.DbPractice, this.Doctor);

            return this.Content(xml, "text/xml");
        }

        internal static string ExportDoctorXml(CerebelloEntitiesAccessFilterWrapper db, Practice practice, Doctor doctor)
        {
            var rg = new ReportData(db, practice);

            var doctorData = rg.GetReportDataSourceForXml(doctor, null);

            var stringBuilder = new StringBuilder();
            using (var writer = new StringWriter(stringBuilder))
            {
                var xs = new XmlSerializer(typeof(ReportData.XmlDoctorData));
                xs.Serialize(writer, doctorData);
            }

            return stringBuilder.ToString();
        }

        internal static MemoryStream ExportPatientsPdf(int? patientId, CerebelloEntitiesAccessFilterWrapper db, Practice practice, Doctor doctor)
        {
            var reportDataSource = new ReportData(db, practice).GetReportDataSourceForPdf(doctor, patientId);
            var watermark = new ReportFrame(
                new ReportFrameData()
                    {
                        Header1 = doctor.CFG_Documents.Header1,
                        Header2 = doctor.CFG_Documents.Header2,
                        FooterLeft1 = doctor.CFG_Documents.FooterLeft1,
                        FooterLeft2 = doctor.CFG_Documents.FooterLeft2,
                        FooterRight1 = doctor.CFG_Documents.FooterRight1,
                        FooterRight2 = doctor.CFG_Documents.FooterRight2
                    });

            var report = new Report(watermark);
            foreach (var patient in reportDataSource.Patients)
            {
                using (var patientContext = report.AddDataContext(patient))
                {
                    patientContext.AddTitle(ReportTitleSize.H1, m => "Patient: " + PersonHelper.GetFullName(m.FirstName, m.MiddleName, m.LastName));

                    var patientCard = patientContext.AddCard();
                    patientCard.AddField(m => m.FirstName);
                    patientCard.AddField(m => m.MiddleName);
                    patientCard.AddField(m => m.LastName);
                    patientCard.AddField(m => m.Gender);
                    patientCard.AddField(m => m.DateOfBirth);
                    patientCard.AddField(m => m.SSN);
                    patientCard.AddField(m => m.PhoneHome);
                    patientCard.AddField(m => m.PhoneMobile);
                    patientCard.AddField(m => m.Email, true);

                    patientContext.AddTitle(ReportTitleSize.H2, "Address");

                    var patientAddressCard = patientContext.AddCard(m => m.Address);
                    patientAddressCard.AddField(m => m.AddressLine1, true);
                    patientAddressCard.AddField(m => m.AddressLine2, true);
                    patientAddressCard.AddField(m => m.City);
                    patientAddressCard.AddField(m => m.County);
                    patientAddressCard.AddField(m => m.StateProvince);
                    patientAddressCard.AddField(m => m.ZipCode);

                    patientContext.AddTitle(ReportTitleSize.H2, "History");

                    foreach (var patientSession in patient.Sessions)
                    {
                        patientContext.AddTitle(ReportTitleSize.H2, "Consultation - " + patientSession.Date);

                        // Anamneses
                        if (patientSession.PastMedicalHistories.Any())
                        {
                            patientContext.AddTitle(ReportTitleSize.H3, "Anamnese");
                            foreach (var pastMedicalHistory in patientSession.PastMedicalHistories)
                                using (var dataContext = report.AddDataContext(pastMedicalHistory))
                                {
                                    var anamneseCard = dataContext.AddCard();
                                    anamneseCard.AddField(m => m.MajorEvents, true);
                                    anamneseCard.AddField(m => m.Allergies, true);
                                    anamneseCard.AddField(m => m.OngoinMedicationProblems, true);
                                    anamneseCard.AddField(m => m.FamilyMedicalHistory, true);
                                    anamneseCard.AddField(m => m.PreventiveCare, true);
                                    anamneseCard.AddField(m => m.SocialHistory, true);
                                    anamneseCard.AddField(m => m.NutritionHistory, true);
                                    anamneseCard.AddField(m => m.DevelopmentHistory, true);
                                }
                        }

                        // physical examinations
                        if (patientSession.PhysicalExaminations.Any())
                        {
                            patientContext.AddTitle(ReportTitleSize.H3, "Physical examination");
                            foreach (var physicalExamination in patientSession.PhysicalExaminations)
                                using (var dataContext = report.AddDataContext(physicalExamination))
                                {
                                    var card = dataContext.AddCard();
                                    card.AddField(m => m.Notes, true);
                                }
                        }

                        // diagostic hypotheses
                        if (patientSession.DiagnosticHypotheses.Any())
                        {
                            patientContext.AddTitle(ReportTitleSize.H3, "Diagnostic hypothesis");
                            foreach (var physicalExamination in patientSession.PhysicalExaminations)
                                using (var dataContext = report.AddDataContext(physicalExamination))
                                {
                                    var card = dataContext.AddCard();
                                    card.AddField(m => m.Notes, true);
                                }
                        }

                        // diagostic hypotheses
                        if (patientSession.Prescriptions.Any())
                        {
                            patientContext.AddTitle(ReportTitleSize.H3, "Prescription");
                            foreach (var prescription in patientSession.Prescriptions)
                                using (var dataContext = report.AddDataContext(prescription))
                                {
                                    var grid = dataContext.AddGrid(m => m.ReceiptMedicines);
                                    grid.AddColumn(m => m.MedicineText);
                                    grid.AddColumn(m => m.Quantity);
                                    grid.AddColumn(m => m.Prescription);
                                    grid.AddColumn(m => m.Observations);
                                }
                        }

                        // diagostic hypotheses
                        if (patientSession.DiagnosticHypotheses.Any())
                        {
                            patientContext.AddTitle(ReportTitleSize.H3, "Examination or procedure requests");
                            foreach (var examRequest in patientSession.ExaminationRequests)
                                using (var dataContext = report.AddDataContext(examRequest))
                                {
                                    var card = dataContext.AddCard();
                                    card.AddField(m => m.MedicalProcedureCode, true);
                                    card.AddField(m => m.MedicalProcedureName, true);
                                    card.AddField(m => m.Notes, true);
                                }
                        }
                    }
                }
            }
            return report.Stream;
        }

        [SelfPermission]
        public FileStreamResult ExportPatientsPdf(int? patientId)
        {
            var pdf = ExportPatientsPdf(patientId, this.db, this.DbPractice, this.Doctor);

            // Returning the generated PDF as a file.
            return this.File(pdf, MimeTypesHelper.GetContentType(".pdf"));
        }
    }


    public class ReportData
    {
        private readonly CerebelloEntitiesAccessFilterWrapper db;
        private readonly Practice practice;

        public ReportData(CerebelloEntitiesAccessFilterWrapper db, Practice practice)
        {
            this.db = db;
            this.practice = practice;
        }

        public XmlDoctorData GetReportDataSourceForXml(Doctor doctor, int? patientId)
        {
            return this.GetReportDataSource(doctor, patientId, false);
        }

        public PdfDoctorData GetReportDataSourceForPdf(Doctor doctor, int? patientId)
        {
            return (PdfDoctorData)this.GetReportDataSource(doctor, patientId, true);
        }

        private XmlDoctorData GetReportDataSource(Doctor doctor, int? patientId, bool isPdf)
        {
            var doctorDbUser = doctor.Users.Single();
            var doctorDbPerson = doctorDbUser.Person;

            var medicalEntity = UsersController.GetDoctorEntity(this.db.SYS_MedicalEntity, doctor);
            var medicalSpecialty = UsersController.GetDoctorSpecialty(this.db.SYS_MedicalSpecialty, doctor);

            // Copying properties from the model to the view model.
            var doctorData = isPdf ? new PdfDoctorData() : new XmlDoctorData();
            Mapper.Map(doctorDbPerson, doctorData);
            Mapper.Map(doctorDbUser, doctorData);
            if (doctorDbPerson.Address != null)
                Mapper.Map(doctorDbPerson.Address, doctorData.Address);

            UsersController.FillDoctorViewModel(doctorDbUser, medicalEntity, medicalSpecialty, doctorData, doctor);

            if (patientId != null)
            {
                doctorData.Patients = doctor.Patients
                                            .Where(p => p.Id == patientId)
                                            .Select(p => this.GetPatientData(p, isPdf))
                                            .OrderBy(x => x.LastName)
                                            .ThenBy(x => x.FirstName)
                                            .ToList();
            }
            else
            {
                doctorData.Patients = doctor.Patients
                                            .Select(p => this.GetPatientData(p, isPdf))
                                            .OrderBy(x => x.LastName)
                                            .ThenBy(x => x.FirstName)
                                            .ToList();
            }

            return doctorData;
        }

        private XmlPatientData GetPatientData(Patient patient, bool isPdf)
        {
            var result = isPdf ? new PdfPatientData() : new XmlPatientData();

            Mapper.Map(patient.Person, result);
            result.DateOfBirth = ModelDateTimeHelper.ConvertToLocalDateTime(practice, patient.Person.DateOfBirth);

            Mapper.Map(patient.Person.Address, result.Address);

            result.Sessions = this.GetAllSessionsData(patient);
            return result;
        }

        private List<SessionData> GetAllSessionsData(Patient patient)
        {
            var sessions = PatientsController.GetSessionViewModels(this.practice, patient, null)
                                             .Select(this.GetSessionData)
                                             .ToList();

            return sessions;
        }

        private SessionData GetSessionData(SessionViewModel arg)
        {
            var receiptGetter = this.ViewModelGetter<Receipt, ReceiptViewModel>(ReceiptsController.GetViewModel);
            var physicalExaminationGetter = this.ViewModelGetter<PhysicalExamination, PhysicalExaminationViewModel>(PhysicalExaminationController.GetViewModel);
            var diagnosticHypothesisGetter = this.ViewModelGetter<DiagnosticHypothesis, DiagnosticHypothesisViewModel>(DiagnosticHypothesesController.GetViewModel);
            var examsGetter = this.ViewModelGetter<ExaminationRequest, ExaminationRequestViewModel>(ExamsController.GetViewModel);
            var examResultsGetter = this.ViewModelGetter<ExaminationResult, ExaminationResultViewModel>(ExamResultsController.GetViewModel);
            var medicalCertificateGetter = this.ViewModelGetter<MedicalCertificate, MedicalCertificateViewModel>(MedicalCertificatesController.GetViewModel);

            var result = new SessionData
                {
                    Date = arg.Date,
                    PastMedicalHistories = this.db.PastMedicalHistories.Where(x => arg.AnamneseIds.Contains(x.Id)).Select(Mapper.Map<PastMedicalHistoryViewModel>).ToList(),
                    Prescriptions = this.db.Receipts.Where(x => arg.ReceiptIds.Contains(x.Id))
                        .Select(receiptGetter).ToList(),
                    PhysicalExaminations = this.db.PhysicalExaminations.Where(x => arg.PhysicalExaminationIds.Contains(x.Id))
                        .Select(physicalExaminationGetter).ToList(),
                    DiagnosticHypotheses = this.db.DiagnosticHypotheses.Where(x => arg.DiagnosticHipothesesId.Contains(x.Id))
                        .Select(diagnosticHypothesisGetter).ToList(),
                    ExaminationRequests = this.db.ExaminationRequests.Where(x => arg.ExaminationRequestIds.Contains(x.Id))
                        .Select(examsGetter).ToList(),
                    ExaminationResults = this.db.ExaminationResults.Where(x => arg.ExaminationResultIds.Contains(x.Id))
                        .Select(examResultsGetter).ToList(),
                    Diagnosis = this.db.Diagnoses.Where(x => arg.DiagnosisIds.Contains(x.Id)).Select(Mapper.Map<DiagnosisViewModel>).ToList(),
                    MedicalCertificates = this.db.MedicalCertificates.Where(x => arg.MedicalCertificateIds.Contains(x.Id))
                        .Select(medicalCertificateGetter).ToList()
                };

            return result;
        }

        private Func<TModel, TViewModel> ViewModelGetter<TModel, TViewModel>(Func<TModel, Func<DateTime, DateTime>, TViewModel> fnc)
        {
            return m => fnc(m, this.ToLocalConverter());
        }

        private readonly object locker = new object();
        private Func<DateTime, DateTime> toLocalCache;
        private Func<DateTime, DateTime> ToLocalConverter()
        {
            if (this.toLocalCache == null)
                lock (this.locker)
                    if (this.toLocalCache == null)
                    {
                        var practiceId = this.practice.Id;
                        var timeZoneId = this.db.Practices.Where(p => p.Id == practiceId).Select(p => p.WindowsTimeZoneId).Single();
                        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                        Func<DateTime, DateTime> toLocal = utcDateTime => TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);

                        Thread.MemoryBarrier();

                        this.toLocalCache = toLocal;
                    }

            return this.toLocalCache;
        }

        public class PdfDoctorData : XmlDoctorData
        {
            public new string Gender
            {
                get { return EnumHelper.GetValueDisplayDictionary(typeof(TypeGender))[(int)base.Gender]; }
            }

            public new string MedicalEntityJurisdiction
            {
                get
                {
                    return base.MedicalEntityJurisdiction.HasValue ? EnumHelper.GetValueDisplayDictionary(typeof(TypeEstadoBrasileiro))[(int)base.MedicalEntityJurisdiction.Value] : null;
                }
            }

            public new List<PdfPatientData> Patients
            {
                get { return base.Patients.OfType<PdfPatientData>().ToList(); }
            }
        }

        public class PdfPatientData : XmlPatientData
        {
            public new string Gender
            {
                get { return EnumHelper.GetValueDisplayDictionary(typeof(TypeGender))[(int)base.Gender]; }
            }
        }


        [XmlRoot("Doctor", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
        [XmlType("Doctor")]
        public class XmlDoctorData : UserViewModel
        {
            public new TypeGender Gender
            {
                get { return (TypeGender)base.Gender; }
                set { base.Gender = (int)value; }
            }

            public new TypeEstadoBrasileiro? MedicalEntityJurisdiction
            {
                get { return (TypeEstadoBrasileiro?)base.MedicalEntityJurisdiction; }
                set { base.MedicalEntityJurisdiction = (int?)value; }
            }

            public List<XmlPatientData> Patients { get; set; }
        }

        [XmlRoot("Patient", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
        [XmlType("Patient")]
        public class XmlPatientData : PatientViewModel
        {
            public new TypeGender Gender
            {
                get { return (TypeGender)base.Gender; }
                set { base.Gender = (int)value; }
            }

            public List<AppointmentViewModel> Appointments { get; set; }

            [XmlElement]
            public List<SessionData> Sessions { get; set; }
        }


        [XmlRoot("Session", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
        [XmlType("Session")]
        public class SessionData
        {
            public DateTime Date { get; set; }

            public List<PastMedicalHistoryViewModel> PastMedicalHistories { get; set; }
            public List<PhysicalExaminationViewModel> PhysicalExaminations { get; set; }
            public List<ReceiptViewModel> Prescriptions { get; set; }
            public List<ExaminationRequestViewModel> ExaminationRequests { get; set; }
            public List<ExaminationResultViewModel> ExaminationResults { get; set; }
            public List<DiagnosisViewModel> Diagnosis { get; set; }
            public List<MedicalCertificateViewModel> MedicalCertificates { get; set; }
            public List<DiagnosticHypothesisViewModel> DiagnosticHypotheses { get; set; }
        }
    }
}
