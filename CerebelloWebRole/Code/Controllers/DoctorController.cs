using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;

namespace CerebelloWebRole.Code
{
    public abstract class DoctorController : PracticeController
    {
        public Doctor Doctor { get; set; }

        public override bool IsSelfUser(User user)
        {
            var doctorUrlId = this.ControllerContext.RouteData.GetRequiredString("doctor");
            if (user.DoctorId != null)
                return user.Doctor.UrlIdentifier == doctorUrlId;

            return base.IsSelfUser(user);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // if the base has already set a result, then we just exit this method
            if (filterContext.Result != null)
                return;

            this.InitDoctor();
        }

        private bool wasInitDoctorCalled;
        public void InitDoctor()
        {
            if (this.wasInitDoctorCalled)
                return;

            this.wasInitDoctorCalled = true;

            // the URL's doctor identifier (doctor's name)
            var doctorIdentifier = this.RouteData.Values["doctor"] as string;

            // Getting list of all doctors in this practice.
            var allDoctors = this.db.Doctors
                .Include("Users")
                .Include("Users.Person")
                .ToList();

            // Resolved: uniqueness of UrlIdentifier is ensured.
            // issue: 2 doctors with the same name would cause this query to fail
            // the doctor being visualized (not the user as a doctor)
            var doctor = allDoctors
                .FirstOrDefault(d => d.UrlIdentifier == doctorIdentifier);

            Debug.Assert(doctor != null, "doctor must not be null");
            //if (doctor == null)
            //    return;

            this.Doctor = doctor;

            var doctorViewModels = allDoctors.Select(doc => new DoctorViewModel
                {
                    Id = doc.Id,
                    Name = doc.Users.ElementAt(0).Person.FullName,
                    UrlIdentifier = doc.UrlIdentifier,
                    ImageUrl = GravatarHelper.GetGravatarUrl(doc.Users.ElementAt(0).Person.EmailGravatarHash, GravatarHelper.Size.s32),
                    CRM = doc.CRM,
                    MedicalSpecialty = doc.MedicalSpecialtyName,
                    IsScheduleConfigured = doc.CFG_Schedule != null,
                    MedicalEntity = string.Format(
                        string.IsNullOrEmpty(doc.MedicalEntityJurisdiction) ? "{0}" : "{0}-{1}",
                        doc.MedicalEntityCode,
                        doc.MedicalEntityJurisdiction),
                })
                .ToList();

            this.ViewBag.Doctor = doctorViewModels.Where(doc => doc.Id == doctor.Id).FirstOrDefault();

            this.ViewBag.AllDoctors = doctorViewModels;
        }
    }
}