using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Chat;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Hubs;
using CerebelloWebRole.Code.Json;
using CerebelloWebRole.Code.Notifications;
using CerebelloWebRole.Code.Notifications.Data;
using CerebelloWebRole.Code.Security;
using CerebelloWebRole.Models;
using HtmlAgilityPack;
using JetBrains.Annotations;

namespace CerebelloWebRole.Areas.App.Controllers
{
    /// <summary>
    /// Controller for users in the practice.
    /// Base URL: https://www.cerebello.com.br/p/consultoriodrhouse/users
    /// </summary>
    public class UsersController : PracticeController
    {
        public override bool IsSelfUser(User user)
        {
            if (this.IsActionName("Edit")
                || this.IsActionName("Details")
                || this.IsActionName("Delete")
                || this.IsActionName("ResetPassword"))
            {
                var context = this.ControllerContext;
                var idObj = context.RequestContext.RouteData.Values["id"] ?? "";
                int id;
                var isValidId = int.TryParse(idObj.ToString(), out id);

                if (isValidId)
                    return user.Id == id;
            }

            return base.IsSelfUser(user);
        }

        private bool IsActionName([AspMvcAction] string actionName)
        {
            var context = this.ControllerContext;
            var curActionName = context.RequestContext.RouteData.GetRequiredString("action");
            var result = String.Compare(curActionName, actionName, StringComparison.OrdinalIgnoreCase) == 0;
            return result;
        }

        /// <summary>
        /// Gets informations for the page used to create new users.
        /// This page has no informations at all.
        /// URL: https://www.cerebello.com.br/p/consultoriodrhouse/users/create
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [UserRolePermission(UserRoleFlags.Administrator)]
        public ActionResult Create()
        {
            return this.Edit((int?)null);
        }

        [HttpPost]
        [UserRolePermission(UserRoleFlags.Administrator)]
        public ActionResult Create(UserViewModel viewModel)
        {
            return this.Edit(viewModel);
        }

        /// <summary>
        /// Creates an UserViewModel given an User object.
        /// </summary>
        /// <param name="user">User object to be used as source of values.</param>
        /// <param name="practice"> </param>
        /// <param name="medicalEntity">medical entity, if the user is a doctor. If medical entity is null, medical entity won't be added to the view-model even if the user is a doctor</param>
        /// <param name="medicalSpecialty">medical specialty, if the user is a doctor. If medical specialty is null, medical specialty won't be added to the view-model even if the user is a doctor</param>
        /// <returns>A new UserViewModel with informations copied from the User object.</returns>
        public static UserViewModel GetViewModel(
            [NotNull] User user, [NotNull] Practice practice, SYS_MedicalEntity medicalEntity = null, SYS_MedicalSpecialty medicalSpecialty = null)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (practice == null) throw new ArgumentNullException("practice");

            var address = user.Person.Addresses.SingleOrDefault();

            var viewModel = new UserViewModel();

            FillUserViewModel(user, practice, viewModel);

            viewModel.Address = address == null
                                    ? new AddressViewModel()
                                    : new AddressViewModel
                                          {
                                              CEP = address.CEP,
                                              City = address.City,
                                              Complement = address.Complement,
                                              Neighborhood = address.Neighborhood,
                                              StateProvince = address.StateProvince,
                                              Street = address.Street
                                          };

            var userDoctor = user.Doctor;
            if (userDoctor != null)
                FillDoctorViewModel(user, medicalEntity, medicalSpecialty, viewModel, userDoctor);

            return viewModel;
        }

        internal static void FillUserViewModel(User user, Practice practice, UserViewModel viewModel)
        {
            viewModel.Id = user.Id;
            viewModel.UserName = user.UserName;

            viewModel.FullName = user.Person.FullName;
            viewModel.ImageUrl = GravatarHelper.GetGravatarUrl(user.Person.EmailGravatarHash, GravatarHelper.Size.s16);
            viewModel.Gender = user.Person.Gender;
            viewModel.DateOfBirth = ConvertToLocalDateTime(practice, user.Person.DateOfBirth);
            viewModel.MaritalStatus = user.Person.MaritalStatus;
            viewModel.BirthPlace = user.Person.BirthPlace;
            viewModel.Cpf = user.Person.CPF;
            viewModel.Profissao = user.Person.Profession;
            viewModel.Email = user.Person.Email;

            viewModel.IsAdministrador = user.AdministratorId != null;
            viewModel.IsDoctor = user.DoctorId != null;
            viewModel.IsSecretary = user.SecretaryId != null;
            viewModel.IsOwner = user.IsOwner;
        }

        internal static void FillDoctorViewModel(User user, SYS_MedicalEntity medicalEntity, SYS_MedicalSpecialty medicalSpecialty, UserViewModel viewModel, Doctor doctor)
        {
            viewModel.MedicCRM = doctor.CRM;
            viewModel.MedicalSpecialtyId = medicalSpecialty != null ? medicalSpecialty.Id : (int?)null;
            viewModel.MedicalSpecialtyName = medicalSpecialty != null ? medicalSpecialty.Name : null;
            viewModel.MedicalEntityId = medicalEntity != null ? medicalEntity.Id : (int?)null;
            viewModel.MedicalEntityName = medicalEntity != null ? medicalEntity.Name : null;
            viewModel.MedicalEntityJurisdiction = (int)(TypeEstadoBrasileiro)Enum.Parse(
                typeof(TypeEstadoBrasileiro),
                user.Doctor.MedicalEntityJurisdiction);
        }

        [HttpGet]
        [SelfOrUserRolePermission(UserRoleFlags.Administrator)]
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
                var user = this.db.Users.FirstOrDefault(p => p.Id == id);
                if (user == null)
                    return this.ObjectNotFound();

                var medicalEntity = GetDoctorEntity(this.db.SYS_MedicalEntity, user.Doctor);
                var medicalSpecialty = GetDoctorSpecialty(this.db.SYS_MedicalSpecialty, user.Doctor);

                model = GetViewModel(user, this.DbPractice, medicalEntity, medicalSpecialty);

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

            if (this.DbUser.AdministratorId != null || this.DbUser.IsOwner)
                this.ViewBag.CanEditRole = true;

            return this.View("Edit", model);
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
        [SelfOrUserRolePermission(UserRoleFlags.Administrator)]
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

                // TODO: suggest that r# use the attribute EdmScalarPropertyAttribute(IsNullable=false)
                // as a way to determine if a property can ever receive a null value or not
                // there was a bug in the line inside the following if, that could be detected by r# if it did consider that attribute.
                if (!string.IsNullOrWhiteSpace(formModel.FullName))
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

                // Checking doctors limit of this account.
                if (formModel.IsDoctor)
                {
                    var doctorsCount = this.DbPractice.Users.Count(u => u.DoctorId != null);
                    if (doctorsCount >= this.DbPractice.AccountContract.DoctorsLimit)
                        this.ModelState.AddModelError(
                            "DoctorsLimit",
                            "Essa conta está configurada para suportar até {0} médicos.",
                            this.DbPractice.AccountContract.DoctorsLimit);
                }

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
                var userData = new CreateAccountViewModel
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
                user.Person.DateOfBirth = ConvertToUtcDateTime(this.DbPractice, formModel.DateOfBirth);
                user.Person.BirthPlace = formModel.BirthPlace;
                user.Person.CPF = formModel.Cpf;
                user.Person.CPFOwner = formModel.CpfOwner;
                user.Person.CreatedOn = this.GetUtcNow();
                user.Person.MaritalStatus = formModel.MaritalStatus;
                user.Person.Profession = formModel.Profissao;
                user.Person.Email = formModel.Email;
                user.Person.EmailGravatarHash = GravatarHelper.GetGravatarHash(formModel.Email);

                // handle address
                if (!user.Person.Addresses.Any())
                    user.Person.Addresses.Add(
                        new Address
                            {
                                PracticeId = this.DbUser.PracticeId,
                                CEP = formModel.Address.CEP,
                                City = formModel.Address.City,
                                Complement = formModel.Address.Complement,
                                Neighborhood = formModel.Address.Neighborhood,
                                StateProvince = formModel.Address.StateProvince,
                                Street = formModel.Address.Street,
                            });

                // when the user is a doctor, we need to fill the properties of the doctor
                if (formModel.IsDoctor)
                {
                    // Only administrators can change the role of an user.
                    if (this.DbUser.AdministratorId != null)
                    {
                        // if user is already a doctor, we just edit the properties
                        // otherwise we create a new doctor instance
                        if (user.Doctor == null)
                        {
                            user.Doctor = new Doctor { PracticeId = this.DbUser.PracticeId, };
                            BusHelper.FillNewDoctorUtilityBelt(user.Doctor);
                        }
                    }

                    // Changing the doctor's informations.
                    if (!string.IsNullOrWhiteSpace(formModel.MedicCRM))
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
                        user.Doctor.MedicalEntityJurisdiction =
                            ((TypeEstadoBrasileiro)formModel.MedicalEntityJurisdiction.Value).ToString();

                    // Creating an unique UrlIdentifier for this doctor.
                    // This does not consider UrlIdentifier's used by other kinds of objects.
                    string urlId = GetUniqueDoctorUrlId(this.db.Doctors, formModel.FullName, this.DbPractice.Id);
                    if (urlId == null && !string.IsNullOrWhiteSpace(formModel.FullName))
                    {
                        this.ModelState.AddModelError(
                            () => formModel.FullName,
                            // Todo: this message is also used in the AuthenticationController.
                            string.Format("Quantidade máxima de homônimos excedida para esta conta: {0}.", this.DbPractice.UrlIdentifier));
                    }

                    if (!string.IsNullOrWhiteSpace(urlId))
                        user.Doctor.UrlIdentifier = urlId;
                }
                else
                {
                    // Only administrators can change the role of an user.
                    if (this.DbUser.AdministratorId != null)
                    {
                        if (user.Doctor != null)
                            this.db.Doctors.DeleteObject(user.Doctor);

                        // if the user is not a doctor, then we make sure
                        // by assigning the doctor property to null
                        user.Doctor = null;
                    }
                }

                // when the user is an administrator
                if (formModel.IsAdministrador)
                {
                    // Only administrators can change the role of an user.
                    if (this.DbUser.AdministratorId != null)
                        if (user.Administrator == null)
                            user.Administrator = new Administrator { PracticeId = this.DbUser.PracticeId, };
                }
                else
                {
                    // Only administrators can change the role of an user.
                    if (this.DbUser.AdministratorId != null)
                    {
                        if (user.Administrator != null)
                            this.db.Administrators.DeleteObject(user.Administrator);
                        user.Administrator = null;
                    }
                }

                if (user.IsOwner)
                {
                    if (!formModel.IsAdministrador)
                        this.ModelState.AddModelError(
                            () => formModel.IsAdministrador,
                            "Não é possível remover o papel de administrador do proprietário da conta.");
                }

                // when the user is a secretary
                if (formModel.IsSecretary)
                {
                    // Only administrators can change the role of an user.
                    if (this.DbUser.AdministratorId != null)
                        if (user.Secretary == null)
                            user.Secretary = new Secretary { PracticeId = this.DbUser.PracticeId, };
                }
                else
                {
                    // Only administrators can change the role of an user.
                    if (this.DbUser.AdministratorId != null)
                    {
                        if (user.Secretary != null)
                            this.db.Secretaries.DeleteObject(user.Secretary);
                        user.Secretary = null;
                    }
                }
            }

            // If ModelState is still valid, save the objects to the database.
            if (this.ModelState.IsValid)
            {
                if (formModel.Id == null)
                {
                    var notificationData = new NewUserCreatedNotificationData() { UserName = user.UserName };
                    var dbNotification = new Notification()
                        {
                            CreatedOn = this.GetUtcNow(),
                            PracticeId = this.DbPractice.Id,
                            UserToId = this.DbUser.Id,
                            Type = NotificationConstants.NEW_USER_NOTIFICATION_TYPE,
                            Data = new JavaScriptSerializer().Serialize(notificationData)
                        };
                    this.db.Notifications.AddObject(dbNotification);
                    this.db.SaveChanges();
                    NotificationsHub.BroadcastDbNotification(dbNotification, notificationData);
                }

                // Saving all the changes.
                db.SaveChanges();

                return this.RedirectToAction("Details", new { id = user.Id });
            }

            this.ViewBag.MedicalSpecialtyOptions =
                this.db.SYS_MedicalSpecialty
                .ToList()
                .Select(ms => new SelectListItem { Value = ms.Id.ToString(), Text = ms.Name })
                .ToList();

            this.ViewBag.MedicalEntityOptions =
                this.db.SYS_MedicalEntity
                .ToList()
                .Select(me => new SelectListItem { Value = me.Id.ToString(), Text = me.Name })
                .ToList();

            if (this.DbUser.AdministratorId != null || this.DbUser.IsOwner)
                this.ViewBag.CanEditRole = true;

            // Removes all duplicated messages.
            this.ModelState.RemoveDuplicates();

            return this.View("Edit", formModel);
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
            var user = this.db.Users.FirstOrDefault(p => p.Id == id);
            if (user == null)
                return this.ObjectNotFound();

            var medicalEntity = GetDoctorEntity(this.db.SYS_MedicalEntity, user.Doctor);
            var medicalSpecialty = GetDoctorSpecialty(this.db.SYS_MedicalSpecialty, user.Doctor);

            var model = GetViewModel(user, this.DbPractice, medicalEntity, medicalSpecialty);

            this.ViewBag.IsSelf = user.Id == this.DbUser.Id;

            return View(model);
        }

        [HttpGet]
        [UserRolePermission(UserRoleFlags.Administrator, StatusDescription = "Você não tem permissão para excluir um usuário.")]
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

            return this.View();
        }

        [HttpPost]
        public ActionResult ChangePassword(PasswordViewModel vm)
        {
            if (vm.Password == Constants.DEFAULT_PASSWORD)
                this.ModelState.AddModelError(
                    () => vm.Password,
                    string.Format("A senha não pode ser '{0}'.", Constants.DEFAULT_PASSWORD));

            // todo: checking password strength would be nice.
            Debug.Assert(this.DbUser.Person != null, "'this.DbUser' must not be null.");

            if (vm.Password != vm.RepeatPassword)
                this.ModelState.AddModelError(() => vm.RepeatPassword, "A senha desejada deve ser repetida.");

            var loggedUser = this.DbUser;

            // Checking the current password (the one that will become old)
            // - this is needed to allow the person to go away from the computer...
            //     no one can go there and just change the password.
            // - this is not needed if the current password is the default password.
            var defaultPasswordHash = CipherHelper.Hash(Constants.DEFAULT_PASSWORD, loggedUser.PasswordSalt);
            bool isDefaultPwd = defaultPasswordHash == loggedUser.Password;
            this.ViewBag.IsDefaultPassword = isDefaultPwd;
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
                user.SYS_PasswordAlt = null;

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
                    }, this.db.Users, out user, this.GetUtcNow());

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

        [UserRolePermission(UserRoleFlags.Administrator)]
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
