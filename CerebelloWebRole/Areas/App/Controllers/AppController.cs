using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code.Controllers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class AppController : DoctorController
    {
        [HttpGet]
        public JsonResult SearchEverything(string term)
        {
            List<AutocompleteViewModel> searchResult = new List<AutocompleteViewModel>();

            searchResult.AddRange(( from p in db.Patients
                                     where p.Person.FullName.Contains(term) && p.DoctorId == this.Doctor.Id
                                     select p).Take(5).ToList().Select(p =>
                                     new AutocompleteViewModel()
                                     {
                                         url = Url.Action("details", "patients", new { id = p.Id }),
                                         id = p.Id,
                                         value = p.Person.FullName,
                                         description = "Paciente, " +  DateTimeHelper.GetPersonAgeInWords(p.Person.DateOfBirth, true).ToString()
                                     }));

            searchResult.AddRange((from m in db.Medicines.Include("Laboratory")
                                   where m.Name.Contains(term) && m.Doctor.Id == this.Doctor.Id
                                   select m).Take(5).ToList().Select(m =>
                                       new AutocompleteViewModel()
                                       {
                                           url = Url.Action("details", "medicines", new { id = m.Id }),
                                           id = m.Id,
                                           value = m.Name,
                                           description = "Medicamento, " + m.Laboratory.Name
                                       }));

            return Json(searchResult, JsonRequestBehavior.AllowGet);
        }
    }
}
