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

        public ActionResult Welcome(string practice)
        {
            return this.View();
        }

        public ActionResult PricesAndPlans()
        {
            return this.View();
        }

        public ActionResult AccountCanceled(string practice)
        {
            return this.View();
        }
    }
}
