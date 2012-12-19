using System.Web.Mvc;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Filters;

namespace CerebelloWebRole.Areas.App.Controllers
{
    [SelfOrUserRolePermission(UserRoleFlags.Administrator)]
    public class ConfigController : DoctorController
    {
        //
        // GET: /App/Config/

        public ActionResult Index()
        {
            return View();
        }
    }
}
