using System;
using System.Data.Objects;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Chat;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Json;
using CerebelloWebRole.Code.Security;
using CerebelloWebRole.Models;
using HtmlAgilityPack;

namespace CerebelloWebRole.Areas.App.Controllers
{
    /// <summary>
    /// Controller for users in the practice.
    /// Base URL: http://www.cerebello.com.br/p/consultoriodrhouse/users
    /// </summary>
    public class UsersController : PracticeController
    {
        /// <summary>
        /// Creates an UserViewModel given an User object.
        /// </summary>
        /// <param name="user">User object to be used as source of values.</param>
        /// <returns>A new UserViewModel with informations copied from the User object.</returns>
        public static UserViewModel GetViewModel(User user, Practice practice, SYS_MedicalEntity medicalEntity, SYS_MedicalSpecialty medicalSpecialty)
        {
            var viewModel = new UserViewModel()
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.Person.FullName,
                ImageUrl = GravatarHelper.GetGravatarUrl(user.Person.EmailGravatarHash, GravatarHelper.Size.s64),
                Gender = user.Person.Gender,
                DateOfBirth = ConvertToLocalDateTime(practice, user.Person.DateOfBirth),
                MaritalStatus = user.Person.MaritalStatus,
                BirthPlace = user.Person.BirthPlace,
                CPF = user.Person.CPF,
                Profissao = user.Person.Profession,
                Email = user.Person.Email,

                IsAdministrador = user.AdministratorId != null,
                IsDoctor = user.DoctorId != null,
                IsSecretary = user.SecretaryId != null,

                Address = user.Person.Address == null ? new AddressViewModel() : new AddressViewModel()
                {
                    CEP = user.Person.Address.CEP,
                    City = user.Person.Address.City,
                    Complement = user.Person.Address.Complement,
                    Neighborhood = user.Person.Address.Neighborhood,
                    StateProvince = user.Person.Address.StateProvince,
                    Street = user.Person.Address.Street
                }
            };

            var userDoctor = user.Doctor;
            if (userDoctor != null)
            {
                viewModel.MedicCRM = userDoctor.CRM;
                viewModel.MedicalSpecialtyId = medicalSpecialty.Id;
                viewModel.MedicalSpecialtyName = medicalSpecialty.Name;
                viewModel.MedicalEntityId = medicalEntity.Id;
                viewModel.MedicalEntityJurisdiction = (int)(TypeEstadoBrasileiro)Enum.Parse(
                    typeof(TypeEstadoBrasileiro),
                    user.Doctor.MedicalEntityJurisdiction);
            }

            return viewModel;
        }

        /// <summary>
        /// Gets informations for the root page of this controller.
        /// This page consists of a list of users.
        /// URL: http://www.cerebello.com.br/p/consultoriodrhouse/users
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
                    },
                    EmailGravatarHash = u.Person.EmailGravatarHash,
                }).ToList();

            foreach (var eachItem in dataCollection)
            {
                if (!string.IsNullOrEmpty(eachItem.EmailGravatarHash))
                    eachItem.vm.ImageUrl = GravatarHelper.GetGravatarUrl(eachItem.EmailGravatarHash, GravatarHelper.Size.s64);
            }

            model.Users = dataCollection.Select(item => item.vm).ToList();

            return View(model);
        }

        /// <summary>
        /// Gets informations for the page used to create new users.
        /// This page has no informations at all.
        /// URL: http://www.cerebello.com.br/p/consultoriodrhouse/users/create
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [UsersManagementPermission]
        public ActionResult Create()
        {
            return this.Edit((int?)null);
        }

        [HttpPost]
        [UsersManagementPermission]
        public ActionResult Create(UserViewModel viewModel)
        {
            return this.Edit(viewModel);
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            UserViewModel model = null;

            // ToDo: @masbicudo, eu coloquei essa linha pra evitar um crash na View.
            // Está certo isso?
            // @andrerpena: estava correto, alterei o nome da variável para ficar mais claro, pois nem eu entendi direito.
            // @andrerpena: afinal, se está dentro da action de editar então é pq está esitando, mas na verdade essa
            // @andrerpena: também é usada para criar um novo carinha.
            this.ViewBag.IsEditingOrCreating = id != null ? 'E' : 'C';

            if (id != null)
            {
                var user = this.db.Users.First(p => p.Id == id);

                var medicalEntity = GetDoctorEntity(this.db.SYS_MedicalEntity, user.Doctor);
                var medicalSpecialty = GetDoctorSpecialty(this.db.SYS_MedicalSpecialty, user.Doctor);

                model = GetViewModel(user, this.Practice, medicalEntity, medicalSpecialty);

                ViewBag.Title = "Alterando usuário: " + model.FullName;
            }
            else
                ViewBag.Title = "Novo usuário";

            this.ViewBag.IsEditingOrCreating = id != null ? 'E' : 'C';

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

        public static SYS_MedicalEntity GetDoctorEntity(IObjectSet<SYS_MedicalEntity> dbSet, Doctor doctor)
        {
            var result = doctor == null ? null
                : (dbSet.FirstOrDefault(me => me.Name == doctor.MedicalEntityName && me.Code == doctor.MedicalEntityCode)
                ?? dbSet.FirstOrDefault(me => me.Name == doctor.MedicalEntityName)
                ?? dbSet.FirstOrDefault(me => me.Code == doctor.MedicalEntityCode));
            return result;
        }

        public static SYS_MedicalSpecialty GetDoctorSpecialty(IObjectSet<SYS_MedicalSpecialty> dbSet, Doctor doctor)
        {
            var result = doctor == null ? null
                : (dbSet.FirstOrDefault(ms => ms.Name == doctor.MedicalSpecialtyName && ms.Code == doctor.MedicalSpecialtyCode)
                ?? dbSet.FirstOrDefault(ms => ms.Name == doctor.MedicalSpecialtyName)
                // Note: Code has dupplicates in the database.
                ?? dbSet.FirstOrDefault(ms => ms.Code == doctor.MedicalSpecialtyCode));
            return result;
        }

        [HttpPost]
        public ActionResult Edit(UserViewModel formModel)
        {
            var isEditingOrCreating = formModel.Id != null ? 'E' : 'C';

            this.ViewBag.IsEditingOrCreating = isEditingOrCreating;

            var utcNow = this.GetUtcNow();

            User user;

            // Normalizing the name of the person.
            if (!string.IsNullOrEmpty(formModel.FullName))
                formModel.FullName = Regex.Replace(formModel.FullName, @"\s+", " ").Trim();

            if (isEditingOrCreating == 'E')
            {
                // Note: User name cannot be edited, and should not be validated.
                this.ModelState.ClearPropertyErrors(() => formModel.UserName);

                user = db.Users.First(p => p.Id == formModel.Id);
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
                    this.ModelState.AddModelError(() => formModel.UserName, "Nome de usuário inválido.");
                }

                var loggedUser = this.DbUser;

                // Looking for another user with the same UserName or Email.
                var conflictingData = this.db.Users
                    .Where(u => u.PracticeId == loggedUser.PracticeId)
                    .Where(u => u.UserName == formModel.UserName || u.Person.Email == formModel.Email)
                    .Select(u => new { u.UserName, u.Person.Email })
                    .ToList();

                // Verifying wich fields are conflicting: Email.
#warning [Validate] Must validate all emails.
                bool emailConflict = conflictingData.Any(c => c.Email == formModel.Email);

                // For every new user we must create a login, with a common
                // password used the first time the person logs in.
                // The only action allowed with this password,
                // is to change the password.
                var userData = new CerebelloWebRole.Models.CreateAccountViewModel
                {
                    UserName = formModel.UserName,
                    Password = Constants.DEFAULT_PASSWORD,
                    ConfirmPassword = Constants.DEFAULT_PASSWORD,
                    EMail = formModel.Email,
                    FullName = formModel.FullName,
                    Gender = (short)formModel.Gender,
                };

                // Creating the new user.
                // The user belongs to the same practice as the logged user.
                var result = SecurityManager.CreateUser(out user, userData, this.db.Users, utcNow, loggedUser.PracticeId);

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
            }

#warning Must validade all emails, cannot repeat emails in the same practice.

            if (!formModel.IsDoctor && !formModel.IsAdministrador && !formModel.IsSecretary)
                this.ModelState.AddModelError("", "Usuário tem que ter pelo menos uma função: médico, administrador ou secretária.");

            // If the user being edited is a doctor, then we must check the fields that are required for medics.
            if (!formModel.IsDoctor)
            {
                // Removing validation error of medic properties, because this user is not a medic.
                this.ModelState.ClearPropertyErrors(() => formModel.MedicCRM);
                this.ModelState.ClearPropertyErrors(() => formModel.MedicalEntityId);
                this.ModelState.ClearPropertyErrors(() => formModel.MedicalSpecialtyId);
                this.ModelState.ClearPropertyErrors(() => formModel.MedicalSpecialtyName);
                this.ModelState.ClearPropertyErrors(() => formModel.MedicalEntityJurisdiction);
            }

            if (user != null)
            {
                user.Person.DateOfBirth = ConvertToUtcDateTime(this.Practice, formModel.DateOfBirth);
                user.Person.BirthPlace = formModel.BirthPlace;
                user.Person.CPF = formModel.CPF;
                user.Person.CPFOwner = formModel.CPFOwner;
                user.Person.CreatedOn = this.GetUtcNow();
                user.Person.MaritalStatus = formModel.MaritalStatus;
                user.Person.Profession = formModel.Profissao;
                user.Person.Email = formModel.Email;
                user.Person.EmailGravatarHash = GravatarHelper.GetGravatarHash(formModel.Email);

                // handle address
                if (user.Person.Address == null)
                    user.Person.Address = new Address { PracticeId = this.DbUser.PracticeId, };

                user.Person.Address.CEP = formModel.Address.CEP;
                user.Person.Address.City = formModel.Address.City;
                user.Person.Address.Complement = formModel.Address.Complement;
                user.Person.Address.Neighborhood = formModel.Address.Neighborhood;
                user.Person.Address.StateProvince = formModel.Address.StateProvince;
                user.Person.Address.Street = formModel.Address.Street;

                var practiceId = this.Practice.Id;

                // when the user is a doctor, we need to fill the properties of the doctor
                if (formModel.IsDoctor)
                {
                    // if user is already a doctor, we just edit the properties
                    // otherwise we create a new doctor instance
                    if (user.Doctor == null)
                        user.Doctor = new Doctor { PracticeId = this.DbUser.PracticeId, };

                    user.Doctor.CRM = formModel.MedicCRM;

                    if (formModel.MedicalEntityId != null)
                    {
                        var me = this.db.SYS_MedicalEntity.First(me1 => me1.Id == formModel.MedicalEntityId);
                        user.Doctor.MedicalEntityCode = me.Code;
                        user.Doctor.MedicalEntityName = me.Name;
                    }

                    if (formModel.MedicalSpecialtyId != null)
                    {
                        var ms = this.db.SYS_MedicalSpecialty.First(ms1 => ms1.Id == formModel.MedicalSpecialtyId);
                        user.Doctor.MedicalSpecialtyCode = ms.Code;
                        user.Doctor.MedicalSpecialtyName = ms.Name;
                    }

                    if (formModel.MedicalEntityJurisdiction != null)
                        user.Doctor.MedicalEntityJurisdiction = ((TypeEstadoBrasileiro)formModel.MedicalEntityJurisdiction.Value).ToString();

                    // Creating an unique UrlIdentifier for this doctor.
                    // This does not consider UrlIdentifier's used by other kinds of objects.
                    string urlId = GetUniqueDoctorUrlId(this.db.Doctors, formModel.FullName, practiceId);
                    if (urlId == null)
                    {
                        this.ModelState.AddModelError(
                            () => formModel.FullName,
                            // Todo: this message is also used in the AuthenticationController.
                            "Quantidade máxima de homônimos excedida.");
                    }
                    user.Doctor.UrlIdentifier = urlId;
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
                        user.Administrator = new Administrator { PracticeId = this.DbUser.PracticeId, };
                }
                else
                {
                    user.Administrator = null;
                }

                if (user.IsOwner)
                {
                    if (!formModel.IsAdministrador)
                        this.ModelState.AddModelError(
                            () => formModel.IsAdministrador,
                            "Cannot remove administrator role from the owner of the account.");
                }

                // when the user is a secretary
                if (formModel.IsSecretary)
                {
                    if (user.Secretary == null)
                        user.Secretary = new Secretary { PracticeId = this.DbUser.PracticeId, };
                }
                else
                {
                    user.Secretary = null;
                }
            }

            // If ModelState is still valid, save the objects to the database.
            if (this.ModelState.IsValid)
            {
                // Saving all the changes.
                db.SaveChanges();

                // Making user available in the Chat
                ChatServerHelper.SetupUserIfNonexisting(this.db, this.Practice.Id, user.Id);

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

        public static string GetUniqueDoctorUrlId(IObjectSet<Doctor> dbDoctorsSet, string fullName, int? practiceId)
        {
            // todo: this piece of code is very similar to SetPatientUniqueUrlIdentifier.

            var urlIdSrc = StringHelper.GenerateUrlIdentifier(fullName);
            var urlId = urlIdSrc;

            // todo: there is a concurrency problem here.
            int cnt = 2;
            while (dbDoctorsSet.Any(d => d.UrlIdentifier == urlId
                                       && d.Users.FirstOrDefault().PracticeId == practiceId
                                       && d.Users.FirstOrDefault().Person.FullName != fullName))
            {
                urlId = string.Format("{0}_{1}", urlIdSrc, cnt++);

                if (cnt > 20)
                    return null;
            }

            return urlId;
        }

        // TODO: add permission attribute ProfileAccessPermission
        public ActionResult Details(int id)
        {
            var user = this.db.Users.First(p => p.Id == id);

            var medicalEntity = GetDoctorEntity(this.db.SYS_MedicalEntity, user.Doctor);
            var medicalSpecialty = GetDoctorSpecialty(this.db.SYS_MedicalSpecialty, user.Doctor);

            var model = GetViewModel(user, this.Practice, medicalEntity, medicalSpecialty);

            return View(model);
        }

        [HttpGet]
        [UsersManagementPermission]
        public JsonResult Delete(int id)
        {
            try
            {
                var user = this.db.Users.First(m => m.Id == id);

                if (user.IsOwner)
                {
                    this.ModelState.AddModelError("id", "Não é possível excluir o usuário que é proprietário da conta.");
                }
                else if (id == this.DbUser.Id)
                {
                    this.ModelState.AddModelError("id", "Não é possível remover a si mesmo.");
                }
                else
                {
                    // delete appointments manually (SQL Server won't do this automatically)
                    var appointments = user.Appointments.ToList();
                    while (appointments.Any())
                    {
                        var appointment = appointments.First();
                        this.db.Appointments.DeleteObject(appointment);
                        appointments.Remove(appointment);
                    }

                    // delete chat messages received manually
                    var chatMessagesReceived = user.ChatMessagesReceived.ToList();
                    while (chatMessagesReceived.Any())
                    {
                        var chatMessageReceived = chatMessagesReceived.First();
                        this.db.ChatMessages.DeleteObject(chatMessageReceived);
                        chatMessagesReceived.Remove(chatMessageReceived);
                    }

                    // delete chat messages sent manually
                    var chatMessagesSent = user.ChatMessagesSent.ToList();
                    while (chatMessagesSent.Any())
                    {
                        var chatMessageSent = chatMessagesSent.First();
                        this.db.ChatMessages.DeleteObject(chatMessageSent);
                        chatMessagesSent.Remove(chatMessageSent);
                    }

                    this.db.Users.DeleteObject(user);
                }

                if (this.ModelState.IsValid)
                {
                    // I need to store the user I as it's going to be deleted
                    var userIdBackup = user.Id;
                    this.db.SaveChanges();

                    // Makes user unavailable in the Chat
                    ChatServerHelper.RemoveUserIfExisting(this.Practice.Id, userIdBackup);

                    return this.Json(new JsonDeleteMessage { success = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var message = this.ModelState.GetAllErrors().First().Item2.ErrorMessage;
                    return this.Json(new JsonDeleteMessage { success = false, text = message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return this.Json(new JsonDeleteMessage { success = false, text = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ChangePassword()
        {
            var loggedUser = this.DbUser;
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

            var loggedUser = this.DbUser;

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
                var user = this.db.Users.Single(u => u.Id == loggedUser.Id);
                user.Password = newPasswordHash;
                user.LastActiveOn = this.GetUtcNow();

                this.db.SaveChanges();

                // The password has changed, we need to log the user in again.
                var ok = SecurityManager.Login(
                    this.HttpContext.Response.Cookies,
                    new LoginViewModel
                    {
                        Password = vm.Password,
                        PracticeIdentifier = string.Format("{0}", this.RouteData.Values["practice"]),
                        RememberMe = false,
                        UserNameOrEmail = loggedUser.UserName,
                    }, this.db.Users, out user);

                if (!ok || user == null)
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
                var request = WebRequest.Create("http://www.buscacep.correios.com.br/servicos/dnec/consultaEnderecoAction.do");
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                using (var requestWriter = new StreamWriter(request.GetRequestStream()))
                    requestWriter.Write(String.Format("relaxation={0}&TipoCep=ALL&semelhante=N&cfm=1&Metodo=listaLogradouro&TipoConsulta=relaxation&StartRow=1&EndRow=10", cep));

                var response = request.GetResponse();

                var document = new HtmlDocument();
                document.Load(response.GetResponseStream());

                var cepInfo = new CEPInfo()
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

        [UsersManagementPermission]
        public ActionResult ResetPassword(int id, string typeTextCheck)
        {
            // Reseting the password of the user.
            var user = this.db.Users.SingleOrDefault(u => u.Id == id);

            if (user == null)
                return new HttpNotFoundResult("User does not exist.");

            if (this.ModelState.IsValid)
            {
                SecurityManager.ResetUserPassword(user);

                this.db.SaveChanges();

                return Json(new { success = true, defaultPassword = Constants.DEFAULT_PASSWORD }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, text = this.ModelState.GetAllErrors().TextMessage() }, JsonRequestBehavior.AllowGet);
        }
    }
}
