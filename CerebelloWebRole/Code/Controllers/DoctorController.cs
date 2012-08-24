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
            var doctor = this.db.Doctors.Where(d => d.UrlIdentifier == doctorIdentifier).FirstOrDefault();

            if (doctor != null)
            {
                this.Doctor = doctor;
                var doc = new DoctorViewModel()
                     {
                         Name = doctor.Users.ElementAt(0).Person.FullName,
                         UrlIdentifier = doctor.UrlIdentifier,
                         ImageUrl = GravatarHelper.GetGravatarUrl(doctor.Users.ElementAt(0).GravatarEmailHash, GravatarHelper.Size.s32),
                         CRM = doctor.CRM,
                         MedicalSpecialty = doctor.SYS_MedicalSpecialty.Name,
                         IsScheduleConfigured = doctor.CFG_Schedule != null,
                     };

                this.ViewBag.Doctor = doc;

                doc.MedicalEntity = string.Format(
                    string.IsNullOrEmpty(doctor.MedicalEntityJurisdiction) ? "{0}" : "{0}-{1}",
                    doctor.SYS_MedicalEntity.Code,
                    doctor.MedicalEntityJurisdiction);

                return;
            }
        }
    }
}