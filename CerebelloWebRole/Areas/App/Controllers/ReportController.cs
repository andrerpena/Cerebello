using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Helpers;
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

            var doctorData = rg.GetXml(doctor, null);

            var stringBuilder = new StringBuilder();
            using (var writer = new StringWriter(stringBuilder))
            {
                var xs = new XmlSerializer(typeof(ReportData.XmlDoctorData));
                xs.Serialize(writer, doctorData);
            }

            return stringBuilder.ToString();
        }

        [SelfPermission]
        public FileContentResult ExportPatientsPdf(int? patientId)
        {
            var pdf = ExportPatientsPdf(patientId, this.db, this.DbPractice, this.Doctor, this.Request);

            // Returning the generated PDF as a file.
            return this.File(pdf.DocumentBytes, pdf.MimeType);

            //var fileName = pdf.DocumentName + "." + pdf.Extension;
            //return this.File(result.DocumentBytes, result.MimeType, Server.UrlEncode(fileName));
        }

        internal static RenderingResult ExportPatientsPdf(int? patientId, CerebelloEntitiesAccessFilterWrapper db, Practice practice, Doctor doctor, HttpRequestBase request)
        {
            // todo: must get doctor with everything at once... and use all data
            //// Getting doctors with everything they have.
            //var doctor = this.db.Doctors
            //    .Include("Users")
            //    .Include("Users.Person")
            //    .Include("Users.Person.Addresses")
            //    .Include("Patients")
            //    .Include("Patients.Person")
            //    .Include("Patients.Person.Addresses")
            //    .Include("Patients.Anamneses")
            //    .Include("Patients.Receipts")
            //    .Include("Patients.MedicalCertificates")
            //    .Include("Patients.ExaminationRequests")
            //    .Include("Patients.ExaminationResults")
            //    .Include("Patients.Diagnoses")
            //    .SingleOrDefault(x => x.Id == doctorId);

            var rg = new ReportData(db, practice);
            var doctorData = rg.GetPdf(doctor, patientId);

            Report reportMain;

            if (DebugConfig.IsDebug && DebugConfig.UseLocalResourcesOnly)
            {
                // Getting file name of the report model.
                var resourcePath = String.Format("C:\\Cerebello\\CerebelloWebRole\\Content\\Reports\\PatientsList\\Doctor.trdx");

                // Creating the report and exporting PDF.
                reportMain = CreateReportFromFile(resourcePath);
            }
            else if (false)
            {
                // Getting URL of the report model.
                var domain = request.Url.GetLeftPart(UriPartial.Authority);
                var urlMain = new Uri(String.Format("{0}/Content/Reports/PatientsList/Doctor.trdx", domain));

                // Creating the report and exporting PDF.
                reportMain = CreateReportFromUrl(urlMain);
            }
            else
            {
                // Getting resource name of the report model.
                var resourcePath = String.Format("Content\\Reports\\PatientsList\\Doctor.trdx");

                // Creating the report and exporting PDF.
                reportMain = CreateReportFromResource(resourcePath);
            }

            // Creating the report and exporting PDF.
            using (reportMain)
            {
                reportMain.DataSource = new[] { doctorData };

                // Exporting PDF from report.
                var reportProcessor = new ReportProcessor();
                var pdf = reportProcessor.RenderReport("PDF", reportMain, null);
                return pdf;
            }
        }

        private static Report CreateReportFromUrl(Uri uri)
        {
            var settings = new XmlReaderSettings { IgnoreWhitespace = true };
            Report report;
            using (var xmlReader = XmlReader.Create(uri.ToString(), settings))
            {
                var xmlSerializer = new ReportXmlSerializer();
                report = (Report)xmlSerializer.Deserialize(xmlReader);
            }

            var subReports = report.Items.Find(typeof(Telerik.Reporting.SubReport), true).OfType<SubReport>();
            foreach (var eachSubReport in subReports)
            {
                var uriSub = new Uri(uri, String.Format("{0}.trdx", eachSubReport.Name));
                var reportSub = CreateReportFromUrl(uriSub);
                report.Disposed += (s, e) => reportSub.Dispose();
                eachSubReport.ReportSource = reportSub;
            }

            return report;
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

            var subReports = report.Items.Find(typeof(Telerik.Reporting.SubReport), true).OfType<SubReport>();
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
            var asm = typeof(ReportController).Assembly;
            var asmName = asm.GetName().Name;

            var settings = new XmlReaderSettings { IgnoreWhitespace = true };
            Report report;
            using (var fileStream = System.IO.File.Open(fileName, FileMode.Open))
            using (var xmlReader = XmlReader.Create(fileStream, settings))
            {
                var xmlSerializer = new ReportXmlSerializer();
                report = (Report)xmlSerializer.Deserialize(xmlReader);
            }

            var subReports = report.Items.Find(typeof(Telerik.Reporting.SubReport), true).OfType<SubReport>();
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
        private bool isPdf;
        private readonly CerebelloEntitiesAccessFilterWrapper db;
        private readonly Practice practice;

        public ReportData(CerebelloEntitiesAccessFilterWrapper db, Practice practice)
        {
            this.db = db;
            this.practice = practice;
        }

        public XmlDoctorData GetXml(Doctor doctor, int? patientId)
        {
            return this.GetBackupData(doctor, patientId, false);
        }

        public PdfDoctorData GetPdf(Doctor doctor, int? patientId)
        {
            return (PdfDoctorData)this.GetBackupData(doctor, patientId, true);
        }

        private XmlDoctorData GetBackupData(Doctor doctor, int? patientId, bool isPdf)
        {
            this.isPdf = isPdf;

            var docUser = doctor.Users.Single();
            var docPerson = docUser.Person;

            var medicalEntity = UsersController.GetDoctorEntity(this.db.SYS_MedicalEntity, doctor);
            var medicalSpecialty = UsersController.GetDoctorSpecialty(this.db.SYS_MedicalSpecialty, doctor);

            // Getting all patients data.
            var doctorData = this.isPdf ? new PdfDoctorData() : new XmlDoctorData();
            PatientsController.FillPersonViewModel(this.practice, docPerson, doctorData);
            UsersController.FillUserViewModel(docUser, this.practice, doctorData);
            UsersController.FillDoctorViewModel(docUser, medicalEntity, medicalSpecialty, doctorData, doctor);

            doctorData.Address = PatientsController.GetAddressViewModel(docPerson.Addresses.SingleOrDefault());

            if (patientId != null)
            {
                doctorData.Patients = doctor.Patients
                                            .Where(p => p.Id == patientId)
                                            .Select(this.GetPatientData)
                                            .OrderBy(x => x.FullName)
                                            .ToList();
            }
            else
            {
                doctorData.Patients = doctor.Patients
                                            .Select(this.GetPatientData)
                                            .OrderBy(x => x.FullName)
                                            .ToList();
            }

            return doctorData;
        }

        private XmlPatientData GetPatientData(Patient patient)
        {
            var result = this.isPdf ? new PdfPatientData() : new XmlPatientData();

            PatientsController.FillPersonViewModel(this.practice, patient.Person, result);

            result.Id = patient.Id;
            result.Observations = patient.Person.Observations;

            result.Address = PatientsController.GetAddressViewModel(patient.Person.Addresses.Single());

            result.Sessions = GetAllSessionsData(patient);

            return result;
        }

        private List<SessionData> GetAllSessionsData(Patient patient)
        {
            var sessions = PatientsController.GetSessionViewModels(this.practice, patient)
                                             .Select(GetSessionData)
                                             .ToList();

            return sessions;
        }

        private SessionData GetSessionData(SessionViewModel arg)
        {
            var result = new SessionData
                {
                    Date = arg.Date,
                    Anamneses = this.db.Anamnese.Where(x => arg.AnamneseIds.Contains(x.Id))
                                    .Select(AnamnesesController.GetViewModel).ToList(),
                    Receipts = this.db.Receipts.Where(x => arg.ReceiptIds.Contains(x.Id))
                                   .Select(ReceiptsController.GetViewModel).ToList(),
                    PhysicalExaminations = this.db.PhysicalExaminations.Where(x => arg.PhysicalExaminationIds.Contains(x.Id))
                                               .Select(PhysicalExaminationController.GetViewModel).ToList(),
                    DiagnosticHipotheses = this.db.DiagnosticHypotheses.Where(x => arg.DiagnosticHipothesesId.Contains(x.Id))
                                                .Select(DiagnosticHypothesesController.GetViewModel).ToList(),
                    ExaminationRequests = this.db.ExaminationRequests.Where(x => arg.ExaminationRequestIds.Contains(x.Id))
                                              .Select(ExamsController.GetViewModel).ToList(),
                    ExaminationResults = this.db.ExaminationResults.Where(x => arg.ExaminationResultIds.Contains(x.Id))
                                             .Select(ExamResultsController.GetViewModel).ToList(),
                    Diagnosis = this.db.Diagnoses.Where(x => arg.DiagnosisIds.Contains(x.Id))
                                    .Select(DiagnosisController.GetViewModel).ToList(),
                    MedicalCertificates = this.db.MedicalCertificates.Where(x => arg.MedicalCertificateIds.Contains(x.Id))
                                              .Select(MedicalCertificatesController.GetViewModel).ToList()
                };

            return result;
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
