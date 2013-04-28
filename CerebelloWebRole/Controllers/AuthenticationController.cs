using System;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Controllers;
using CerebelloWebRole.Code.Helpers;
using CerebelloWebRole.Code.Hubs;
using CerebelloWebRole.Code.Notifications;
using CerebelloWebRole.Code.Notifications.Data;
using CerebelloWebRole.Code.Security;
using CerebelloWebRole.Models;

namespace CerebelloWebRole.Controllers
{
    public class AuthenticationController : RootController
    {
        private CerebelloEntities db = null;

        /// <summary>
        /// Login page, that allows an user to log into the software.
        /// </summary>
        /// <remarks>
        /// Requiriments:
        ///     - Should populate the practice identifier if it's present in the passed returnUrl
        /// </remarks>
        /// <param name="returnUrl">The url that the login button should direct user, after heshe logs in.</param>
        /// <returns>Returns a ViewResult to show tha page.</returns>
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            var viewModel = new LoginViewModel();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                try
                {
                    // extract practice name from returnUrl
                    var routeData = RouteHelper.GetRouteDataByUrl("~" + returnUrl);
                    if (routeData != null && routeData.Values.ContainsKey("practice"))
                        viewModel.PracticeIdentifier = (string)routeData.Values["practice"];

                    if (this.User.Identity.IsAuthenticated)
                        this.ModelState.AddModelError(
                            "returnUrl",
                            "Suas credenciais não te permitem acessar esta área do Cerebello. "
                            + "Entre com as credenciais apropriadas nos campos abaixo.");
                    else
                        this.ModelState.AddModelError(
                            "returnUrl",
                            "Entre com suas credenciais para poder acessar o Cerebello.");

                    viewModel.ReturnUrl = returnUrl;
                }
                catch
                {
                    // the returnUrl must be invalid, let's just ignore it
                }
            }

            return this.View(viewModel);
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            this.db = this.CreateNewCerebelloEntities();

            base.Initialize(requestContext);
        }

        /// <summary>
        /// Logs the user in or not, based on the informations provided.
        /// URL: https://www.cerebello.com.br/authentication/login
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Login(LoginViewModel loginModel)
        {
            User user;

            var cookieCollection = this.HttpContext.Response.Cookies;

            if (!SecurityManager.Login(cookieCollection, loginModel, this.db.Users, out user, this.GetUtcNow()))
            {
                this.ModelState.Clear();
                this.ModelState.AddModelError(() => loginModel.PracticeIdentifier, "As credenciais informadas não estão corretas.");
            }

            if (!this.ModelState.IsValid)
            {
                this.ViewBag.LoginFailed = true;
                return this.View(loginModel);
            }

            user.LastActiveOn = this.GetUtcNow();
            user.SYS_PasswordAlt = null; // clearing sys password (this password can only be used once)

            this.db.SaveChanges();

            // We only use the returnUrl if it is a valid URL
            // allowing an invalid URL is a security issue
            {
                bool useReturnUrl = false;
                if (!string.IsNullOrEmpty(loginModel.ReturnUrl))
                {
                    try
                    {
                        // extract practice name from returnUrl
                        var routeData = RouteHelper.GetRouteDataByUrl("~" + loginModel.ReturnUrl);
                        if (routeData.Values.ContainsKey("practice"))
                            useReturnUrl = loginModel.PracticeIdentifier == (string)routeData.Values["practice"];
                    }
                    catch
                    {
                        // the returnUrl must be invalid, let's just ignore it
                    }
                }

                if (!useReturnUrl)
                    loginModel.ReturnUrl = null;
            }

            if (loginModel.Password == Constants.DEFAULT_PASSWORD)
            {
                return this.RedirectToAction("ChangePassword", "Users", new { area = "App", practice = loginModel.PracticeIdentifier });
            }
            else if (!string.IsNullOrWhiteSpace(loginModel.ReturnUrl))
            {
                return this.Redirect(loginModel.ReturnUrl);
            }
            else
            {
                // if the user is a doctor, redirect to the specific doctor profile
                if (user.DoctorId != null)
                    return this.RedirectToAction("Index", "DoctorHome", new { area = "App", practice = loginModel.PracticeIdentifier, doctor = user.Doctor.UrlIdentifier });

                return this.RedirectToAction("Index", "PracticeHome", new { area = "App", practice = loginModel.PracticeIdentifier });
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
            this.db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult CreateAccount()
        {
            this.ViewBag.MedicalSpecialtyOptions =
                this.db.SYS_MedicalSpecialty
                .ToList()
                .Select(me => new SelectListItem { Value = me.Id.ToString(), Text = me.Name })
                .ToList();

            this.ViewBag.MedicalEntityOptions =
                this.db.SYS_MedicalEntity
                .ToList()
                .Select(me => new SelectListItem { Value = me.Id.ToString(), Text = me.Name })
                .ToList();

            return this.View();
        }

        [HttpPost]
        public ActionResult CreateAccount(CreateAccountViewModel registrationData)
        {
            if (this.ModelState.Remove(e => e.ErrorMessage.Contains("requerido")))
                this.ModelState.AddModelError("MultipleItems", "É necessário preencher todos os campos.");

            // Normalizing name properties.
            if (!string.IsNullOrEmpty(registrationData.PracticeName))
                registrationData.PracticeName = Regex.Replace(registrationData.PracticeName, @"\s+", " ").Trim();

            if (!string.IsNullOrEmpty(registrationData.FullName))
                registrationData.FullName = Regex.Replace(registrationData.FullName, @"\s+", " ").Trim();

            var urlPracticeId = StringHelper.GenerateUrlIdentifier(registrationData.PracticeName);

            // Note: Url identifier for the name of the user, don't need any verification.
            // The name of the user must be unique inside a practice, not the entire database.

            bool alreadyExistsPracticeId = this.db.Practices.Any(p => p.UrlIdentifier == urlPracticeId);

            if (alreadyExistsPracticeId)
            {
                this.ModelState.AddModelError(
                    () => registrationData.PracticeName,
                    "Nome do consultório já está em uso.");
            }

            var utcNow = this.GetUtcNow();

            // Creating the new user.
            User user;
            var result = SecurityManager.CreateUser(out user, registrationData, this.db.Users, utcNow, null);

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
                this.ModelState.ClearPropertyErrors(() => registrationData.MedicalEntityId);
                this.ModelState.ClearPropertyErrors(() => registrationData.MedicalSpecialtyId);
                this.ModelState.ClearPropertyErrors(() => registrationData.MedicalSpecialtyName);
                this.ModelState.ClearPropertyErrors(() => registrationData.MedicalEntityJurisdiction);
            }

            if (user != null)
            {
                string timeZoneId = null;
                if (registrationData.PracticeProvince != null)
                    timeZoneId = TimeZoneDataAttribute.GetAttributeFromEnumValue((TypeEstadoBrasileiro)registrationData.PracticeProvince.Value).Id;

                user.Practice = new Practice
                {
                    Name = registrationData.PracticeName,
                    UrlIdentifier = urlPracticeId,
                    CreatedOn = utcNow,
                    WindowsTimeZoneId = timeZoneId,
                    Province = registrationData.PracticeProvince,
                };

                // Setting the BirthDate of the user as a person.
                user.Person.DateOfBirth = PracticeController.ConvertToUtcDateTime(user.Practice, registrationData.DateOfBirth ?? new DateTime());

                user.IsOwner = true;

                user.Administrator = new Administrator { };

                bool isNewDoctor = false;
                // when the user is a doctor, we need to fill the properties of the doctor
                if (registrationData.IsDoctor)
                {
                    // if user is already a doctor, we just edit the properties
                    // otherwise we create a new doctor instance
                    if (user.Doctor == null)
                    {
                        user.Doctor = new Doctor();
                        isNewDoctor = true;
                    }

                    user.Doctor.CRM = registrationData.MedicCRM;

                    if (registrationData.MedicalSpecialtyId != null)
                    {
                        var ms = this.db.SYS_MedicalSpecialty
                                     .Single(ms1 => ms1.Id == registrationData.MedicalSpecialtyId);
                        user.Doctor.MedicalSpecialtyCode = ms.Code;
                        user.Doctor.MedicalSpecialtyName = ms.Name;
                    }

                    if (registrationData.MedicalEntityId != null)
                    {
                        var me = this.db.SYS_MedicalEntity
                                     .Single(me1 => me1.Id == registrationData.MedicalEntityId);
                        user.Doctor.MedicalEntityCode = me.Code;
                        user.Doctor.MedicalEntityName = me.Name;
                    }

                    user.Doctor.MedicalEntityJurisdiction = ((TypeEstadoBrasileiro)registrationData.MedicalEntityJurisdiction).ToString();

                    // Creating an unique UrlIdentifier for this doctor.
                    // This is the first doctor, so there will be no conflicts.
                    var urlId = UsersController.GetUniqueDoctorUrlId(this.db.Doctors, registrationData.FullName, null);
                    if (urlId == null)
                    {
                        this.ModelState.AddModelError(
                            () => registrationData.FullName,
                            // Todo: this message is also used in the UserController.
                            "Quantidade máxima de homônimos excedida.");
                    }

                    user.Doctor.UrlIdentifier = urlId;
                }

                this.db.Users.AddObject(user);

                if (this.ModelState.IsValid)
                {
                    // Creating confirmation email, with the token.
                    TokenId tokenId;

                    // Setting verification token.
                    using (var db2 = this.CreateNewCerebelloEntities())
                    {
                        var token = db2.GLB_Token.CreateObject();
                        token.Value = Guid.NewGuid().ToString("N");
                        token.Type = "VerifyPracticeAndEmail";
                        token.Name = string.Format("Practice={0}&UserName={1}", user.Practice.UrlIdentifier, user.UserName);
                        token.ExpirationDate = utcNow.AddDays(30);
                        db2.GLB_Token.AddObject(token);
                        db2.SaveChanges();

                        tokenId = new TokenId(token.Id, token.Value);
                    }

                    // Rendering message bodies from partial view.
                    var emailViewModel = new UserEmailViewModel(user) { Token = tokenId.ToString(), };
                    var toAddress = new MailAddress(user.Person.Email, user.Person.FullName);
                    var emailMessageToUser = this.CreateEmailMessage("ConfirmationEmail", toAddress, emailViewModel);

                    // sending e-mail to cerebello@cerebello.com.br
                    // to tell us the good news
                    var emailViewModel2 = new InternalCreateAccountEmailViewModel(user, registrationData);
                    var toAddress2 = new MailAddress("cerebello@cerebello.com.br", registrationData.FullName);
                    var mailMessage2 = this.CreateEmailMessagePartial("InternalCreateAccountEmail", toAddress2, emailViewModel2);
                    this.SendEmailAsync(mailMessage2).ContinueWith(t =>
                        {
                            // observing exception so that it is not raised
                            var ex = t.Exception;

                            // todo: should do something when e-mail is not sent
                            // 1) use a schedule table to save a serialized e-mail, and then send it later
                            // 2) log a warning message somewhere stating that this e-mail was not sent
                            // send e-mail again is not an option, SendEmailAsync already tries a lot of times
                        });

                    // If the ModelState is still valid, then save objects to the database,
                    // and send confirmation email message to the user.
                    using (emailMessageToUser)
                    {
                        // Saving changes to the DB.
                        this.db.SaveChanges();

                        // Creating a new medical practice.
                        var trialContract = new AccountContract
                        {
                            Practice = user.Practice,

                            ContractTypeId = (int)ContractTypes.TrialContract,
                            IsTrial = true,
                            IssuanceDate = utcNow,
                            StartDate = utcNow,
                            EndDate = null, // indeterminated
                            CustomText = null,

                            DoctorsLimit = null,
                            PatientsLimit = 50, // fixed limit for trial account

                            // no billings
                            BillingAmount = null,
                            BillingDueDay = null,
                            BillingPaymentMethod = null,
                            BillingPeriodCount = null,
                            BillingPeriodSize = null,
                            BillingPeriodType = null,
                            BillingDiscountAmount = null,
                        };

                        user.Practice.AccountContract = trialContract;

                        this.db.AccountContracts.AddObject(trialContract);

                        this.db.SaveChanges();

                        // if the new user is a doctor, create some other useful things
                        // like some medical-certificates and a default health-insurance
                        if (isNewDoctor)
                            BusHelper.FillNewDoctorUtilityBelt(user.Doctor);

                        // adding message to the user so that he/she completes his/her profile informations
                        // todo: add complete profile notification
                        var notificationData = new CompletePracticeInfoNotificationData();
                        var notificationDataString = new JavaScriptSerializer().Serialize(notificationData);
                        var dbNotification = new Notification()
                            {
                                CreatedOn = this.GetUtcNow(),
                                PracticeId = user.PracticeId,
                                Data = notificationDataString,
                                UserToId = user.Id,
                                Type = NotificationConstants.COMPLETE_INFO_NOTIFICATION_TYPE
                            };
                        this.db.Notifications.AddObject(dbNotification);
                        NotificationsHub.BroadcastDbNotification(dbNotification, notificationData);

                        user.Practice.Owner = user;
                        user.Person.PracticeId = user.PracticeId;
                        user.Administrator.PracticeId = user.PracticeId;
                        if (user.Doctor != null)
                            user.Doctor.PracticeId = user.PracticeId;
                        this.db.SaveChanges();

                        // Sending the confirmation e-mail to the new user.
                        // This must be synchronous.
                        this.TrySendEmail(emailMessageToUser);

                        // Log the user in.
                        var loginModel = new LoginViewModel
                        {
                            Password = registrationData.Password,
                            PracticeIdentifier = user.Practice.UrlIdentifier,
                            RememberMe = false,
                            UserNameOrEmail = registrationData.UserName,
                        };

                        if (!SecurityManager.Login(this.HttpContext.Response.Cookies, loginModel, this.db.Users, out user, this.GetUtcNow()))
                        {
                            throw new Exception("Login cannot fail.");
                        }

                        return this.RedirectToAction("CreateAccountCompleted", new { practice = user.Practice.UrlIdentifier });
                    }
                }
            }

            this.ViewBag.MedicalSpecialtyOptions =
                this.db.SYS_MedicalSpecialty
                .ToList()
                .Select(me => new SelectListItem { Value = me.Id.ToString(), Text = me.Name })
                .ToList();

            this.ViewBag.MedicalEntityOptions =
                this.db.SYS_MedicalEntity
                .ToList()
                .Select(me => new SelectListItem { Value = me.Id.ToString(), Text = me.Name })
                .ToList();

            return this.View(registrationData);
        }

        public ActionResult CreateAccountCompleted(string practice, bool? mustValidateEmail)
        {
            this.ViewBag.MustValidateEmail = mustValidateEmail;
            return this.View(new VerifyPracticeAndEmailViewModel { PracticeIdentifier = practice });
        }

        #region ResetPassword: Request
        [AcceptVerbs(new[] { "Get", "Post" })]
        public ActionResult VerifyPracticeAndEmail(VerifyPracticeAndEmailViewModel viewModel)
        {
            var utcNow = this.GetUtcNow();

            User user = null;

            // If user is not logged yet, we will use the userName and password to login.
            if (this.Request.IsAuthenticated)
            {
                var authenticatedPrincipal = this.User as AuthenticatedPrincipal;

                if (authenticatedPrincipal == null)
                    throw new Exception(
                        "HttpContext.User should be a AuthenticatedPrincipal when the user is authenticated");

                if (authenticatedPrincipal.Profile.PracticeIdentifier == viewModel.PracticeIdentifier)
                    user = this.db.Users.FirstOrDefault(u => u.Id == authenticatedPrincipal.Profile.Id);
            }

            if (user != null || this.Request.HttpMethod == "GET")
            {
                this.ModelState.Remove(() => viewModel.PracticeIdentifier);
                this.ModelState.Remove(() => viewModel.UserNameOrEmail);
                this.ModelState.Remove(() => viewModel.Password);
                this.ModelState.Remove(() => viewModel.Token);
            }

            if (user == null)
            {
                var loginModel = new LoginViewModel
                {
                    PracticeIdentifier = viewModel.PracticeIdentifier ?? "",
                    UserNameOrEmail = viewModel.UserNameOrEmail ?? "",
                    Password = viewModel.Password ?? "",
                    RememberMe = viewModel.RememberMe,
                };

                var cookieCollection = this.HttpContext.Response.Cookies;
                if (!this.ModelState.IsValid ||
                    !SecurityManager.Login(cookieCollection, loginModel, this.db.Users, out user, this.GetUtcNow()))
                {
                    this.ViewBag.LoginFailed = true;
                }
                else
                {
                    user.LastActiveOn = this.GetUtcNow();
                    user.SYS_PasswordAlt = null;

                    this.db.SaveChanges();

                    if (loginModel.Password == Constants.DEFAULT_PASSWORD)
                        throw new Exception("Cannot create initial user with a default password.");
                }
            }

            var isTokenValid = TokenId.IsValid(viewModel.Token);
            GLB_Token token = null;
            if (isTokenValid && user != null)
            {
                var tokenId = new TokenId(viewModel.Token);

                // Getting verification token, using the informations.
                var tokenName = string.Format("Practice={0}&UserName={1}", viewModel.PracticeIdentifier, user.UserName);
                token = this.db.GLB_Token.SingleOrDefault(tk =>
                                                              tk.Id == tokenId.Id
                                                              && tk.Value == tokenId.Value
                                                              && tk.Type == "VerifyPracticeAndEmail"
                                                              && tk.Name == tokenName);
            }

            var practice = this.db.Practices
                .SingleOrDefault(p => p.UrlIdentifier == viewModel.PracticeIdentifier);

            if (token == null)
                isTokenValid = false;

            if (practice == null && this.Request.HttpMethod != "GET" && !this.ModelState.HasPropertyErrors(() => viewModel.PracticeIdentifier))
                this.ModelState.AddModelError(() => viewModel.PracticeIdentifier, "Consultório não foi achado.");

            if (token != null && practice != null)
            {
                // setting practice verification data
                if (utcNow <= token.ExpirationDate)
                {
                    practice.VerificationDate = utcNow;
                }
                else
                    isTokenValid = false;

                // Destroying token... it has been used with success, and is no longer needed.
                this.db.GLB_Token.DeleteObject(token);

                // Saving changes.
                // Note: even if ModelState.IsValid is false,
                // we need to save the changes to invalidate token when it expires.
                this.db.SaveChanges();
            }

            if (!isTokenValid && user != null)
            {
                this.ModelState.AddModelError(() => viewModel.Token, "Problema com o token.");
            }

            if (this.ModelState.IsValid && user != null)
            {
                return this.RedirectToAction("Welcome", "Home",
                                             new { area = "", practice = practice.UrlIdentifier });
            }

            viewModel.Password = null; // cannot allow password going to the view.
            return this.View(viewModel);
        }

        public ActionResult ResetPasswordRequest()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult ResetPasswordRequest(IdentityViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel.PracticeIdentifier))
                return this.View(viewModel);

            if (string.IsNullOrWhiteSpace(viewModel.UserNameOrEmail))
                return this.View(viewModel);

            // Can only reset password if practice has already been verified.
            var practice = this.db.Practices.SingleOrDefault(p => p.UrlIdentifier == viewModel.PracticeIdentifier);

            var user = SecurityManager.GetUser(this.db.Users, viewModel.PracticeIdentifier, viewModel.UserNameOrEmail);

            if (practice == null || user == null)
            {
                this.ModelState.ClearPropertyErrors(() => viewModel.PracticeIdentifier);
                this.ModelState.ClearPropertyErrors(() => viewModel.UserNameOrEmail);
                this.ModelState.AddModelError(
                    () => viewModel.PracticeIdentifier,
                    "O consultório ou usuário não existem. Por favor verifique se não cometeu nenhum erro de digitação.");
            }

            if (practice != null && practice.VerificationDate == null && user != null)
                this.ModelState.AddModelError(() => viewModel.PracticeIdentifier, "Não é possível resetar a senha pois o consultório ainda não foi verificado. Confirme o seu e-mail antes de tentar mudar a senha.");

            if (this.ModelState.IsValid)
            {
                var utcNow = this.GetUtcNow();

                // Creating confirmation email, with the token.
                MailMessage message;

                if (user.Person.Email != null)
                {
                    #region Creating token and e-mail message

                    // Setting verification token.
                    // Note: tokens are safe to save even if validation fails.
                    TokenId tokenId;
                    using (var db2 = this.CreateNewCerebelloEntities())
                    {
                        var token = db2.GLB_Token.CreateObject();
                        token.Value = Guid.NewGuid().ToString("N");
                        token.Type = "ResetPassword";
                        token.Name = string.Format(
                            "Practice={0}&UserName={1}",
                            user.Practice.UrlIdentifier,
                            user.UserName);
                        token.ExpirationDate = utcNow.AddDays(30);
                        db2.GLB_Token.AddObject(token);
                        db2.SaveChanges();

                        tokenId = new TokenId(token.Id, token.Value);
                    }

                    // Rendering message bodies from partial view.
                    var emailViewModel = new UserEmailViewModel(user) { Token = tokenId.ToString(), };
                    var toAddress = new MailAddress(user.Person.Email, user.Person.FullName);
                    message = this.CreateEmailMessagePartial("ResetPasswordEmail", toAddress, emailViewModel);

                    #endregion
                }
                else
                {
                    return this.RedirectToAction("ResetPasswordManually");
                }

                // If the ModelState is still valid, then save objects to the database,
                // and send confirmation email message to the user.
                using (message)
                {
                    if (this.ModelState.IsValid)
                    {
                        try
                        {
                            // Sending the password reset e-mail to the user.
                            this.TrySendEmail(message);
                        }
                        catch (SmtpException)
                        {
                            // if e-mail was not sent, try to send it again, after 10 seconds
                            Thread.Sleep(10000);
                            this.TrySendEmail(message);
                        }

                        return this.RedirectToAction("ResetPasswordEmailSent");
                    }
                }
            }

            return this.View(viewModel);
        }

        public ActionResult ResetPasswordManually()
        {
            return this.View();
        }

        public ActionResult ResetPasswordEmailSent()
        {
            return this.View();
        }
        #endregion

        #region ResetPassword: Action
        public ActionResult ResetPassword(ResetPasswordViewModel viewModel, bool getRequest = true)
        {
            var utcNow = this.GetUtcNow();

            var user = SecurityManager.GetUser(this.db.Users, viewModel.PracticeIdentifier, viewModel.UserNameOrEmail);

            // Getting token information, so that we can locate the token in the database.
            var tokenInfo = new TokenId(viewModel.Token);

            var tokenName = string.Format("Practice={0}&UserName={1}", viewModel.PracticeIdentifier, user.UserName);

            // Destroying the token.
            var token = this.db.GLB_Token.SingleOrDefault(tk =>
                                                          tk.Id == tokenInfo.Id
                                                          && tk.Value == tokenInfo.Value
                                                          && tk.ExpirationDate >= utcNow
                                                          && tk.Type == "ResetPassword"
                                                          && tk.Name == tokenName);

            if (token == null)
                this.ViewBag.CannotRedefinePassword = true;

            return this.View();
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordViewModel viewModel)
        {
            var utcNow = this.GetUtcNow();

            var user = SecurityManager.GetUser(this.db.Users, viewModel.PracticeIdentifier, viewModel.UserNameOrEmail);

            if (user != null)
            {
                // Getting token information, so that we can locate the token in the database.
                var tokenInfo = new TokenId(viewModel.Token);

                var tokenName = string.Format("Practice={0}&UserName={1}", viewModel.PracticeIdentifier, user.UserName);

                // Destroying the token.
                var token = this.db.GLB_Token.SingleOrDefault(tk =>
                                                              tk.Id == tokenInfo.Id
                                                              && tk.Value == tokenInfo.Value
                                                              && tk.ExpirationDate >= utcNow
                                                              && tk.Type == "ResetPassword"
                                                              && tk.Name == tokenName);

                if (token == null)
                {
                    this.ViewBag.CannotRedefinePassword = true;
                    return this.View();
                }

                if (this.ModelState.IsValid)
                {
                    SecurityManager.SetUserPassword(this.db.Users, viewModel.PracticeIdentifier, viewModel.UserNameOrEmail, viewModel.NewPassword);

                    this.db.GLB_Token.DeleteObject(token);

                    this.db.SaveChanges();

                    return this.RedirectToAction(
                        "ResetPasswordSuccess",
                        new IdentityViewModel
                            {
                                PracticeIdentifier = viewModel.PracticeIdentifier,
                                UserNameOrEmail = viewModel.UserNameOrEmail
                            });
                }
            }

            return this.View(viewModel);
        }

        public ActionResult ResetPasswordCancel(ResetPasswordViewModel viewModel)
        {
            var utcNow = this.GetUtcNow();

            var user = SecurityManager.GetUser(this.db.Users, viewModel.PracticeIdentifier, viewModel.UserNameOrEmail);

            if (user != null)
            {
                // Getting token information, so that we can locate the token in the database.
                var tokenInfo = new TokenId(viewModel.Token);

                var tokenName = string.Format("Practice={0}&UserName={1}", viewModel.PracticeIdentifier, user.UserName);

                // Destroying the token.
                var token = this.db.GLB_Token.SingleOrDefault(tk =>
                                                              tk.Id == tokenInfo.Id
                                                              && tk.Value == tokenInfo.Value
                                                              && tk.ExpirationDate >= utcNow
                                                              && tk.Type == "ResetPassword"
                                                              && tk.Name == tokenName);

                if (token != null)
                {
                    this.db.GLB_Token.DeleteObject(token);

                    this.db.SaveChanges();
                }
                else
                    this.ViewBag.CannotRedefinePassword = true;
            }

            return this.View();
        }

        public ActionResult ResetPasswordSuccess(IdentityViewModel viewModel)
        {
            return this.View(viewModel);
        }
        #endregion

        public ActionResult LogoutLogin(string returnUrl)
        {
            this.Logout();
            return this.RedirectToAction("Login");
        }
    }
}
