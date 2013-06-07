using System.Web.Mvc;
using CerebelloWebRole.Code;

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
