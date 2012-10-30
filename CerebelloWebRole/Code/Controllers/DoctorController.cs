using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;

namespace CerebelloWebRole.Code
{
    public abstract class DoctorController : PracticeController
    {
        public Doctor Doctor { get; private set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // the URL's doctor identifier (doctor's name)
            var doctorIdentifier = this.RouteData.Values["doctor"] as string;

            // Resolved: uniqueness of UrlIdentifier is ensured.
            // issue: 2 doctors with the same name would cause this query to fail
            // the doctor being visualized (not the user as a doctor)
            var doctor = this.db.Doctors.Include("Users").Include("Users.Person").FirstOrDefault(d => d.UrlIdentifier == doctorIdentifier);

            if (doctor == null)
                return;

            this.Doctor = doctor;
            var doc = new DoctorViewModel()
                {
                    Name = doctor.Users.ElementAt(0).Person.FullName,
                    UrlIdentifier = doctor.UrlIdentifier,
                    ImageUrl = GravatarHelper.GetGravatarUrl(doctor.Users.ElementAt(0).Person.EmailGravatarHash, GravatarHelper.Size.s32),
                    CRM = doctor.CRM,
                    MedicalSpecialty = doctor.MedicalSpecialtyName,
                    IsScheduleConfigured = doctor.CFG_Schedule != null,
                };

            this.ViewBag.Doctor = doc;

            doc.MedicalEntity = string.Format(
                string.IsNullOrEmpty(doctor.MedicalEntityJurisdiction) ? "{0}" : "{0}-{1}",
                doctor.MedicalEntityCode,
                doctor.MedicalEntityJurisdiction);
        }
    }
}