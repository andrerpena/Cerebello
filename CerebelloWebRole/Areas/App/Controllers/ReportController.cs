using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using SmartRecords;
using iTextSharp.text;
using iTextSharp.text.pdf;

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

            var document = new Document(PageSize.A4, 36, 36, 80, 80);
            var art = new Rectangle(50, 50, 545, 792);
            var documentStream = new MemoryStream();
            var writer = PdfWriter.GetInstance(document, documentStream);
            writer.SetBoxSize("art", art);
            writer.PageEvent = new ReportWatermark(
                new ReportWatermarkData()
                    {
                        Header1 = doctor.CFG_Documents.Header1,
                        Header2 = doctor.CFG_Documents.Header2,
                        FooterLeft1 = doctor.CFG_Documents.FooterLeft1,
                        FooterLeft2 = doctor.CFG_Documents.FooterLeft2,
                        FooterRight1 = doctor.CFG_Documents.FooterRight1,
                        FooterRight2 = doctor.CFG_Documents.FooterRight2
                    });

            var report = new Report<ReportData.PdfPatientData>();
            foreach (var patient in reportDataSource.Patients)
            {
                report.AddTitle(ReportTitleSize.H1, m => "Patient: " + m.FullName);
                
                var patientCard = report.AddCard();
                patientCard.AddField(m => m.FullName, true);
                patientCard.AddField(m => m.Gender);
                patientCard.AddField(m => m.DateOfBirth);
                patientCard.AddField(m => m.Profession);
                patientCard.AddField(m => m.MaritalStatus);
                patientCard.AddField(m => m.Cpf);
                patientCard.AddField(m => m.CpfOwner);
                patientCard.AddField(m => m.PhoneLand);
                patientCard.AddField(m => m.PhoneCell);
                patientCard.AddField(m => m.Email, true);

                report.AddTitle(ReportTitleSize.H2, "Address");

                var patientAddressCard = report.AddCard(m => m.Address);
                patientAddressCard.AddField(m => m.Street, true);
                patientAddressCard.AddField(m => m.Complement);
                patientAddressCard.AddField(m => m.Neighborhood);
                patientAddressCard.AddField(m => m.StateProvince);
                patientAddressCard.AddField(m => m.City);
                patientAddressCard.AddField(m => m.CEP);
            }

            writer.CloseStream = false;

            document.Open();

            document.Close();
            documentStream.Position = 0;

            return documentStream;
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
            var docUser = doctor.Users.Single();
            var docPerson = docUser.Person;

            var medicalEntity = UsersController.GetDoctorEntity(this.db.SYS_MedicalEntity, doctor);
            var medicalSpecialty = UsersController.GetDoctorSpecialty(this.db.SYS_MedicalSpecialty, doctor);

            // Getting all patients data.
            var doctorData = isPdf ? new PdfDoctorData() : new XmlDoctorData();
            PatientsController.FillPersonViewModel(this.practice, docPerson, doctorData);
            UsersController.FillUserViewModel(docUser, this.practice, doctorData);
            UsersController.FillDoctorViewModel(docUser, medicalEntity, medicalSpecialty, doctorData, doctor);

            doctorData.Address = PatientsController.GetAddressViewModel(docPerson.Addresses.SingleOrDefault());

            if (patientId != null)
            {
                doctorData.Patients = doctor.Patients
                                            .Where(p => p.Id == patientId)
                                            .Select(p => this.GetPatientData(p, isPdf))
                                            .OrderBy(x => x.FullName)
                                            .ToList();
            }
            else
            {
                doctorData.Patients = doctor.Patients
                                            .Select(p => this.GetPatientData(p, isPdf))
                                            .OrderBy(x => x.FullName)
                                            .ToList();
            }

            return doctorData;
        }

        private XmlPatientData GetPatientData(Patient patient, bool isPdf)
        {
            var result = isPdf ? new PdfPatientData() : new XmlPatientData();

            PatientsController.FillPersonViewModel(this.practice, patient.Person, result);

            result.Id = patient.Id;
            result.Observations = patient.Person.Observations;

            result.Address = PatientsController.GetAddressViewModel(patient.Person.Addresses.FirstOrDefault())
                ?? new AddressViewModel();

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
            var anamneseGetter = this.ViewModelGetter<Anamnese, AnamneseViewModel>(AnamnesesController.GetViewModel);
            var receiptGetter = this.ViewModelGetter<Receipt, ReceiptViewModel>(ReceiptsController.GetViewModel);
            var physicalExaminationGetter = this.ViewModelGetter<PhysicalExamination, PhysicalExaminationViewModel>(PhysicalExaminationController.GetViewModel);
            var diagnosticHypothesisGetter = this.ViewModelGetter<DiagnosticHypothesis, DiagnosticHypothesisViewModel>(DiagnosticHypothesesController.GetViewModel);
            var examsGetter = this.ViewModelGetter<ExaminationRequest, ExaminationRequestViewModel>(ExamsController.GetViewModel);
            var examResultsGetter = this.ViewModelGetter<ExaminationResult, ExaminationResultViewModel>(ExamResultsController.GetViewModel);
            var diagnosisGetter = this.ViewModelGetter<Diagnosis, DiagnosisViewModel>(DiagnosisController.GetViewModel);
            var medicalCertificateGetter = this.ViewModelGetter<MedicalCertificate, MedicalCertificateViewModel>(MedicalCertificatesController.GetViewModel);

            var result = new SessionData
                {
                    Date = arg.Date,
                    Anamneses = this.db.Anamnese.Where(x => arg.AnamneseIds.Contains(x.Id))
                        .Select(anamneseGetter).ToList(),
                    Receipts = this.db.Receipts.Where(x => arg.ReceiptIds.Contains(x.Id))
                        .Select(receiptGetter).ToList(),
                    PhysicalExaminations = this.db.PhysicalExaminations.Where(x => arg.PhysicalExaminationIds.Contains(x.Id))
                        .Select(physicalExaminationGetter).ToList(),
                    DiagnosticHipotheses = this.db.DiagnosticHypotheses.Where(x => arg.DiagnosticHipothesesId.Contains(x.Id))
                        .Select(diagnosticHypothesisGetter).ToList(),
                    ExaminationRequests = this.db.ExaminationRequests.Where(x => arg.ExaminationRequestIds.Contains(x.Id))
                        .Select(examsGetter).ToList(),
                    ExaminationResults = this.db.ExaminationResults.Where(x => arg.ExaminationResultIds.Contains(x.Id))
                        .Select(examResultsGetter).ToList(),
                    Diagnosis = this.db.Diagnoses.Where(x => arg.DiagnosisIds.Contains(x.Id))
                        .Select(diagnosisGetter).ToList(),
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

            public new string MaritalStatus
            {
                get
                {
                    return base.MaritalStatus.HasValue ? EnumHelper.GetValueDisplayDictionary(typeof(TypeMaritalStatus))[(int)base.MaritalStatus.Value] : null;
                }
            }

            public new string CpfOwner
            {
                get
                {
                    return base.CpfOwner.HasValue ? EnumHelper.GetValueDisplayDictionary(typeof(TypeCpfOwner))[(int)base.CpfOwner.Value] : null;
                }
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

            public new string MaritalStatus
            {
                get
                {
                    if (base.MaritalStatus.HasValue)
                        return EnumHelper.GetValueDisplayDictionary(typeof(TypeMaritalStatus))[(int)base.MaritalStatus.Value];
                    return null;
                }
            }

            public new string CpfOwner
            {
                get
                {
                    if (base.CpfOwner.HasValue)
                        return EnumHelper.GetValueDisplayDictionary(typeof(TypeCpfOwner))[(int)base.CpfOwner.Value];
                    return null;
                }
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

            public new TypeMaritalStatus? MaritalStatus
            {
                get { return (TypeMaritalStatus?)base.MaritalStatus; }
                set { base.MaritalStatus = (short?)value; }
            }

            public new TypeCpfOwner? CpfOwner
            {
                get { return (TypeCpfOwner?)base.CpfOwner; }
                set { base.CpfOwner = (short?)value; }
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

            public new TypeMaritalStatus? MaritalStatus
            {
                get { return (TypeMaritalStatus?)base.MaritalStatus; }
                set { base.MaritalStatus = (short?)value; }
            }

            public new TypeCpfOwner? CpfOwner
            {
                get { return (TypeCpfOwner?)base.CpfOwner; }
                set { base.CpfOwner = (short?)value; }
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

            public List<AnamneseViewModel> Anamneses { get; set; }
            public List<PhysicalExaminationViewModel> PhysicalExaminations { get; set; }
            public List<ReceiptViewModel> Receipts { get; set; }
            public List<ExaminationRequestViewModel> ExaminationRequests { get; set; }
            public List<ExaminationResultViewModel> ExaminationResults { get; set; }
            public List<DiagnosisViewModel> Diagnosis { get; set; }
            public List<MedicalCertificateViewModel> MedicalCertificates { get; set; }
            public List<DiagnosticHypothesisViewModel> DiagnosticHipotheses { get; set; }
        }
    }
}
