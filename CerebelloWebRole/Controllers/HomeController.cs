using System.Web.Mvc;
using CerebelloWebRole.Code;
using System.Linq;
using CerebelloWebRole.Models;

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
            this.InitDb();
            var viewModel = new WelcomeViewModel();
            var dbPractice = this.db.Practices.SingleOrDefault(p => p.UrlIdentifier == practice);
            this.InitDbUser(this.ControllerContext.RequestContext);
            if (dbPractice != null && this.DbUser.PracticeId == dbPractice.Id)
                viewModel.IsTrial = dbPractice.AccountContract.IsTrial;
            return this.View(viewModel);
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
