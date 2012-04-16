using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class PracticeHomeController : PracticeController
    {
        //
        // GET: /App/Home/

        public ActionResult Index()
        {
            return View();
        }

    }
}
