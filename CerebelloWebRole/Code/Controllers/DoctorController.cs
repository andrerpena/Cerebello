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
    public class DoctorController : PracticeController
    {
        public Doctor Doctor { get; private set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                var authenticatedPrincipal = filterContext.HttpContext.User as AuthenticatedPrincipal;
                if (authenticatedPrincipal != null)
                {
                    var doctorName = this.RouteData.Values["doctor"] as string;

                    var doctor = (
                        from Doctor d in this.db.Doctors
                        where d.UserPractice.User.Person.UrlIdentifier == doctorName
                        select d).FirstOrDefault();

                    if (doctor != null)
                    {
                        this.Doctor = doctor;
                        this.ViewBag.Doctor = new DoctorViewModel()
                             {
                                 Name = doctor.UserPractice.User.Person.FullName,
                                 UrlIdentifier = doctor.UserPractice.User.Person.UrlIdentifier,
                                 ImageUrl = GravatarHelper.GetGravatarUrl(doctor.UserPractice.User.GravatarEmailHash, GravatarHelper.Size.s32),
                                 CRM = doctor.CRM,
                                 MedicalEntity = doctor.MedicalEntity.Name,
                                 MedicalSpecialty = doctor.MedicalSpecialty.Name

                             };
                        return;
                    }
                }
            }
            filterContext.Result = new HttpUnauthorizedResult();
        }
    }
}