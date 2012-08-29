using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Security;
using Cerebello.Model;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Controllers;
using CerebelloWebRole.Code.Mvc;
using CerebelloWebRole.Code.Security;
using CerebelloWebRole.Models;
using CerebelloWebRole.Areas.App.Controllers;

namespace CerebelloWebRole.Areas.Site.Controllers
{
    public class AuthenticationController : RootController
    {
        private CerebelloEntities db = null;

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            if (this.db == null)
                this.db = new CerebelloEntities();

            base.Initialize(requestContext);
        }

        /// <summary>
        /// Logs the user in or not, based on the informations provided.
        /// URL: http://www.cerebello.com.br/authentication/login
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Login(LoginViewModel loginModel)
        {
            User user;

            if (!this.ModelState.IsValid || !SecurityManager.Login(loginModel, db, out user))
            {
                ViewBag.LoginFailed = true;
                return View();
            }

#warning Todo seems to be wrong...
            // TODO: efetuar o login

            user.LastActiveOn = this.GetUtcNow();

            this.db.SaveChanges();

            if (loginModel.Password == Constants.DEFAULT_PASSWORD)
            {
                return RedirectToAction("changepassword", "users", new { area = "app", practice = loginModel.PracticeIdentifier });
            }
            else
            {
                return RedirectToAction("index", "practicehome", new { area = "app", practice = loginModel.PracticeIdentifier });
            }
        }

        /// <summary>
        /// Signs the user out
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return this.Redirect("/");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult CreateAccount()
        {
            ViewBag.MedicalSpecialtyOptions =
                this.db.SYS_MedicalSpecialty
                .ToList()
                .Select(me => new SelectListItem { Value = me.Id.ToString(), Text = me.Name })
                .ToList();

            ViewBag.MedicalEntityOptions =
                this.db.SYS_MedicalEntity
                .ToList()
                .Select(me => new SelectListItem { Value = me.Id.ToString(), Text = me.Name })
                .ToList();

            return View();
        }

        [HttpPost]
        public ActionResult CreateAccount(CreateAccountViewModel registrationData)
        {
            // Normalizing name properties.
            if (!string.IsNullOrEmpty(registrationData.PracticeName))
                registrationData.PracticeName = Regex.Replace(registrationData.PracticeName, @"\s+", " ").Trim();

            if (!string.IsNullOrEmpty(registrationData.FullName))
                registrationData.FullName = Regex.Replace(registrationData.FullName, @"\s+", " ").Trim();

            var urlPracticeId = StringHelper.GenerateUrlIdentifier(registrationData.PracticeName);

            // Note: Url identifier for the name of the user, don't need any verification.
            // The name of the user must be unique inside a practice, not the entire database.

            bool alreadyExistsPracticeId = db.Practices.Where(p => p.UrlIdentifier == urlPracticeId).Any();

            if (alreadyExistsPracticeId)
            {
                this.ModelState.AddModelError(
                    () => registrationData.PracticeName,
                    "Nome do consultório já está em uso.");
            }

            var utcNow = this.GetUtcNow();

            // Creating the new user.
            User user;
            var result = SecurityManager.CreateUser(out user, registrationData, db, utcNow, null);

            if (result == CreateUserResult.InvalidUserNameOrPassword)
            {
                // Note: nothing to do because user-name and password fields are already validated.
            }

            if (result == CreateUserResult.UserNameAlreadyInUse)
            {
                this.ModelState.AddModelError(
                    () => registrationData.UserName,
                    // Todo: this message is also used in the App -> UsersController.
                    "O nome de usuário não pode ser registrado pois já está em uso. "
                    + "Note que nomes de usuário diferenciados por acentos, "
                    + "maiúsculas/minúsculas ou por '.', '-' ou '_' não são permitidos."
                    + "(Não é possível cadastrar 'MiguelAngelo' e 'miguel.angelo' no mesmo consultório.");
            }

            // If the user being edited is a medic, then we must check the fields that are required for medics.
            if (!registrationData.IsDoctor)
            {
                // Removing validation error of medic properties, because this user is not a medic.
                this.ModelState.ClearPropertyErrors(() => registrationData.MedicCRM);
                this.ModelState.ClearPropertyErrors(() => registrationData.MedicalEntity);
                this.ModelState.ClearPropertyErrors(() => registrationData.MedicalSpecialty);
                this.ModelState.ClearPropertyErrors(() => registrationData.MedicalEntityJurisdiction);
            }

            if (user != null)
            {
                var timeZoneId = TimeZoneDataAttribute.GetAttributeFromEnumValue((TypeTimeZone)registrationData.PracticeTimeZone).Id;

                // Creating a new medical practice.
                user.Practice = new Practice
                {
                    Name = registrationData.PracticeName,
                    UrlIdentifier = urlPracticeId,
                    CreatedOn = utcNow,
                    WindowsTimeZoneId = timeZoneId,
                };

                // Setting the BirthDate of the user as a person.
                user.Person.DateOfBirth = PracticeController.ConvertToUtcDateTime(user.Practice, registrationData.DateOfBirth);

                // when the user is a doctor, we need to fill the properties of the doctor
                if (registrationData.IsDoctor)
                {
                    // if user is already a doctor, we just edit the properties
                    // otherwise we create a new doctor instance
                    if (user.Doctor == null)
                        user.Doctor = db.Doctors.CreateObject();

                    user.Doctor.CRM = registrationData.MedicCRM;
                    user.Doctor.MedicalSpecialtyId = registrationData.MedicalSpecialty ?? 0;
                    user.Doctor.MedicalEntityId = registrationData.MedicalEntity ?? 0;
                    user.Doctor.MedicalEntityJurisdiction = registrationData.MedicalEntityJurisdiction.ToString();

                    // Creating an unique UrlIdentifier for this doctor.
                    // This is the first doctor, so there will be no conflicts.
                    string urlId = UsersController.GetUniqueDoctorUrlId(this.db, registrationData.FullName, null);
                    if (urlId == null)
                    {
                        this.ModelState.AddModelError(
                            () => registrationData.FullName,
                            // Todo: this message is also used in the UserController.
                            "Quantidade máxima de homônimos excedida.");
                    }
                    user.Doctor.UrlIdentifier = urlId;
                }

                db.Users.AddObject(user);

                // If the ModelState is still valid, then save objects to the database.
                if (this.ModelState.IsValid)
                {
                    db.SaveChanges();
                    return RedirectToAction("createaccountcompleted");
                }
            }

            ViewBag.MedicalSpecialtyOptions =
                this.db.SYS_MedicalSpecialty
                .ToList()
                .Select(me => new SelectListItem { Value = me.Id.ToString(), Text = me.Name })
                .ToList();

            ViewBag.MedicalEntityOptions =
                this.db.SYS_MedicalEntity
                .ToList()
                .Select(me => new SelectListItem { Value = me.Id.ToString(), Text = me.Name })
                .ToList();

            return View(registrationData);
        }

        public ActionResult CreateAccountCompleted()
        {
            return View();
        }
    }
}
