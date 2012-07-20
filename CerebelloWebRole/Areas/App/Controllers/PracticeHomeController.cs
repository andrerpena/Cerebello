using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code;
using CerebelloWebRole.Areas.App.Models;
using Cerebello.Model;
using CerebelloWebRole.Code.Controllers;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class PracticeHomeController : PracticeController
    {
        public PracticeHomeController()
        {
            this.UserNowGetter = () => DateTimeHelper.GetTimeZoneNow();
            this.UtcNowGetter = () => DateTime.UtcNow;
        }

        public Func<DateTime> UserNowGetter { get; set; }

        public Func<DateTime> UtcNowGetter { get; set; }

        //
        // GET: /App/Home/

        public ActionResult Index()
        {
            var model = new PracticeHomeIndexViewModel();
            model.Doctors = DoctorsController.GetDoctorViewModelsFromPractice(this.db, this.Practice, this.UserNowGetter());

            var currentPracticeId = this.GetCurrentUser().PracticeId;

            model.PatientsCount = this.db.Patients
                .Where(p => p.Doctor.Users.FirstOrDefault().PracticeId == currentPracticeId)
                .Count();

            return View(model);
        }
    }
}
