using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code.Security;
using CerebelloWebRole.Models;
using CerebelloWebRole.Areas.App.Models;
using Cerebello.Model;

namespace CerebelloWebRole.Code.Controllers
{
    public abstract class DoctorController : PracticeController
    {
        public Doctor Doctor { get; private set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // the URL's doctor identifier (doctor's name)
            var doctorIdentifier = this.RouteData.Values["doctor"] as string;

            // issue: 2 doctors with the same name would cause this query to fail
            // the doctor being visualized (not the user as a doctor)
            var doctor = this.db.Doctors.Where(d => d.Users.Any(u => u.Person.UrlIdentifier == doctorIdentifier)).FirstOrDefault();

            if (doctor != null)
            {
                this.Doctor = doctor;
                this.ViewBag.Doctor = new DoctorViewModel()
                     {
                         Name = doctor.Users.ElementAt(0).Person.FullName,
                         UrlIdentifier = doctor.Users.ElementAt(0).Person.UrlIdentifier,
                         ImageUrl = GravatarHelper.GetGravatarUrl(doctor.Users.ElementAt(0).GravatarEmailHash, GravatarHelper.Size.s32),
                         CRM = doctor.CRM,
                         MedicalEntity = doctor.SYS_MedicalEntity.Name,
                         MedicalSpecialty = doctor.SYS_MedicalSpecialty.Name
                     };
                return;
            }
        }
    }
}