using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code;
using CerebelloWebRole.Areas.App.Models;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class PracticeHomeController : PracticeController
    {
        //
        // GET: /App/Home/

        public ActionResult Index()
        {
            var model = new PracticeHomeIndexViewModel();

            var usersThatAreDoctors = this.db.Users
                .Where(u => u.PracticeId == this.Practice.Id)
                .Where(u => u.Doctor != null);

            var dataCollection = usersThatAreDoctors
                .Select(u => new
                {
                    vm = new DoctorViewModel()
                    {
                        Id = u.Id,
                        Name = u.Person.FullName,
                        UrlIdentifier = u.Person.UrlIdentifier,
                        ImageUrl = GravatarHelper.GetGravatarUrl(u.GravatarEmailHash, GravatarHelper.Size.s64),
                        CRM = u.Doctor.CRM,
                        MedicalEntity = u.Doctor.SYS_MedicalEntity.Name,
                        MedicalSpecialty = u.Doctor.SYS_MedicalSpecialty.Name,
                    },
                    doc = u.Doctor,
                })
                .ToList();

            // Getting the next free time slot of each doctor.
            // Todo: this is going to become a problem in the future, because this info has no cache.
            foreach (var eachItem in dataCollection)
                eachItem.vm.NextAvailableTime = ScheduleController.FindNextFreeTime(this.db, eachItem.doc).Item1;

            model.Doctors = dataCollection.Select(item => item.vm).ToList();

            var currentPracticeId = this.GetCurrentUser().PracticeId;

            model.PatientsCount = this.db.Patients
                .Where(p => p.Doctor.Users.FirstOrDefault().PracticeId == currentPracticeId)
                .Count();

            return View(model);
        }
    }
}
