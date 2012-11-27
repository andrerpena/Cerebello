using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Models;

namespace CerebelloWebRole.Areas.App.Controllers
{
    [UserRolePermission(RoleFlags = UserRoleFlags.Administrator | UserRoleFlags.Owner)]
    public class ConfigPracticeController : PracticeController
    {
        private ConfigPracticeViewModel ConfigPracticeViewModel()
        {

            var timeZoneId = this.Practice.WindowsTimeZoneId;

            var timeZone = Enum.GetValues(typeof(TypeTimeZone))
                .Cast<TypeTimeZone>()
                .First(tz => TimeZoneDataAttribute.GetAttributeFromEnumValue(tz).Id == timeZoneId);

            var address = this.Practice.Address ?? new Address();

            var viewModel = new ConfigPracticeViewModel
                {
                    Address = new AddressViewModel
                        {
                            CEP = address.CEP,
                            City = address.City,
                            Complement = address.Complement,
                            Neighborhood = address.Neighborhood,
                            StateProvince = address.StateProvince,
                            Street = address.Street,
                        },
                    Email = this.Practice.Email,
                    Pabx = this.Practice.PABX,
                    PhoneAlt = this.Practice.PhoneAlt,
                    PhoneMain = this.Practice.PhoneMain,
                    PracticeName = this.Practice.Name,
                    PracticeTimeZone = (short)timeZone,
                    SiteUrl = this.Practice.SiteUrl,
                };

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
                viewModel.PracticeName = Regex.Replace(viewModel.PracticeName.Trim(), @"\s+", " ");

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
