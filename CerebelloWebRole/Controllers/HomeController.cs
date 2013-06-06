using System.Web.Mvc;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Controllers
{
    public class HomeController : CerebelloSiteController
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return this.View();
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
