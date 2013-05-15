using System.Web.Mvc;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Controllers
{
    [Code.Filters.SelfOrUserRolePermission(Code.Filters.UserRoleFlags.Administrator)]
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
