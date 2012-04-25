using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Areas.App.Models;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class SymptomsController : Controller
    {
        
        /// <summary>
        /// Symptoms index
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var viewModel = new SymptomsIndexViewModel();
            return View(viewModel);
        }

    }
}
