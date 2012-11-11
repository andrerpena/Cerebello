using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class LaboratoriesController : DoctorController
    {
        [HttpGet]
        public ActionResult CreateMoal()
        {
            return this.EditModal((int?)null);
        }

        [HttpGet]
        public ActionResult EditModal(int? id)
        {
            return this.View();
        }
    }
}