using System.Web.Mvc;
using CerebelloWebRole.Code.Controllers;

namespace CerebelloWebRole.Controllers
{
    public class HomeController : RootController
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }
    }
}
