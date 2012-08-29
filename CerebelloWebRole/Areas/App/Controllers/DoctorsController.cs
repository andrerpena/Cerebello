using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class DoctorsController : PracticeController
    {
        public DoctorsController()
        {
            this.UserNowGetter = () => DateTimeHelper.GetTimeZoneNow();
            this.UtcNowGetter = () => DateTime.UtcNow;
        }

        public Func<DateTime> UserNowGetter { get; set; }

        public Func<DateTime> UtcNowGetter { get; set; }

        //
        // GET: /App/PracticeDoctors/

        public ActionResult Index()
        {
            var model = new PracticeDoctorsViewModel();
            model.Doctors = GetDoctorViewModelsFromPractice(this.db, this.Practice, this.UserNowGetter());
            return View(model);
        }

        public static List<DoctorViewModel> GetDoctorViewModelsFromPractice(CerebelloEntities db, Practice practice, DateTime userNow)
        {
            var usersThatAreDoctors = db.Users
                .Where(u => u.PracticeId == practice.Id)
                .Where(u => u.Doctor != null);

            var dataCollection = usersThatAreDoctors
                .Select(u => new
                {
                    ViewModel = new DoctorViewModel()
                    {
                        Id = u.Id,
                        Name = u.Person.FullName,
                        UrlIdentifier = u.Person.UrlIdentifier,
                        CRM = u.Doctor.CRM,
                        MedicalSpecialty = u.Doctor.SYS_MedicalSpecialty.Name,
                    },
                    MedicalEntityCode = u.Doctor.SYS_MedicalEntity.Code,
                    MedicalEntityJurisdiction = u.Doctor.MedicalEntityJurisdiction,
                    Doctor = u.Doctor,
                    EmailGravatarHash = u.Person.EmailGravatarHash,
                })
                .ToList();

            // Getting more doctor's informations:
            // Todo: this is going to become a problem in the future, because this info has no cache.
            // - next free time slot of each doctor;
            // - gravatar image.
            foreach (var eachItem in dataCollection)
            {
                if (!string.IsNullOrEmpty(eachItem.EmailGravatarHash))
                    eachItem.ViewModel.ImageUrl = GravatarHelper.GetGravatarUrl(eachItem.EmailGravatarHash, GravatarHelper.Size.s64);

                eachItem.ViewModel.MedicalEntity = string.Format(
                    string.IsNullOrEmpty(eachItem.MedicalEntityJurisdiction) ? "{0}" : "{0}-{1}",
                    eachItem.MedicalEntityCode,
                    eachItem.MedicalEntityJurisdiction);

                eachItem.ViewModel.NextAvailableTime = ScheduleController.FindNextFreeTime(db, eachItem.Doctor, userNow, userNow).Item1;
            }

            var doctors = dataCollection.Select(item => item.ViewModel).ToList();
            return doctors;
        }
    }
}
