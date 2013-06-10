using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class PracticeHomeController : PracticeController
    {
        private PracticeHomeControllerViewModel GetViewModel()
        {
            var timeZoneId = this.DbPractice.WindowsTimeZoneId;

            var timeZone = Enum.GetValues(typeof(TypeTimeZone))
                .Cast<TypeTimeZone>()
                .First(tz => TimeZoneDataAttribute.GetAttributeFromEnumValue(tz).Id == timeZoneId);

            var address = this.DbPractice.Address ?? new Address();

            var viewModel = new PracticeHomeControllerViewModel
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
                    Email = this.DbPractice.Email,
                    Pabx = this.DbPractice.PABX,
                    PhoneAlt = this.DbPractice.PhoneAlt,
                    PhoneMain = this.DbPractice.PhoneMain,
                    PracticeName = this.DbPractice.Name,
                    PracticeTimeZone = (short)timeZone,
                    SiteUrl = this.DbPractice.SiteUrl,
                    Doctors = GetDoctorViewModelsFromPractice(this.db, this.DbPractice, this.GetPracticeLocalNow()),
                    Users = (from u in this.DbPractice.Users.OrderBy(u => u.Person.FullName).ToList()
                             select UsersController.GetViewModel(u, u.Practice)).ToList()
                };

            return viewModel;
        }

        public ActionResult Index()
        {
            var viewModel = this.GetViewModel();
            return View(viewModel);
        }

        [UserRolePermission(UserRoleFlags.Administrator)]
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
        [UserRolePermission(UserRoleFlags.Administrator)]
        public ActionResult Edit(PracticeHomeControllerViewModel formModel)
        {
            if (this.ModelState.IsValid)
            {
                formModel.PracticeName = Regex.Replace(formModel.PracticeName.Trim(), @"\s+", " ");

                this.DbPractice.Name = formModel.PracticeName;
                this.DbPractice.WindowsTimeZoneId = TimeZoneDataAttribute.GetAttributeFromEnumValue(
                    (TypeTimeZone)formModel.PracticeTimeZone).Id;

                this.DbPractice.PhoneMain = formModel.PhoneMain;
                this.DbPractice.PhoneAlt = formModel.PhoneAlt;
                this.DbPractice.SiteUrl = formModel.SiteUrl;
                this.DbPractice.Email = formModel.Email;

                var address = this.DbPractice.Address;
                if (address == null)
                    this.DbPractice.Address = address = new Address();
                address.PracticeId = this.DbPractice.Id;
                address.CEP = formModel.Address.CEP;
                address.City = formModel.Address.City;
                address.Complement = formModel.Address.Complement;
                address.Neighborhood = formModel.Address.Neighborhood;
                address.StateProvince = formModel.Address.StateProvince;
                address.Street = formModel.Address.Street;

                this.db.SaveChanges();

                return this.RedirectToAction("Index");
            }

            return this.View(formModel);
        }

        public ActionResult RedirectToDoctorHome()
        {
            if (this.DbUser != null)
            {
                var practice = this.DbPractice.UrlIdentifier;
                if (this.DbUser.DoctorId != null)
                {
                    var doctor = this.DbUser.Doctor.UrlIdentifier;
                    var url = this.Url.Action("Index", "DoctorHome", new { practice, doctor, area = "App" });
                    return this.Redirect(url);
                }
                else
                {
                    var url = this.Url.Action("Index", "PracticeHome", new { practice, area = "App" });
                    return this.Redirect(url);
                }
            }

            // todo: this should never happen
            return this.RedirectToAction("Index");
        }
    }
}
