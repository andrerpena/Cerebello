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
    public class DoctorHomeController : DoctorController
    {
        //
        // GET: /App/DoctorHome/

        public ActionResult Index()
        {
            var utcNow = this.GetUtcNow();
            var localNow = this.GetPracticeLocalNow();

            // find today's appointments
            var todayStart = utcNow.Date;
            var todayEnd = todayStart.AddDays(1);

            // returns whether the appointment is in the past
            Func<Appointment, bool> getIsInThePast = a => ConvertToLocalDateTime(this.DbPractice, a.Start) < localNow;

            Func<Appointment, bool> getIsNow = a => a.Start <= utcNow && a.End > utcNow;

            // returns whether the patient has arrived
            Func<Appointment, bool> getPatientArrived = a => !getIsInThePast(a) && a.Status == (int)TypeAppointmentStatus.Accomplished;

            // returns the status text
            Func<Appointment, string> getStatusText = a =>
                {
                    if (getPatientArrived(a))
                        return "Paciente chegou";
                    return EnumHelper.GetText(a.Status, typeof(TypeAppointmentStatus)) ??
                           EnumHelper.GetText(TypeAppointmentStatus.Undefined);
                };

            var todaysAppointments =
                this.db.Appointments
                    .Where(
                        a =>
                        a.DoctorId == this.Doctor.Id && a.Start >= todayStart && a.Start < todayEnd &&
                        a.Type == (int)TypeAppointment.MedicalAppointment)
                    .AsEnumerable()
                    .Select(
                        a => new AppointmentViewModel()
                            {
                                Id = a.Id,
                                Description = a.Description,
                                PatientId = a.PatientId,
                                PatientName = a.PatientId != default(int) ? a.Patient.Person.FullName : null,
                                LocalDateTime = ConvertToLocalDateTime(this.DbPractice, a.Start),
                                LocalDateTimeSpelled = DateTimeHelper.GetFormattedTime(ConvertToLocalDateTime(this.DbPractice, a.Start)) + " - " + DateTimeHelper.GetFormattedTime(ConvertToLocalDateTime(this.DbPractice, a.End)),
                                HealthInsuranceId = a.HealthInsuranceId,
                                HealthInsuranceName = a.HealthInsurance.Name,
                                IsInThePast = getIsInThePast(a),
                                IsNow = getIsNow(a),
                                PatientArrived = getPatientArrived(a),
                                Status = a.Status,
                                StatusText = getStatusText(a)
                            }).ToList();

            var nextGenericAppointments =
                this.db.Appointments.Where(a => a.DoctorId == this.Doctor.Id && a.Type == (int)TypeAppointment.GenericAppointment && ((a.Start < utcNow && a.End > utcNow) || a.Start > utcNow)).OrderBy(a => a.Start).Take(5)
                    .AsEnumerable()
                    .Select(
                        a => new AppointmentViewModel()
                            {
                                Id = a.Id,
                                Description = a.Description,
                                LocalDateTime = ConvertToLocalDateTime(this.DbPractice, a.Start),
                                LocalDateTimeSpelled = DateTimeHelper.GetFormattedTime(ConvertToLocalDateTime(this.DbPractice, a.Start)) + " - " + DateTimeHelper.GetFormattedTime(ConvertToLocalDateTime(this.DbPractice, a.End)),
                                IsNow = getIsNow(a)
                            }).ToList();

            var medicalEntity = UsersController.GetDoctorEntity(this.db.SYS_MedicalEntity, this.Doctor);
            var medicalSpecialty = UsersController.GetDoctorSpecialty(this.db.SYS_MedicalSpecialty, this.Doctor);

            var viewModel = new DoctorHomeViewModel()
                {
                    DoctorName = this.Doctor.Users.First().Person.FullName,
                    Gender = (TypeGender)this.Doctor.Users.First().Person.Gender,
                    NextFreeTime = ScheduleController.FindNextFreeTimeInPracticeLocalTime(this.db, this.Doctor, localNow),
                    TodaysAppointments = todaysAppointments,
                    NextGenericAppointments = nextGenericAppointments,
                    MedicCrm = this.Doctor.CRM,
                    MedicalSpecialtyId = medicalSpecialty != null ? medicalSpecialty.Id : (int?)null,
                    MedicalSpecialtyName = medicalSpecialty != null ? medicalSpecialty.Name : null,
                    MedicalEntityId = medicalEntity != null ? medicalEntity.Id : (int?)null,
                    MedicalEntityName = medicalEntity != null ? medicalEntity.Name : null,
                    MedicalEntityJurisdiction = (int)(TypeEstadoBrasileiro)Enum.Parse(
                    typeof(TypeEstadoBrasileiro),
                    this.Doctor.MedicalEntityJurisdiction)
                };

            this.ViewBag.PracticeLocalDate = localNow.ToShortDateString();

            return View(viewModel);
        }






        [SelfPermission]
        public ActionResult XmlBackup()
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
        public ActionResult PdfBackup(int? patientId)
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
            var urlMain = new Uri(string.Format("{0}/Content/Reports/PatientsList/Doctor.trdx", domain));

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

        private static Telerik.Reporting.Report CreateReportFromUrl(Uri uri)
        {
            var settings = new XmlReaderSettings { IgnoreWhitespace = true };
            using (var xmlReader = XmlReader.Create(uri.ToString(), settings))
            {
                var xmlSerializer = new ReportXmlSerializer();
                var report = (Report)xmlSerializer.Deserialize(xmlReader);


                var subReports = report.Items.Find(typeof(Telerik.Reporting.SubReport), true).OfType<SubReport>();
                foreach (var eachSubReport in subReports)
                {
                    var uriSub = new Uri(uri, string.Format("{0}.trdx", eachSubReport.Name));
                    var reportSub = CreateReportFromUrl(uriSub);
                    report.Disposed += (s, e) => reportSub.Dispose();
                    eachSubReport.ReportSource = reportSub;
                }

                return report;
            }
        }

        private bool isPdf;

        private XmlDoctorData GetBackupData(Doctor doctor, int? patientId, bool isPdf)
        {
            this.isPdf = isPdf;

            var docPerson = doctor.Users.Single().Person;

            // Getting all patients data.
            var doctorData = this.isPdf ? new PdfDoctorData() : new XmlDoctorData();
            PatientsController.FillPersonViewModel(this, docPerson, doctorData);

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
