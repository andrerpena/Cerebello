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
using Telerik.Reporting.Processing;
using Telerik.Reporting.XmlSerialization;
using Report = Telerik.Reporting.Report;
using SubReport = Telerik.Reporting.SubReport;

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

        internal static RenderingResult ExportPatientsPdf(int? patientId, CerebelloEntitiesAccessFilterWrapper db, Practice practice, Doctor doctor)
        {
            var reportDataSource = new ReportData(db, practice).GetReportDataSourceForPdf(doctor, patientId);
            Report telerikReport;

            if (DebugConfig.IsDebug && DebugConfig.UseLocalResourcesOnly)
            {
                // Getting file name of the report model.
                var resourcePath = String.Format("C:\\Cerebello\\CerebelloWebRole\\Content\\Reports\\PatientsList\\Doctor.trdx");

                // Creating the report and exporting PDF.
                telerikReport = CreateReportFromFile(resourcePath);
            }
            else
            {
                // Getting resource name of the report model.
                var resourcePath = String.Format("Content\\Reports\\PatientsList\\Doctor.trdx");

                // Creating the report and exporting PDF.
                telerikReport = CreateReportFromResource(resourcePath);
            }

            // Creating the report and exporting PDF.
            using (telerikReport)
            {
                telerikReport.DataSource = new[] { reportDataSource };

                // Exporting PDF from report.
                var reportProcessor = new ReportProcessor();
                var pdf = reportProcessor.RenderReport("PDF", telerikReport, null);
                return pdf;
            }
        }

        [SelfPermission]
        public FileContentResult ExportPatientsPdf(int? patientId)
        {
            var pdf = ExportPatientsPdf(patientId, this.db, this.DbPractice, this.Doctor);

            // Returning the generated PDF as a file.
            return this.File(pdf.DocumentBytes, pdf.MimeType);
        }

        private static Report CreateReportFromResource(string resourceName)
        {
            var asm = typeof(ReportController).Assembly;
            var asmName = asm.GetName().Name;

            var resName = Path.Combine(asmName, resourceName)
                .Replace("\\", ".");

            var settings = new XmlReaderSettings { IgnoreWhitespace = true };
            Report report;
            using (var xmlReader = XmlReader.Create(asm.GetManifestResourceStream(resName), settings))
            {
                var xmlSerializer = new ReportXmlSerializer();
                report = (Report)xmlSerializer.Deserialize(xmlReader);
            }

            var subReports = report.Items.Find(typeof(SubReport), true).OfType<SubReport>();
            foreach (var eachSubReport in subReports)
            {
                var resourceNameSub = Path.Combine(Path.GetDirectoryName(resourceName), String.Format("{0}.trdx", eachSubReport.Name));
                var reportSub = CreateReportFromResource(resourceNameSub);
                report.Disposed += (s, e) => reportSub.Dispose();
                eachSubReport.ReportSource = reportSub;
            }

            return report;
        }

        private static Report CreateReportFromFile(string fileName)
        {
            var settings = new XmlReaderSettings { IgnoreWhitespace = true };
            Report report;
            using (var fileStream = System.IO.File.Open(fileName, FileMode.Open))
            using (var xmlReader = XmlReader.Create(fileStream, settings))
            {
                var xmlSerializer = new ReportXmlSerializer();
                report = (Report)xmlSerializer.Deserialize(xmlReader);
            }

            var subReports = report.Items.Find(typeof(SubReport), true).OfType<SubReport>();
            foreach (var eachSubReport in subReports)
            {
                var resourceNameSub = Path.Combine(Path.GetDirectoryName(fileName), String.Format("{0}.trdx", eachSubReport.Name));
                var reportSub = CreateReportFromFile(resourceNameSub);
                report.Disposed += (s, e) => reportSub.Dispose();
                eachSubReport.ReportSource = reportSub;
            }

            return report;
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
            var sessions = PatientsController.GetSessionViewModels(this.practice, patient)
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

            public new string MedicalEntityJurisdiction
            {
                get
                {
                    if (base.MedicalEntityJurisdiction.HasValue)
                        return EnumHelper.GetValueDisplayDictionary(typeof(TypeEstadoBrasileiro))[(int)base.MedicalEntityJurisdiction.Value];
                    return null;
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
