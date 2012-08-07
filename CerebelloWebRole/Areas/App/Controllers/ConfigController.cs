using System.Web.Mvc;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;

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
