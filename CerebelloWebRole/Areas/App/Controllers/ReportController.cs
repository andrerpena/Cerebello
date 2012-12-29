using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Filters;
using Telerik.Reporting.Processing;
using Telerik.Reporting.XmlSerialization;
using Report = Telerik.Reporting.Report;
using SubReport = Telerik.Reporting.SubReport;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class ReportController : DoctorController
    {
        private bool isPdf;

        [SelfPermission]
        public ActionResult ExportDoctorXml()
        {
            var doctor = this.Doctor;

            var doctorData = this.GetBackupData(doctor, null, false);

            var stringBuilder = new StringBuilder();
            using (var writer = new StringWriter(stringBuilder))
            {
                var xs = new XmlSerializer(typeof(XmlDoctorData));
                xs.Serialize(writer, doctorData);
            }

            return this.Content(stringBuilder.ToString(), "text/xml");
        }

        [SelfPermission]
        public ActionResult ExportPatientsPdf(int? patientId)
        {
            var doctor = this.Doctor;

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

            var doctorData = (PdfDoctorData)this.GetBackupData(doctor, patientId, true);

            // Getting URL of the report model.
            var domain = this.Request.Url.GetLeftPart(UriPartial.Authority);
            var urlMain = new Uri(String.Format("{0}/Content/Reports/PatientsList/Doctor.trdx", domain));

            // Creating the report and exporting PDF.
            using (var reportMain = CreateReportFromUrl(urlMain))
            {
                reportMain.DataSource = new[] { doctorData };

                // Exporting PDF from report.
                var reportProcessor = new ReportProcessor();
                var pdf = reportProcessor.RenderReport("PDF", reportMain, null);

                // Returning the generated PDF as a file.
                return this.File(pdf.DocumentBytes, pdf.MimeType);

                //var fileName = pdf.DocumentName + "." + pdf.Extension;
                //return this.File(result.DocumentBytes, result.MimeType, Server.UrlEncode(fileName));
            }
        }

        private static Report CreateReportFromUrl(Uri uri)
        {
            var settings = new XmlReaderSettings { IgnoreWhitespace = true };
            using (var xmlReader = XmlReader.Create(uri.ToString(), settings))
            {
                var xmlSerializer = new ReportXmlSerializer();
                var report = (Report)xmlSerializer.Deserialize(xmlReader);


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
            PatientsController.FillPersonViewModel(this, docPerson, doctorData);
            UsersController.FillUserViewModel(docUser, this.DbPractice, doctorData);
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

            PatientsController.FillPersonViewModel(this, patient.Person, result);

            result.Id = patient.Id;
            result.Observations = patient.Person.Observations;

            result.Address = PatientsController.GetAddressViewModel(patient.Person.Addresses.Single());

            result.Sessions = GetAllSessionsData(patient);

            return result;
        }

        private List<SessionData> GetAllSessionsData(Patient patient)
        {
            var sessions = PatientsController.GetSessionViewModels(this.DbPractice, patient)
                                             .Select(GetSessionData)
                                             .ToList();

            return sessions;
        }

        private SessionData GetSessionData(SessionViewModel arg)
        {
            var result = new SessionData();

            result.Date = arg.Date;

            result.Anamneses =
                this.db.Anamnese.Where(x => arg.AnamneseIds.Contains(x.Id))
                .Select(AnamnesesController.GetViewModel).ToList();

            result.Receipts =
                this.db.Receipts.Where(x => arg.ReceiptIds.Contains(x.Id))
                .Select(ReceiptsController.GetViewModel).ToList();

            result.ExaminationRequests =
                this.db.ExaminationRequests.Where(x => arg.ExaminationRequestIds.Contains(x.Id))
                .Select(ExamsController.GetViewModel).ToList();

            result.ExaminationResults =
                this.db.ExaminationResults.Where(x => arg.ExaminationResultIds.Contains(x.Id))
                .Select(ExamResultsController.GetViewModel).ToList();

            result.Diagnosis =
                this.db.Diagnoses.Where(x => arg.DiagnosisIds.Contains(x.Id))
                .Select(DiagnosisController.GetViewModel).ToList();

            result.MedicalCertificates =
                this.db.MedicalCertificates.Where(x => arg.MedicalCertificateIds.Contains(x.Id))
                .Select(MedicalCertificatesController.GetViewModel).ToList();

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
            public List<ReceiptViewModel> Receipts { get; set; }
            public List<ExaminationRequestViewModel> ExaminationRequests { get; set; }
            public List<ExaminationResultViewModel> ExaminationResults { get; set; }
            public List<DiagnosisViewModel> Diagnosis { get; set; }
            public List<MedicalCertificateViewModel> MedicalCertificates { get; set; }
        }
    }
}
