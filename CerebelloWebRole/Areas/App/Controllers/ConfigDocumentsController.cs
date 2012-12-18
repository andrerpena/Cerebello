using System.Web.Mvc;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Filters;

namespace CerebelloWebRole.Areas.App.Controllers
{
    [SelfOrUserRolePermission(RoleFlags = UserRoleFlags.Administrator)]
    public class ConfigDocumentsController : DoctorController
    {
        [HttpGet]
        public ActionResult Edit(string returnUrl)
        {
            var config = this.Doctor.CFG_Documents;

            var viewModel = new ConfigDocumentsViewModel()
            {
                Header1 = config.Header1,
                Header2 = config.Header2,
                FooterLeft1 = config.FooterLeft1,
                FooterLeft2 = config.FooterLeft2,
                FooterRight1 = config.FooterRight1,
                FooterRight2 = config.FooterRight2
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Edit(ConfigDocumentsViewModel formModel, string returnUrl)
        {
            if (this.ModelState.IsValid)
            {
                var config = this.Doctor.CFG_Documents;

                config.Header1 = formModel.Header1;
                config.Header2 = formModel.Header2;
                config.FooterLeft1 = formModel.FooterLeft1;
                config.FooterLeft2 = formModel.FooterLeft2;
                config.FooterRight1 = formModel.FooterRight1;
                config.FooterRight2 = formModel.FooterRight2;

                this.db.SaveChanges();

                if (!string.IsNullOrEmpty(returnUrl))
                    return this.Redirect(returnUrl);

                return this.RedirectToAction("index", "config");
            }

            return this.View(formModel);
        }

        //
        // GET: /App/ConfigDocuments/

        public ActionResult Index()
        {
            return View();
        }

        
    }
}
