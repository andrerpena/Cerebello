using System;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Models;
using System.Linq;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class ConfigPracticeController : PracticeController
    {
        private ConfigPracticeViewModel ConfigPracticeViewModel()
        {
            var viewModel = new ConfigPracticeViewModel();

            var timeZoneId = this.Practice.WindowsTimeZoneId;

            var timeZone = Enum.GetValues(typeof(TypeTimeZone))
                .Cast<TypeTimeZone>()
                .First(tz => TimeZoneDataAttribute.GetAttributeFromEnumValue(tz).Id == timeZoneId);

            viewModel.PracticeTimeZone = (short)timeZone;

            viewModel.PracticeName = this.Practice.Name;
            return viewModel;
        }

        public ActionResult Index()
        {
            var viewModel = this.ConfigPracticeViewModel();

            return View(viewModel);
        }

        public ActionResult Edit()
        {
            var viewModel = this.ConfigPracticeViewModel();

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Edit(ConfigPracticeViewModel viewModel)
        {
            if (this.ModelState.IsValid)
            {
                this.Practice.Name = viewModel.PracticeName;
                this.Practice.WindowsTimeZoneId = TimeZoneDataAttribute.GetAttributeFromEnumValue(
                    (TypeTimeZone)viewModel.PracticeTimeZone).Id;

                this.db.SaveChanges();

                return this.RedirectToAction("Index");
            }

            return View(viewModel);
        }

    }
}
