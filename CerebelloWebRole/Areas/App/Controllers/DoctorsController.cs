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
            
            model.Doctors = (from up in this.Practice.UserPractices where up.Doctor != null

                             select new DoctorViewModel()
                             {
                                 Name = up.User.Person.FullName,
                                 UrlIdentifier = up.User.Person.UrlIdentifier,
                                 ImageUrl = GravatarHelper.GetGravatarUrl(up.User.GravatarEmailHash, GravatarHelper.Size.s64),
                                 CRM = up.Doctor.CRM,
                                 MedicalEntity = up.Doctor.MedicalEntity.Name,
                                 MedicalSpecialty = up.Doctor.MedicalSpecialty.Name

                             }).ToList();

            return View(model);
        }
    }
}
