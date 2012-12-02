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

            Func<DateTime, string> getRelativeDate = s =>
                {
                    var result = s.ToShortDateString();
                    result += ", " + DateTimeHelper.GetFormattedTime(s);
                    result += ", " +
                              DateTimeHelper.ConvertToRelative(s, localNow,
                                                               DateTimeHelper.RelativeDateOptions.IncludeSuffixes |
                                                               DateTimeHelper.RelativeDateOptions.IncludePrefixes |
                                                               DateTimeHelper.RelativeDateOptions.ReplaceToday |
                                                               DateTimeHelper.RelativeDateOptions.ReplaceYesterdayAndTomorrow);

                    return result;
                };

            // find next appointments
            var nextAppoitments =
                this.db.Appointments.Where(a => a.DoctorId == this.Doctor.Id && a.Start > utcNow)
                .Take(10).AsEnumerable().Select(a => new AppointmentViewModel()
                {
                    Description = a.Description,
                    PatientId = a.PatientId,
                    PatientName = a.PatientId != default(int) ? a.Patient.Person.FullName : null,
                    Date = ConvertToLocalDateTime(this.Practice, a.Start),
                    DateSpelled = getRelativeDate(a.Start)
                }).ToList();

            var person = this.Doctor.Users.First().Person;
            var viewModel = new DoctorHomeViewModel()
                {
                    DoctorName = person.FullName,
                    NextAppointments = nextAppoitments,
                    Gender = (TypeGender)person.Gender,
                };

            return View(viewModel);
        }

    }
}
