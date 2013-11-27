using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Controllers
{
    [SelfOrUserRolePermission(UserRoleFlags.Administrator)]
    public class ConfigRecordsController : DoctorController
    {
        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult MedicalRecordEditor(ConfigRecordViewModel viewModel)
        {
            return this.View(viewModel);
        }
    }
}