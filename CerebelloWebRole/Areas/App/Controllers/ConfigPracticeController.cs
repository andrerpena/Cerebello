using System.Web.Mvc;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class ConfigPracticeController : PracticeController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
