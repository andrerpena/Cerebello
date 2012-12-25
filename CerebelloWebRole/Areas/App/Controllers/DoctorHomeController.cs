using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml.Serialization;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Filters;

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

            var docPerson = doctor.Users.Single().Person;

            // Getting all patients data.
            var doctorData = new DoctorData();
            PatientsController.FillPersonViewModel(this, docPerson, doctorData);

            doctorData.Address = PatientsController.GetAddressViewModel(docPerson.Addresses.SingleOrDefault());

            doctorData.Patients = this.Doctor.Patients
                                      .Select(this.GetPatientData)
                                      .ToList();

            var stringBuilder = new StringBuilder();
            using (var writer = new StringWriter(stringBuilder))
            {
                var xs = new XmlSerializer(typeof(DoctorData));
                xs.Serialize(writer, doctorData);
            }

            return this.Content(stringBuilder.ToString(), "text/xml");
        }

        private PatientData GetPatientData(Patient patient)
        {
            var result = new PatientData();

            result.Id = patient.Id;

            PatientsController.FillPersonViewModel(this, patient.Person, result);

            result.Address = PatientsController.GetAddressViewModel(patient.Person.Addresses.Single());

            result.Sessions = GetAllSessionsData(this, patient);

            return result;
        }

        private List<SessionData> GetAllSessionsData(DoctorHomeController doctorHomeController, Patient patient)
        {
            var sessions = PatientsController.GetSessionViewModels(this, patient)
                                             .Select(GetSessionData)
                                             .ToList();

            return sessions;
        }

        private SessionData GetSessionData(SessionViewModel arg)
        {
            var result = new SessionData();

            result.Date = arg.Date;

            result.Items = new List<object>();

            result.Items.AddRange(
                this.db.Anamnese.Where(x => arg.AnamneseIds.Contains(x.Id))
                .Select(AnamnesesController.GetViewModel));

            result.Items.AddRange(
                this.db.Receipts.Where(x => arg.ReceiptIds.Contains(x.Id))
                .Select(ReceiptsController.GetViewModel));

            result.Items.AddRange(
                this.db.ExaminationRequests.Where(x => arg.ExaminationRequestIds.Contains(x.Id))
                .Select(ExamsController.GetViewModel));

            result.Items.AddRange(
                this.db.ExaminationResults.Where(x => arg.ExaminationResultIds.Contains(x.Id))
                .Select(ExamResultsController.GetViewModel));

            result.Items.AddRange(
                this.db.Diagnoses.Where(x => arg.DiagnosisIds.Contains(x.Id))
                .Select(DiagnosisController.GetViewModel));

            result.Items.AddRange(
                this.db.MedicalCertificates.Where(x => arg.MedicalCertificateIds.Contains(x.Id))
                .Select(MedicalCertificatesController.GetViewModel));

            return result;
        }


        [XmlRoot("Doctor", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
        [XmlType("Doctor")]
        public class DoctorData : UserViewModel
        {
            public List<PatientData> Patients { get; set; }
        }

        [XmlRoot("Patient", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
        [XmlType("Patient")]
        public class PatientData : UserViewModel
        {
            public List<AppointmentViewModel> Appointments { get; set; }

            [XmlElementAttribute]
            public List<SessionData> Sessions { get; set; }
        }

        [XmlRoot("Session", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
        [XmlType("Session")]
        public class SessionData
        {
            public DateTime Date { get; set; }

            [XmlArrayItem(typeof(AnamneseViewModel))]
            [XmlArrayItem(typeof(ReceiptViewModel))]
            [XmlArrayItem(typeof(ExaminationRequestViewModel))]
            [XmlArrayItem(typeof(ExaminationResultViewModel))]
            [XmlArrayItem(typeof(DiagnosisViewModel))]
            [XmlArrayItem(typeof(MedicalCertificateViewModel))]
            public List<object> Items { get; set; }
        }
    }
}
