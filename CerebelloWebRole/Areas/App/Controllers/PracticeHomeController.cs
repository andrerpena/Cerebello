using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Models;
using JetBrains.Annotations;

namespace CerebelloWebRole.Areas.App.Controllers
{
    [UserRolePermission(RoleFlags = UserRoleFlags.Administrator | UserRoleFlags.Owner)]
    public class PracticeHomeController : PracticeController
    {
        private Models.PracticeHomeController GetViewModel()
        {
            var timeZoneId = this.Practice.WindowsTimeZoneId;

            var timeZone = Enum.GetValues(typeof(TypeTimeZone))
                .Cast<TypeTimeZone>()
                .First(tz => TimeZoneDataAttribute.GetAttributeFromEnumValue(tz).Id == timeZoneId);

            var address = this.Practice.Address ?? new Address();

            var viewModel = new Models.PracticeHomeController
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
                    Doctors = GetDoctorViewModelsFromPractice(this.db, this.Practice, this.GetPracticeLocalNow()),
                    Users = (from u in this.Practice.Users.OrderBy(u => u.Person.FullName).ToList()
                             select UsersController.GetViewModel(u, u.Practice)).ToList()
                };

            return viewModel;
        }

        public ActionResult Index()
        {
            var viewModel = this.GetViewModel();
            return View(viewModel);
        }

        public ActionResult Edit()
        {
            var viewModel = this.GetViewModel();
            return View(viewModel);
        }

        public static List<DoctorViewModel> GetDoctorViewModelsFromPractice(CerebelloEntitiesAccessFilterWrapper db, Practice practice, DateTime localNow)
        {
            var usersThatAreDoctors = db.Users
                .Where(u => u.PracticeId == practice.Id)
                .Where(u => u.Doctor != null);

            var dataCollection = usersThatAreDoctors
                .Select(u => new
                {
                    ViewModel = new DoctorViewModel()
                    {
                        Id = u.Id,
                        Name = u.Person.FullName,
                        UrlIdentifier = u.Doctor.UrlIdentifier,
                        CRM = u.Doctor.CRM,
                        MedicalSpecialty = u.Doctor.MedicalSpecialtyName,
                    },
                    u.Doctor.MedicalEntityCode,
                    u.Doctor.MedicalEntityJurisdiction,
                    u.Doctor,
                    u.Person.EmailGravatarHash,
                })
                .ToList();

            // Getting more doctor's informations:
            // Todo: this is going to become a problem in the future, because this info has no cache.
            // - next free time slot of each doctor;
            // - gravatar image.
            foreach (var eachItem in dataCollection)
            {
                if (!string.IsNullOrEmpty(eachItem.EmailGravatarHash))
                    eachItem.ViewModel.ImageUrl = GravatarHelper.GetGravatarUrl(eachItem.EmailGravatarHash, GravatarHelper.Size.s16);

                eachItem.ViewModel.MedicalEntity = string.Format(
                    string.IsNullOrEmpty(eachItem.MedicalEntityJurisdiction) ? "{0}" : "{0}-{1}",
                    eachItem.MedicalEntityCode,
                    eachItem.MedicalEntityJurisdiction);

                // It is only possible to determine the next available time if the schedule of the doctor is already configured.
                if (eachItem.Doctor.CFG_Schedule != null)
                {
                    var nextSlotInLocalTime = ScheduleController.FindNextFreeTimeInPracticeLocalTime(db, eachItem.Doctor, localNow);
                    eachItem.ViewModel.NextAvailableTime = nextSlotInLocalTime.Item1;
                }
            }

            var doctors = dataCollection.Select(item => item.ViewModel).ToList();
            return doctors;
        }

        [HttpPost]
        public ActionResult Edit(Models.PracticeHomeController formModel)
        {
            if (this.ModelState.IsValid)
            {
                formModel.PracticeName = Regex.Replace(formModel.PracticeName.Trim(), @"\s+", " ");

                this.Practice.Name = formModel.PracticeName;
                this.Practice.WindowsTimeZoneId = TimeZoneDataAttribute.GetAttributeFromEnumValue(
                    (TypeTimeZone)formModel.PracticeTimeZone).Id;

                this.Practice.PhoneMain = formModel.PhoneMain;
                this.Practice.PhoneAlt = formModel.PhoneAlt;
                this.Practice.SiteUrl = formModel.SiteUrl;
                this.Practice.Email = formModel.Email;

                if (this.Practice.Address == null)
                    this.Practice.Address = new Address();
                this.Practice.Address.CEP = formModel.Address.CEP;
                this.Practice.Address.City = formModel.Address.City;
                this.Practice.Address.Complement = formModel.Address.Complement;
                this.Practice.Address.Neighborhood = formModel.Address.Neighborhood;
                this.Practice.Address.StateProvince = formModel.Address.StateProvince;
                this.Practice.Address.Street = formModel.Address.Street;

                this.db.SaveChanges();

                return this.RedirectToAction("Index");
            }

            return View(formModel);
        }

    }
}
