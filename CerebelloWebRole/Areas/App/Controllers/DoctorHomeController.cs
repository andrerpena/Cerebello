using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code.Controllers;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class DoctorHomeController : DoctorController
    {
        //
        // GET: /App/DoctorHome/

        public ActionResult Index()
        {
            return View();
        }

    }
}
