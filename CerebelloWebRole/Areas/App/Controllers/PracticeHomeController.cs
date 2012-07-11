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
        //
        // GET: /App/Home/

        public ActionResult Index()
        {
            var model = new PracticeHomeIndexViewModel();
            model.Doctors = DoctorsController.GetDoctorViewModelsFromPractice(this.db, this.Practice);

            var currentPracticeId = this.GetCurrentUser().PracticeId;

            model.PatientsCount = this.db.Patients
                .Where(p => p.Doctor.Users.FirstOrDefault().PracticeId == currentPracticeId)
                .Count();

            return View(model);
        }
    }
}
