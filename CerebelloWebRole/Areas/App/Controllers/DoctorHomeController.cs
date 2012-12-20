using System;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Models;

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

            // try to find an appointment that should be happening now
            var appointmentsForNow =
                this.db.Appointments.Where(a => a.DoctorId == this.Doctor.Id && a.Start <= utcNow && a.End > utcNow && a.Type == (int)TypeAppointment.MedicalAppointment).ToList();

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
                this.db.Appointments.Where(a => a.DoctorId == this.Doctor.Id && a.Start >= todayStart && a.Start < todayEnd && a.Type == (int)TypeAppointment.MedicalAppointment)
                .AsEnumerable().Select(a => new AppointmentViewModel()
                {
                    Description = a.Description,
                    PatientId = a.PatientId,
                    PatientName = a.PatientId != default(int) ? a.Patient.Person.FullName : null,
                    LocalDateTime = ConvertToLocalDateTime(this.Practice, a.Start),
                    LocalDateTimeSpelled = DateTimeHelper.GetFormattedTime(ConvertToLocalDateTime(this.Practice, a.Start)),
                    HealthInsuranceId = a.HealthInsuranceId,
                    HealthInsuranceName = a.HealthInsurance.Name,
                    IsInThePast = getIsInThePast(a),
                    IsNow = getIsNow(a),
                    PatientArrived = getPatientArrived(a),
                    Status = a.Status,
                    StatusText = getStatusText(a)
                }).ToList();

            var person = this.Doctor.Users.First().Person;
            var viewModel = new DoctorHomeViewModel()
                {
                    DoctorName = person.FullName,
                    NextFreeTime = ScheduleController.FindNextFreeTimeInPracticeLocalTime(this.db, this.Doctor, localNow),
                    TodaysAppointments = todaysAppointments,
                    Gender = (TypeGender)person.Gender,
                };

            this.ViewBag.PracticeLocalDate = localNow.ToShortDateString();

            return View(viewModel);
        }

    }
}
