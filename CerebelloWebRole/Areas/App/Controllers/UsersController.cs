using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code;
using CerebelloWebRole.Areas.App.Models;
using Cerebello.Model;
using CerebelloWebRole.Code.Mvc;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using CerebelloWebRole.Code.Json;
using System.Text.RegularExpressions;
using CerebelloWebRole.Code.Security;

namespace CerebelloWebRole.Areas.App.Controllers
{
    /// <summary>
    /// Controller for users in the practice.
    /// Base URL: http://www.cerebello.com.br/p/consultoriodrhourse/users
    /// </summary>
    public class UsersController : PracticeController
    {
        /// <summary>
        /// Creates an UserViewModel given an User object.
        /// </summary>
        /// <param name="user">User object to be used as source of values.</param>
        /// <returns>A new UserViewModel with informations copied from the User object.</returns>
        public static UserViewModel GetViewModel(User user)
        {
            var viewModel = new UserViewModel()
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.Person.FullName,
                UrlIdentifier = user.Person.UrlIdentifier,
                ImageUrl = GravatarHelper.GetGravatarUrl(user.GravatarEmailHash, GravatarHelper.Size.s64),
                Gender = user.Person.Gender,
                DateOfBirth = user.Person.DateOfBirth,
                MaritalStatus = user.Person.MaritalStatus,
                BirthPlace = user.Person.BirthPlace,
                CPF = user.Person.CPF,
                Profissao = user.Person.Profession,

                IsAdministrador = user.AdministratorId != null,
                IsMedic = user.DoctorId != null,
                IsSecretary = user.SecretaryId != null,

                Emails = (from e in user.Person.Emails
                          select new EmailViewModel()
                          {
                              Id = e.Id,
                              Address = e.Address
                          }).ToList(),

                Addresses = (from a in user.Person.Addresses
                             select new AddressViewModel()
                             {
                                 Id = a.Id,
                                 CEP = a.CEP,
                                 City = a.City,
                                 Complement = a.Complement,
                                 Neighborhood = a.Neighborhood,
                                 StateProvince = a.StateProvince,
                                 Street = a.Street
                             }).ToList()
            };

            if (user.Doctor != null)
            {
                viewModel.MedicCRM = user.Doctor.CRM;
                viewModel.MedicalSpecialty = user.Doctor.MedicalSpecialtyId;
                viewModel.MedicalEntity = user.Doctor.MedicalEntityId;
                viewModel.MedicalSpecialtyJurisdiction = user.Doctor.MedicalEntityJurisdiction;
            }

            return viewModel;
        }

        /// <summary>
        /// Gets informations for the root page of this controller.
        /// This page consists of a list of users.
        /// URL: http://www.cerebello.com.br/p/consultoriodrhourse/users
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var model = new PracticeUsersViewModel();

            var dataCollection =
                this.Practice.Users.Select(u =>
                new
                {
                    vm = new UserViewModel
                    {
                        Id = u.Id,
                        FullName = u.Person.FullName,
                        UrlIdentifier = u.Person.UrlIdentifier,
                    },
                    u.GravatarEmailHash,
                }).ToList();

            foreach (var eachItem in dataCollection)
            {
                if (!string.IsNullOrEmpty(eachItem.GravatarEmailHash))
                    eachItem.vm.ImageUrl = GravatarHelper.GetGravatarUrl(eachItem.GravatarEmailHash, GravatarHelper.Size.s64);
            }

            model.Users = dataCollection.Select(item => item.vm).ToList();

            return View(model);
        }

        /// <summary>
        /// Gets informations for the page used to create new users.
        /// This page has no informations at all.
        /// URL: http://www.cerebello.com.br/p/consultoriodrhourse/users/create
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Create()
        {
            return this.Edit((int?)null);
        }

        [HttpPost]
        public ActionResult Create(UserViewModel viewModel)
        {
            return this.Edit(viewModel);
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            UserViewModel model = null;

            if (id != null)
            {
                var user = db.Users.Where(p => p.Id == id).First();
                model = GetViewModel(user);

                ViewBag.Title = "Alterando usuário: " + model.FullName;
            }
            else
                ViewBag.Title = "Novo usuário";

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

            return View("Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(UserViewModel formModel)
        {
            bool isEditing = formModel.Id != null;

            User user;

            // Normalizing the name of the person.
            formModel.FullName = Regex.Replace(formModel.FullName, @"\s+", " ").Trim();

            if (isEditing)
            {
                // Note: User name cannot be edited, and should not be validated.
                this.ModelState.ClearPropertyErrors(() => formModel.UserName);

                user = db.Users.Where(p => p.Id == formModel.Id).First();
                user.Person.DateOfBirth = formModel.DateOfBirth;
                user.Person.FullName = formModel.FullName;
                user.Person.Gender = (short)formModel.Gender;

                // If there are model errors, we must return original user name to the view.
                formModel.UserName = user.UserName;
            }
            else
            {
                // UserName must not be null nor empty.
                if (string.IsNullOrWhiteSpace(formModel.UserName))
                {
                    this.ModelState.AddModelError(
                        () => formModel.UserName,
                        "Nome de usuário inválido.");
                }

                var firstEmail = formModel.Emails.FirstOrDefault();
                string userEmailStr = firstEmail != null ? firstEmail.Address : null;

                var loggedUser = this.GetCurrentUser();

                // Looking for another user with the same UserName or Email.
                var conflictingData = this.db.Users
                    .Where(u => u.PracticeId == loggedUser.PracticeId)
                    .Where(u => u.UserName == formModel.UserName || u.Email == userEmailStr)
                    .Select(u => new { u.UserName, u.Email })
                    .ToList();

                // Verifying wich fields are conflicting: Email.
#warning [Validate] Must validate all emails.
                bool emailConflict = conflictingData.Any(c => c.Email == userEmailStr);

                // For every new user we must create a login, with a common
                // password used the first time the person logs in.
                // The only action allowed with this password,
                // is to change the password.
                var userData = new CerebelloWebRole.Models.CreateAccountViewModel
                {
                    UserName = formModel.UserName,
                    Password = Constants.DEFAULT_PASSWORD,
                    ConfirmPassword = Constants.DEFAULT_PASSWORD,
                    DateOfBirth = formModel.DateOfBirth,
                    EMail = userEmailStr,
                    FullName = formModel.FullName,
                    Gender = (short)formModel.Gender,
                };

                // Creating the new user.
                // The user belongs to the same practice as the logged user.
                var result = SecurityManager.CreateUser(out user, userData, this.db, loggedUser.PracticeId);

                if (result == CreateUserResult.UserNameAlreadyInUse)
                {
                    this.ModelState.AddModelError(
                        () => formModel.UserName,
                        // Todo: this message is also used in the AuthenticationController.
                        "O nome de usuário não pode ser registrado pois já está em uso. "
                        + "Note que nomes de usuário diferenciados por acentos, "
                        + "maiúsculas/minúsculas ou por '.', '-' ou '_' não são permitidos."
                        + "(Não é possível cadastrar 'MiguelAngelo' e 'miguel.angelo' no mesmo consultório.");
                }

                if (result == CreateUserResult.CouldNotCreateUrlIdentifier)
                {
                    this.ModelState.AddModelError(
                        () => formModel.FullName,
                        // Todo: this message is also used in the AuthenticationController.
                        "Quantidade máxima de homônimos excedida.");
                }
            }

#warning Must validade all emails, cannot repeat emails in the same practice.

            if (!formModel.IsMedic && !formModel.IsAdministrador && !formModel.IsSecretary)
                this.ModelState.AddModelError("", "Usuário tem que ter pelo menos uma função: médico, administrador ou secretária.");

            // If the user being edited is a medic, then we must check the fields that are required for medics.
            if (formModel.IsMedic)
            {
                if (string.IsNullOrWhiteSpace(formModel.MedicCRM))
                    this.ModelState.AddModelError(
                        () => formModel.MedicCRM,
                        "CRM do médico é requerido.");
            }
            else
            {
                // Removing validation error of medic properties, because this user is not a medic.
                this.ModelState.ClearPropertyErrors(() => formModel.MedicCRM);
                this.ModelState.ClearPropertyErrors(() => formModel.MedicalEntity);
                this.ModelState.ClearPropertyErrors(() => formModel.MedicalSpecialty);
                this.ModelState.ClearPropertyErrors(() => formModel.MedicalSpecialtyJurisdiction);
            }

            if (user != null)
            {
                user.Person.BirthPlace = formModel.BirthPlace;
                user.Person.CPF = formModel.CPF;
                user.Person.CPFOwner = formModel.CPFOwner;
                user.Person.CreatedOn = DateTime.UtcNow;
                user.Person.UrlIdentifier = StringHelper.GenerateUrlIdentifier(formModel.FullName);
                user.Person.MaritalStatus = formModel.MaritalStatus;
                user.Person.Profession = formModel.Profissao;

                // when the user is a doctor, we need to fill the properties of the doctor
                if (formModel.IsMedic)
                {
                    // if user is already a doctor, we just edit the properties
                    // otherwise we create a new doctor instance
                    if (user.Doctor == null)
                        user.Doctor = db.Doctors.CreateObject();

                    user.Doctor.CRM = formModel.MedicCRM;
                    user.Doctor.MedicalSpecialtyId = formModel.MedicalSpecialty ?? 0;
                    user.Doctor.MedicalEntityId = formModel.MedicalEntity ?? 0;
                    user.Doctor.MedicalEntityJurisdiction = formModel.MedicalSpecialtyJurisdiction;
                }
                else
                {
                    // if the user is not a doctor, then we make sure
                    // by assigning the doctor property to null
                    user.Doctor = null;
                }

                // when the user is an administrator
                if (formModel.IsAdministrador)
                {
                    if (user.Administrator == null)
                        user.Administrator = db.Administrators.CreateObject();
                }
                else
                {
                    user.Administrator = null;
                }

                // when the user is a secretary
                if (formModel.IsSecretary)
                {
                    if (user.Secretary == null)
                        user.Secretary = db.Secretaries.CreateObject();
                }
                else
                {
                    user.Secretary = null;
                }

                user.Person.Addresses.Update(
                    formModel.Addresses,
                    (vm, m) => vm.Id == m.Id,
                    (vm, m) =>
                    {
                        m.CEP = vm.CEP;
                        m.City = vm.City;
                        m.Complement = vm.Complement;
                        m.Neighborhood = vm.Neighborhood;
                        m.StateProvince = vm.StateProvince;
                        m.Street = vm.Street;
                    },
                    (m) => this.db.Addresses.DeleteObject(m)
                );

                user.Person.Emails.Update(
                    formModel.Emails,
                    (vm, m) => vm.Id == m.Id,
                    (vm, m) =>
                    {
                        m.Address = vm.Address;
                    },
                    (m) => this.db.Emails.DeleteObject(m)
                );
            }

            // If ModelState is still valid, save the objects to the database.
            if (this.ModelState.IsValid)
            {
                // Saving all the changes.
                db.SaveChanges();

                return RedirectToAction("details", new { id = user.Id });
            }

            ViewBag.MedicalSpecialtyOptions =
                this.db.SYS_MedicalSpecialty
                .ToList()
                .Select(ms => new SelectListItem { Value = ms.Id.ToString(), Text = ms.Name })
                .ToList();

            ViewBag.MedicalEntityOptions =
                this.db.SYS_MedicalEntity
                .ToList()
                .Select(me => new SelectListItem { Value = me.Id.ToString(), Text = me.Name })
                .ToList();

            return View("Edit", formModel);
        }

        public ActionResult Details(int id)
        {
            var user = (User)db.Users.Where(p => p.Id == id).First();
            var model = GetViewModel(user);

            return View(model);
        }

        [HttpGet]
        public JsonResult Delete(int id)
        {
            try
            {
                var user = db.Users.Where(m => m.Id == id).First();

                // delete appointments manulally (SQL Server won't do this automatically)
                var appointments = user.Appointments.ToList();
                while (appointments.Any())
                {
                    var appointment = appointments.First();
                    this.db.Appointments.DeleteObject(appointment);
                    appointments.Remove(appointment);
                }

                this.db.Users.DeleteObject(user);
                this.db.SaveChanges();
                return this.Json(new JsonDeleteMessage { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return this.Json(new JsonDeleteMessage { success = false, text = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult ChangePassword()
        {
            var loggedUser = this.GetCurrentUser();
            var defaultPasswordHash = CipherHelper.Hash(Constants.DEFAULT_PASSWORD, loggedUser.PasswordSalt);
            bool isDefaultPwd = defaultPasswordHash == loggedUser.Password;
            this.ViewBag.IsDefaultPassword = isDefaultPwd;

            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(PasswordViewModel vm)
        {
            if (vm.Password == Constants.DEFAULT_PASSWORD)
                this.ModelState.AddModelError(
                    () => vm.Password,
                    string.Format("A senha não pode ser '{0}'.", Constants.DEFAULT_PASSWORD));

            // todo: checking password strength would be nice.

            if (vm.Password != vm.RepeatPassword)
                this.ModelState.AddModelError(() => vm.RepeatPassword, "A senha desejada deve ser repetida.");

            var loggedUser = this.GetCurrentUser();

            // Checking the current password (the one that will become old)
            // - this is needed to allow the person to go away from the computer...
            //     no one can go there and just change the password.
            // - this is not needed if the current password is the default password.
            var defaultPasswordHash = CipherHelper.Hash(Constants.DEFAULT_PASSWORD, loggedUser.PasswordSalt);
            bool isDefaultPwd = defaultPasswordHash == loggedUser.Password;
            if (isDefaultPwd)
            {
                this.ModelState.Remove("OldPassword");
            }
            else
            {
                var oldPasswordHash = CipherHelper.Hash(vm.OldPassword, loggedUser.PasswordSalt);
                if (loggedUser.Password != oldPasswordHash)
                    this.ModelState.AddModelError(() => vm.RepeatPassword, "A senha atual está incorreta.");
            }

            if (this.ModelState.IsValid)
            {
                var newPasswordHash = CipherHelper.Hash(vm.Password, loggedUser.PasswordSalt);

                // Salvando informações do usuário.
                var user = this.db.Users.Where(u => u.Id == loggedUser.Id).Single();
                user.Password = newPasswordHash;
                user.LastActiveOn = DateTime.Now;

                this.db.SaveChanges();

                // The password has changed, we need to log the user in again.
                var ok = SecurityManager.Login(new CerebelloWebRole.Models.LoginViewModel
                {
                    Password = vm.Password,
                    PracticeIdentifier = string.Format("{0}", this.RouteData.Values["practice"]),
                    RememberMe = false,
                    UserNameOrEmail = loggedUser.UserName,
                }, this.db);

                if (!ok)
                    throw new Exception("This should never happen as the login uses the same data provided by the user.");

                return RedirectToAction("index", "practicehome");
            }

            return View();
        }


        public JsonResult GetCEPInfo(string cep)
        {
            // TODO: Miguel Angelo: copiei este código de PatientsController, deveria ser um método utilitário, ou criar uma classe de base.

            try
            {
                var request = HttpWebRequest.Create("http://www.buscacep.correios.com.br/servicos/dnec/consultaEnderecoAction.do");
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                using (StreamWriter requestWriter = new StreamWriter(request.GetRequestStream()))
                    requestWriter.Write(String.Format("relaxation={0}&TipoCep=ALL&semelhante=N&cfm=1&Metodo=listaLogradouro&TipoConsulta=relaxation&StartRow=1&EndRow=10", cep));

                var response = request.GetResponse();

                HtmlDocument document = new HtmlDocument();
                document.Load(response.GetResponseStream());

                CEPInfo cepInfo = new CEPInfo()
                {
                    Street = document.DocumentNode.SelectSingleNode("//*[@id='lamina']/div[2]/div[2]/div[2]/div/table[1]/tr/td[1]").InnerText,
                    Neighborhood = document.DocumentNode.SelectSingleNode("//*[@id='lamina']/div[2]/div[2]/div[2]/div/table[1]/tr/td[2]").InnerText,
                    City = document.DocumentNode.SelectSingleNode("//*[@id='lamina']/div[2]/div[2]/div[2]/div/table[1]/tr/td[3]").InnerText,
                    StateProvince = document.DocumentNode.SelectSingleNode("//*[@id='lamina']/div[2]/div[2]/div[2]/div/table[1]/tr/td[4]").InnerText
                };

                return Json(cepInfo, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return null;
            }
        }

        public ActionResult AddressEditor(AddressViewModel viewModel)
        {
            return View(viewModel);
        }
    }
}
