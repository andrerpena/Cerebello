using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code;
using CerebelloWebRole.Areas.App.Models;
using Cerebello.Model;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class DoctorsController : PracticeController
    {
        //
        // GET: /App/PracticeDoctors/

        public ActionResult Index()
        {
            var model = new PracticeDoctorsViewModel();
            
            model.Doctors = (from u in this.Practice.Users where u.Doctor != null

                             select new DoctorViewModel()
                             {
                                 Id = u.Id,
                                 Name = u.Person.FullName,
                                 UrlIdentifier = u.Person.UrlIdentifier,
                                 ImageUrl = GravatarHelper.GetGravatarUrl(u.GravatarEmailHash, GravatarHelper.Size.s64),
                                 CRM = u.Doctor.CRM,
                                 MedicalEntity = u.Doctor.SYS_MedicalEntity.Name,
                                 MedicalSpecialty = u.Doctor.SYS_MedicalSpecialty.Name

                             }).ToList();

            return View(model);
        }
    }
}
