using System;
using System.Linq;
using System.Web.Mvc;
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
                    PatientId = a.PatientId,
                    PatientName = a.Patient.Person.FullName,
                    Date = ConvertToLocalDateTime(this.Practice, a.Start),
                    DateSpelled = getRelativeDate(a.Start)
                    
                }).ToList();

            var viewModel = new DoctorHomeViewModel()
                {
                    DoctorName = this.Doctor.Users.First().Person.FullName,
                    NextAppointments = nextAppoitments
                };

            return View(viewModel);
        }

    }
}
