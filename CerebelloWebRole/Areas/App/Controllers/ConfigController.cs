using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code.Controllers;
using CerebelloWebRole.Areas.App.Models;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class ConfigController : DoctorController
    {
        //
        // GET: /App/Config/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Edit(ConfigDocumentsViewModel formModel, string returnUrl)
        {
            return View();
        }
    }
}
