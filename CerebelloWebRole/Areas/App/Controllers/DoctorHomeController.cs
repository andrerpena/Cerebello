using System;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;

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
            Func<Appointment, bool> getIsInThePast = a => ConvertToLocalDateTime(this.Practice, a.Start) < localNow;

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
                                LocalDateTime = ConvertToLocalDateTime(this.Practice, a.Start),
                                LocalDateTimeSpelled = DateTimeHelper.GetFormattedTime(ConvertToLocalDateTime(this.Practice, a.Start)) + " - " + DateTimeHelper.GetFormattedTime(ConvertToLocalDateTime(this.Practice, a.End)),
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
                                LocalDateTime = ConvertToLocalDateTime(this.Practice, a.Start),
                                LocalDateTimeSpelled = DateTimeHelper.GetFormattedTime(ConvertToLocalDateTime(this.Practice, a.Start)) + " - " + DateTimeHelper.GetFormattedTime(ConvertToLocalDateTime(this.Practice, a.End)),
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

    }
}
