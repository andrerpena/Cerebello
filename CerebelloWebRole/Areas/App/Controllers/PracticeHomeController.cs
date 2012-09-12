using System;
using System.Linq;
using System.Web.Mvc;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class PracticeHomeController : PracticeController
    {
        public PracticeHomeController()
        {
        }

        //
        // GET: /App/Home/

        public ActionResult Index()
        {
            var model = new PracticeHomeIndexViewModel();
            model.Doctors = DoctorsController.GetDoctorViewModelsFromPractice(this.db, this.Practice, this.GetPracticeLocalNow());

            var currentPracticeId = this.DBUser.PracticeId;

            model.PatientsCount = this.db.Patients
                .Where(p => p.Doctor.Users.FirstOrDefault().PracticeId == currentPracticeId)
                .Count();

            return View(model);
        }

        public ActionResult Welcome()
        {
            this.Practice.ShowWelcomeScreen = false;

            this.db.SaveChanges();

            return View();
        }
    }
}
