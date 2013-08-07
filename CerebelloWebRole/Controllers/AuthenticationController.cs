using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Models;
using PayPal.Version940;

namespace CerebelloWebRole.Controllers
{
    public class AuthenticationController : RootController
    {
        private CerebelloEntities db = null;
        private Practice dbPractice = null;
        private User dbUser = null;

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            this.db = this.CreateNewCerebelloEntities();
            base.Initialize(requestContext);
        }

        protected override void Dispose(bool disposing)
        {
            this.db.Dispose();
            base.Dispose(disposing);
        }

        #region Login/Logout
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

        public ActionResult LogoutLogin(string returnUrl)
        {
            this.Logout();
            return this.RedirectToAction("Login");
        }
        #endregion

        #region CreateAccount

        public static string defaultSubscription = "free";
        private static readonly HashSet<string> validSubscriptions = new HashSet<string>(new[]
            {
                //"trial",
                "1M",
                //"3M",
                "6M",
                "12M",
                "free",
                //"unlimited",
            }, StringComparer.InvariantCultureIgnoreCase);

        public ActionResult CreateAccount(string subscription)
        {
            subscription = StringHelper.FirstNonEmpty(subscription, defaultSubscription);
            if (!validSubscriptions.Contains(subscription))
                subscription = defaultSubscription;

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

            var registrationData = new CreateAccountViewModel
                {
                    Subscription = subscription,
                };

            return this.View(registrationData);
        }

        [HttpPost]
        public ActionResult CreateAccount(CreateAccountViewModel registrationData)
        {
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

            if (this.ModelState.Remove(e => e.ErrorMessage.Contains("requerido")))
                this.ModelState.AddModelError("MultipleItems", "É necessário preencher todos os campos.");

            // Normalizing name properties.
            if (!string.IsNullOrEmpty(registrationData.PracticeName))
                registrationData.PracticeName = Regex.Replace(registrationData.PracticeName, @"\s+", " ").Trim();

            if (!string.IsNullOrEmpty(registrationData.FullName))
                registrationData.FullName = Regex.Replace(registrationData.FullName, @"\s+", " ").Trim();

            var urlPracticeId = StringHelper.GenerateUrlIdentifier(registrationData.PracticeName);

            var subscription = StringHelper.FirstNonEmpty(registrationData.Subscription, defaultSubscription);
            if (!validSubscriptions.Contains(subscription) || registrationData.AsTrial == true)
                subscription = defaultSubscription;

            // Note: Url identifier for the name of the user, don't need any verification.
            // The name of the user must be unique inside a practice, not the entire database.

            var alreadyLoggedUser = this.User as AuthenticatedPrincipal;

            Practice practiceToReuse = null;
            if (alreadyLoggedUser != null)
            {
                practiceToReuse = this.db.Practices
                    .SingleOrDefault(p => p.UrlIdentifier == alreadyLoggedUser.Profile.PracticeIdentifier
                        && p.AccountContract.IsPartialBillingInfo);
            }

            var alreadyExistingPractice = this.db.Practices.SingleOrDefault(p => p.UrlIdentifier == urlPracticeId);
            if (alreadyExistingPractice != null)
            {
                if (alreadyExistingPractice.AccountContract != null && alreadyExistingPractice.AccountContract.IsPartialBillingInfo)
                    practiceToReuse = practiceToReuse ?? alreadyExistingPractice;
                else
                {
                    // killing practice that already exists if it expires, and freeing the name for a new practice
                    if (alreadyExistingPractice.AccountExpiryDate < this.GetUtcNow())
                    {
                        alreadyExistingPractice.AccountDisabled = true;
                        alreadyExistingPractice.UrlIdentifier += " !expired"; // change this, so that a new practice with this name can be created.
                        this.db.SaveChanges();
                    }
                    else
                    {
                        this.ModelState.AddModelError(
                            () => registrationData.PracticeName,
                            "Nome de consultório já está em uso.");
                    }
                }
            }

            var utcNow = this.GetUtcNow();

            // Creating the new user.
            User user;
            if (practiceToReuse == null)
            {
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
            }
            else
            {
                user = this.db.Users.SingleOrDefault(u => u.Id == alreadyLoggedUser.Profile.Id && u.PracticeId == practiceToReuse.Id);
                SecurityManager.UpdateUser(user, registrationData, this.db.Users, utcNow);
            }

            if (user != null)
            {
                string timeZoneId = null;
                if (registrationData.PracticeProvince != null)
                    timeZoneId = TimeZoneDataAttribute.GetAttributeFromEnumValue((TypeEstadoBrasileiro)registrationData.PracticeProvince.Value).Id;

                user.Practice = practiceToReuse ?? new Practice();
                user.Practice.Name = registrationData.PracticeName;
                user.Practice.UrlIdentifier = urlPracticeId;
                user.Practice.CreatedOn = utcNow;
                user.Practice.WindowsTimeZoneId = timeZoneId;
                user.Practice.Province = registrationData.PracticeProvince;
                user.Practice.PhoneMain = registrationData.PracticePhone;

                // Setting the BirthDate of the user as a person.
                user.Person.DateOfBirth = PracticeController.ConvertToUtcDateTime(user.Practice, registrationData.DateOfBirth ?? new DateTime());

                user.IsOwner = true;

                if (user.Administrator == null)
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
                else
                {
                    // todo: create a program that clears all orphaned Doctor objects
                    user.Doctor = null;
                }

                if (practiceToReuse == null)
                    this.db.Users.AddObject(user);

                if (this.ModelState.IsValid)
                {
                    MailMessage emailMessageToUser = null;
                    if (subscription == "trial" || subscription == "free")
                    {
                        // Creating confirmation email, with a token.
                        emailMessageToUser = this.EmailMessageToUser(user, utcNow, subscription == "trial");

                        // Sending e-mail to tell us the good news.
                        this.SendAccountCreatedSelfEmail(registrationData, user);
                    }

                    // If the ModelState is still valid, then save objects to the database,
                    // and send confirmation email message to the user.
                    using (emailMessageToUser)
                    {
                        // Saving changes to the DB.
                        this.db.SaveChanges();

                        if (subscription == "trial")
                        {
                            // Creating a new trial account contract.
                            var contract = user.Practice.AccountContract ?? new AccountContract();
                            contract.Practice = user.Practice;

                            contract.ContractTypeId = (int)ContractTypes.TrialContract;
                            contract.IsTrial = true;
                            contract.IssuanceDate = utcNow;
                            contract.StartDate = utcNow;
                            contract.EndDate = null; // indeterminated
                            contract.CustomText = null;

                            contract.DoctorsLimit = null;
                            contract.PatientsLimit = 50; // fixed limit for trial account

                            // no billings
                            contract.BillingAmount = null;
                            contract.BillingDueDay = null;
                            contract.BillingPaymentMethod = null;
                            contract.BillingPeriodCount = null;
                            contract.BillingPeriodSize = null;
                            contract.BillingPeriodType = null;
                            contract.BillingDiscountAmount = null;

                            user.Practice.AccountExpiryDate = utcNow.AddHours(Constants.MAX_HOURS_TO_VERIFY_TRIAL_ACCOUNT);
                            user.Practice.AccountContract = contract;

                            if (practiceToReuse == null)
                                this.db.AccountContracts.AddObject(contract);
                        }
                        else if (subscription == "free")
                        {
                            // Creating a new free account contract.
                            var contract = user.Practice.AccountContract ?? new AccountContract();
                            contract.Practice = user.Practice;

                            contract.ContractTypeId = (int)ContractTypes.FreeContract;
                            contract.IsTrial = false;
                            contract.IssuanceDate = utcNow;
                            contract.StartDate = utcNow;
                            contract.EndDate = null; // indeterminated
                            contract.CustomText = null;

                            contract.DoctorsLimit = 1;
                            contract.PatientsLimit = null; // fixed limit for trial account

                            // no billings
                            contract.BillingAmount = null;
                            contract.BillingDueDay = null;
                            contract.BillingPaymentMethod = null;
                            contract.BillingPeriodCount = null;
                            contract.BillingPeriodSize = null;
                            contract.BillingPeriodType = null;
                            contract.BillingDiscountAmount = null;

                            user.Practice.AccountExpiryDate = utcNow.AddHours(Constants.MAX_HOURS_TO_VERIFY_TRIAL_ACCOUNT);
                            user.Practice.AccountContract = contract;

                            if (practiceToReuse == null)
                                this.db.AccountContracts.AddObject(contract);
                        }
                        else if (subscription == "unlimited")
                        {
                            // Creating a new unlimited account contract.
                            var contract = user.Practice.AccountContract ?? new AccountContract();
                            contract.Practice = user.Practice;

                            contract.ContractTypeId = (int)ContractTypes.UnlimitedContract;
                            contract.IsTrial = false;
                            contract.IssuanceDate = utcNow;
                            contract.StartDate = utcNow;
                            contract.EndDate = null; // indeterminated
                            contract.CustomText = null;

                            contract.DoctorsLimit = null;
                            contract.PatientsLimit = null;

                            // billings data can be redefined when the user fills payment info
                            // for now these are the default values
                            contract.IsPartialBillingInfo = true; // indicates that the billing info for this contract must be defined by the user
                            contract.BillingAmount = Bus.Pro.UNLIMITED_PRICE;
                            contract.BillingDueDay = null; // payment method has no default (will be defined in the payment-info step)
                            contract.BillingPaymentMethod = null; // payment method has no default (will be defined in the payment-info step)
                            contract.BillingPeriodCount = null;
                            contract.BillingPeriodSize = 12; // unlimited accounts have always annual billings
                            contract.BillingPeriodType = "M";
                            contract.BillingDiscountAmount = (Bus.Pro.PRICE_YEAR * 12) - Bus.Pro.UNLIMITED_PRICE;

                            user.Practice.AccountExpiryDate = utcNow.AddHours(Constants.MAX_HOURS_TO_VERIFY_TRIAL_ACCOUNT);
                            user.Practice.AccountContract = contract;

                            if (practiceToReuse == null)
                                this.db.AccountContracts.AddObject(contract);
                        }
                        else
                        {
                            // Creating a new account contract, getting info from the subscription string.
                            var dicData = new Dictionary<string, dynamic>(StringComparer.InvariantCultureIgnoreCase)
                                {
                                    { "1M", new { Price = Bus.Pro.PRICE_MONTH, PeriodSize = 1 } },
                                    { "3M", new { Price = Bus.Pro.PRICE_QUARTER, PeriodSize = 3 } },
                                    { "6M", new { Price = Bus.Pro.PRICE_SEMESTER, PeriodSize = 6 } },
                                    { "12M", new { Price = Bus.Pro.PRICE_YEAR, PeriodSize = 12 } }
                                };

                            var data = dicData[subscription];

                            var contract = user.Practice.AccountContract ?? new AccountContract();
                            contract.Practice = user.Practice;

                            contract.ContractTypeId = (int)ContractTypes.ProfessionalContract;
                            contract.IsTrial = false;
                            contract.IssuanceDate = utcNow;
                            contract.StartDate = null; // inderterminated (will be defined when user pays)
                            contract.EndDate = null; // indeterminated
                            contract.CustomText = null;

                            contract.DoctorsLimit = null; // unknown at this moment (will be defined after user fills payment info)
                            contract.PatientsLimit = null; // fixed limit for trial account

                            // billings data can be redefined when the user fills payment info
                            // for now these are the default values
                            contract.IsPartialBillingInfo = true; // indicates that the billing info for this contract must be defined by the user
                            contract.BillingAmount = Bus.Pro.PRICE_MONTH * (int)data.PeriodSize;
                            contract.BillingDueDay = null; // payment method has no default (will be defined in the payment-info step)
                            contract.BillingPaymentMethod = null; // payment method has no default (will be defined in the payment-info step)
                            contract.BillingPeriodCount = null;
                            contract.BillingPeriodSize = data.PeriodSize;
                            contract.BillingPeriodType = "M";
                            contract.BillingDiscountAmount = (Bus.Pro.PRICE_MONTH * (int)data.PeriodSize) - data.Price;

                            user.Practice.AccountExpiryDate = utcNow + Constants.MaxTimeToVerifyProfessionalAccount;
                            user.Practice.AccountContract = contract;

                            if (practiceToReuse == null)
                                this.db.AccountContracts.AddObject(contract);
                        }

                        this.db.SaveChanges();

                        // if the new user is a doctor, create some other useful things
                        // like some medical-certificates and a default health-insurance
                        if (isNewDoctor)
                            BusHelper.FillNewDoctorUtilityBelt(user.Doctor);

                        if (practiceToReuse == null)
                        {
                            // adding message to the user so that he/she completes his/her profile informations
                            // todo: add complete profile notification
                            // If practice is being reused then notification was already sent.
                            var notificationData = new CompletePracticeInfoNotificationData();
                            var notificationDataString = new JavaScriptSerializer().Serialize(notificationData);
                            var dbNotification = new Notification
                                {
                                    CreatedOn = this.GetUtcNow(),
                                    PracticeId = user.PracticeId,
                                    Data = notificationDataString,
                                    UserToId = user.Id,
                                    Type = NotificationConstants.COMPLETE_INFO_NOTIFICATION_TYPE,
                                };
                            this.db.Notifications.AddObject(dbNotification);
                            NotificationsHub.BroadcastDbNotification(dbNotification, notificationData);
                        }

                        if (practiceToReuse == null)
                        {
                            // If practice is being reused then these values were already set.
                            user.Practice.Owner = user;
                            user.Person.PracticeId = user.PracticeId;
                            user.Administrator.PracticeId = user.PracticeId;
                            if (user.Doctor != null)
                                user.Doctor.PracticeId = user.PracticeId;
                        }

                        this.db.SaveChanges();

                        // Sending the confirmation e-mail to the new user.
                        // This must be synchronous.
                        // If practice is being reused then an email was already sent.
                        if (practiceToReuse == null && emailMessageToUser != null)
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

                        if (subscription == "trial" || subscription == "free")
                            return this.RedirectToAction("CreateAccountCompleted", new { practice = user.Practice.UrlIdentifier });
                        else
                            return this.RedirectToAction("SetAccountPaymentInfo", new { practice = user.Practice.UrlIdentifier });
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

        private MailMessage EmailMessageToUser(User user, DateTime utcNow, bool isTrial)
        {
            TokenId tokenId;

            // Setting verification token.
            using (var db2 = this.CreateNewCerebelloEntities())
            {
                var token = new GLB_Token();
                token.Value = Guid.NewGuid().ToString("N");
                token.Type = "VerifyPracticeAndEmail";
                token.Name = string.Format("Practice={0}&UserName={1}", user.Practice.UrlIdentifier, user.UserName);
                token.ExpirationDate = utcNow.AddHours(Constants.MAX_HOURS_TO_VERIFY_TRIAL_ACCOUNT);
                db2.GLB_Token.AddObject(token);
                db2.SaveChanges();

                tokenId = new TokenId(token.Id, token.Value);
            }

            // Rendering message bodies from partial view.
            var emailViewModel = new UserEmailViewModel(user) { Token = tokenId.ToString(), IsTrial = isTrial };
            var toAddress = new MailAddress(user.Person.Email, user.Person.FullName);
            var emailMessageToUser = this.CreateEmailMessage("ConfirmationEmail", toAddress, emailViewModel);

            return emailMessageToUser;
        }

        private void SendAccountCreatedSelfEmail(CreateAccountViewModel registrationData, User user)
        {
            // sending e-mail to cerebello@cerebello.com.br
            // to tell us the good news
            // lots of try catch... this is an internal thing, and should never fail to the client, even if it fails
            try
            {
                var emailViewModel2 = new InternalCreateAccountEmailViewModel(user, registrationData);
                var toAddress2 = new MailAddress("cerebello@cerebello.com.br", registrationData.FullName);
                var mailMessage2 = this.CreateEmailMessagePartial("InternalCreateAccountEmail", toAddress2, emailViewModel2);
                this.SendEmailAsync(mailMessage2).ContinueWith(
                    t =>
                    {
                        // send e-mail again is not an option, SendEmailAsync already tries a lot of times
                        // observing exception so that it is not raised
                        var ex = t.Exception;
                        Trace.TraceError(
                            string.Format(
                                "AuthenticationController.CreateAccount(CreateAccountViewModel): exception when sending internal e-mail: {0}",
                                TraceHelper.GetExceptionMessage(ex)));
                    });
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    string.Format(
                        "AuthenticationController.CreateAccount(CreateAccountViewModel): exception when sending internal e-mail: {0}",
                        TraceHelper.GetExceptionMessage(ex)));
            }
        }

        public ActionResult CreateAccountCompleted(string practice, bool? mustValidateEmail)
        {
            this.ViewBag.MustValidateEmail = mustValidateEmail;
            return this.View(new VerifyPracticeAndEmailViewModel { PracticeIdentifier = practice });
        }
        #endregion

        #region SetAccountPaymentInfo
        /// <summary>
        /// Sets the account payment info, to something of the user choice.
        /// At this moment this can only be called when the account contract has the flag IsPartialBillingInfo set.
        /// </summary>
        /// <param name="practice">The practice url identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [AuthorizeAndValidate]
        public ActionResult SetAccountPaymentInfo()
        {
            var contract = dbPractice.AccountContract;
            var periodTypesDic = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
                {
                    { "1M", "MONTH" },
                    { "3M", "3-MONTHS" },
                    { "6M", "6-MONTHS" },
                    { "12M", "12-MONTHS" },
                };
            string paymentModelName;
            periodTypesDic.TryGetValue(string.Format("{0}{1}", contract.BillingPeriodSize, contract.BillingPeriodType), out paymentModelName);

            var viewModel = new ChangeContractViewModel
                {
                    ContractUrlId = "ProfessionalPlan",
                    CurrentDoctorsCount = dbPractice.Users.Count(x => x.DoctorId != null),
                    DoctorCount = 1,
                    InvoceDueDayOfMonth = PracticeController.ConvertToLocalDateTime(dbPractice, this.GetUtcNow()).Day,
                    PaymentModelName = paymentModelName,
                };

            return this.View(viewModel);
        }

        [HttpPost]
        [AuthorizeAndValidate]
        public ActionResult SetAccountPaymentInfo(ChangeContractViewModel viewModel)
        {
            var mainContract = dbPractice.AccountContract;

            string planId;
            Bus.ContractToPlan.TryGetValue(dbPractice.AccountContract.SYS_ContractType.UrlIdentifier, out planId);

            // because each plan has a different set of features,
            // this is going to be used by the view, to define the partial page that will be shown
            this.ViewBag.CurrentContractName = mainContract.SYS_ContractType.UrlIdentifier;

            if (!viewModel.AcceptedByUser)
            {
                this.ModelState.AddModelError(
                    () => viewModel.AcceptedByUser, "A caixa de checagem de aceitação do contrato precisa ser marcada para concluir o processo.");
            }

            if (mainContract.IsPartialBillingInfo)
            {
                if (planId == "ProfessionalPlan")
                {
                    // calculating values to see if the submited values are correct
                    var unitPrice = Bus.Pro.DOCTOR_PRICE;
                    Func<double, double, double> integ = (x, n) => Math.Pow(n, x) / Math.Log(n);
                    Func<double, double, double> integ0to = (x, n) => (integ(x, n) - integ(0, n));
                    Func<double, double> priceFactor = x => x * (1.0 - 0.1 * integ0to((x - 1) / 3.0, 0.75));
                    Func<double, double> price = (extraDoctors) => Math.Round(priceFactor(extraDoctors) * unitPrice * 100) / 100;

                    var dicValues = new Dictionary<string, decimal>(StringComparer.InvariantCultureIgnoreCase)
                        {
                            { "MONTH", Bus.Pro.PRICE_MONTH },
                            { "3-MONTHS", Bus.Pro.PRICE_QUARTER },
                            { "6-MONTHS", Bus.Pro.PRICE_SEMESTER },
                            { "12-MONTHS", Bus.Pro.PRICE_YEAR },
                        };

                    var dicDiscount = new Dictionary<string, decimal>(StringComparer.InvariantCultureIgnoreCase)
                        {
                            { "MONTH", 0 },
                            { "3-MONTHS", Bus.Pro. DISCOUNT_QUARTER },
                            { "6-MONTHS", Bus.Pro. DISCOUNT_SEMESTER },
                            { "12-MONTHS", Bus.Pro.DISCOUNT_YEAR },
                        };

                    var periodSizesDic = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase)
                        {
                            { "MONTH", 1 },
                            { "3-MONTHS", 3 },
                            { "6-MONTHS", 6 },
                            { "12-MONTHS", 12 },
                        };

                    var dicount = 1m - dicDiscount[viewModel.PaymentModelName] / 100m;
                    var accountValue = dicValues[viewModel.PaymentModelName];
                    var doctorsValueWithoutDiscount = (decimal)Math.Round(price(viewModel.DoctorCount - 1))
                        * periodSizesDic[viewModel.PaymentModelName];

                    var finalValue = accountValue + doctorsValueWithoutDiscount * dicount;

                    var finalValueWithoutDiscount =
                        Bus.Pro.PRICE_MONTH * periodSizesDic[viewModel.PaymentModelName]
                        + doctorsValueWithoutDiscount;

                    // tolerance of R$ 0.10 in the final value... maybe the browser could not make the calculations correctly,
                    // but we must use that value, since it is the value that the user saw
                    if (Math.Abs(finalValue - viewModel.FinalValue) >= 0.10m)
                    {
                        this.ModelState.AddModelError(
                            () => viewModel.FinalValue,
                            "Seu browser apresentou um defeito no cálculo do valor final. Não foi possível processar sua requisição de upgrade.");
                    }

                    viewModel.ContractUrlId = planId;
                    viewModel.CurrentDoctorsCount = dbPractice.Users.Count(x => x.DoctorId != null);

                    if (this.ModelState.IsValid)
                    {
                        //// sending e-mail to cerebello@cerebello.com.br
                        //// to remember us to send the payment request
                        //var emailViewModel = new InternalUpgradeEmailViewModel(this.DbUser, viewModel);
                        //var toAddress = new MailAddress("cerebello@cerebello.com.br", this.DbUser.Person.FullName);
                        //var mailMessage = this.CreateEmailMessagePartial("InternalUpgradeEmail", toAddress, emailViewModel);
                        //this.SendEmailAsync(mailMessage).ContinueWith(t =>
                        //{
                        //    // observing exception so that it is not raised
                        //    var ex = t.Exception;

                        //    // todo: should do something when e-mail is not sent
                        //    // 1) use a schedule table to save a serialized e-mail, and then send it later
                        //    // 2) log a warning message somewhere stating that this e-mail was not sent
                        //    // send e-mail again is not an option, SendEmailAsync already tries a lot of times
                        //});

                        // changing the partial contract, to match the new billing settings
                        // Note: the contract will still be partial, and only when the user pays
                        //     the partial flag will be removed and the StartDate will be defined.
                        mainContract.CustomText = viewModel.WholeUserAgreement;
                        mainContract.DoctorsLimit = viewModel.DoctorCount;

                        mainContract.BillingAmount = finalValueWithoutDiscount;
                        mainContract.BillingDiscountAmount = finalValueWithoutDiscount - viewModel.FinalValue;
                        mainContract.BillingDueDay = viewModel.InvoceDueDayOfMonth;
                        mainContract.BillingPeriodCount = null;
                        mainContract.BillingPeriodSize = periodSizesDic[viewModel.PaymentModelName];
                        mainContract.BillingPeriodType = "M";
                        mainContract.BillingPaymentMethod = "PayPal Invoice";

                        this.db.SaveChanges();

                        // Creating the first billing
                        var utcNow = this.GetUtcNow();
                        var localNow = PracticeController.ConvertToLocalDateTime(dbPractice, utcNow);

                        Billing billing = null;
                        var idSet = string.Format(
                            "CEREB.{1}{2}.{0}",
                            localNow.Year,
                            mainContract.BillingPeriodSize,
                            mainContract.BillingPeriodType);

                        billing = db.Billings.SingleOrDefault(b => b.PracticeId == dbPractice.Id
                            && b.MainAccountContractId == dbPractice.ActiveAccountContractId
                            && b.ReferenceDate == null);

                        if (billing == null)
                        {
                            billing = new Billing
                            {
                                PracticeId = dbPractice.Id,
                                AfterDueMonthlyTax = 1.00m, // 1%
                                AfterDueTax = 2.00m, // 2%
                                IssuanceDate = utcNow,
                                MainAmount = (decimal)mainContract.BillingAmount,
                                MainDiscount = (decimal)mainContract.BillingDiscountAmount,
                                DueDate = PracticeController.ConvertToUtcDateTime(dbPractice, localNow.AddDays(10)),
                                IdentitySetName = idSet,
                                IdentitySetNumber = db.Billings.Count(b => b.PracticeId == dbPractice.Id && b.IdentitySetName == idSet) + 1,
                                ReferenceDate = PracticeController.ConvertToUtcDateTime(dbPractice, null),
                                ReferenceDateEnd = PracticeController.ConvertToUtcDateTime(dbPractice, null),
                                MainAccountContractId = dbPractice.ActiveAccountContractId.Value,
                            };

                            db.Billings.AddObject(billing);
                        }

                        this.db.SaveChanges();

                        // Using PayPal API to start an Express Checkout operation, and then redirecting user to PayPal.
                        var operation = new PayPalSetExpressCheckoutOperation();
                        ConfigAccountController.FillOperationDetails(operation, dbPractice, mainContract, billing);
                        var practice = this.dbPractice.UrlIdentifier;
                        operation.PaymentRequests[0].NotifyUrl = UseExternalIpIfDebug(this.Url.ActionAbsolute("PayPalNotification", new { practice }));
                        var opResult = this.SetExpressCheckout(operation, "PayPalConfirm", "PayPalCancel", new { practice });

                        if (opResult.Errors != null && opResult.Errors.Any())
                            return new StatusCodeResult(HttpStatusCode.InternalServerError, opResult.Errors[0].LongMessage);

                        return this.RedirectToCheckout(opResult);
                    }

                    return this.View(viewModel);
                }
            }

            return this.HttpNotFound();
        }

        [AuthorizeAndValidate]
        public ActionResult CancelPartialAccount()
        {
            // logging off
            FormsAuthentication.SignOut();

            // disabling account
            this.dbPractice.AccountDisabled = true;
            this.dbPractice.UrlIdentifier += " !partial-account-canceled"; // change this, so that a new practice with this name can be created.
            this.db.SaveChanges();

            return this.RedirectToAction("AccountNotCreated");
        }

        public ActionResult AccountNotCreated()
        {
            return this.View();
        }
        #endregion

        #region PayPal: PayPalConfirm, PayPalCancel, PaymentCanceled, PayPalNotification (IPN)
        /// <summary>
        /// Action that termitates the payment process, and then redirects to PaymentConfirmed action.
        /// </summary>
        /// <param name="practice"> </param>
        /// <param name="data"></param>
        /// <returns></returns>
        [AuthorizeAndValidate]
        public ActionResult PayPalConfirm(string practice, PayPalExpressCheckoutConfirmation data)
        {
            var mainContract = dbPractice.AccountContract;

            var billing = db.Billings.Single(b => b.PracticeId == dbPractice.Id
                && b.MainAccountContractId == dbPractice.ActiveAccountContractId
                && b.ReferenceDate == null);

            var utcNow = this.GetUtcNow();

            dbPractice.VerificationDate = utcNow;
            dbPractice.VerificationMethod = "PAYMENT";
            mainContract.StartDate = utcNow;
            mainContract.IsPartialBillingInfo = false;
            billing.ReferenceDate = utcNow;
            if (mainContract.BillingPeriodType == "M" && mainContract.BillingPeriodSize.HasValue)
                billing.ReferenceDateEnd = utcNow.AddMonths(mainContract.BillingPeriodSize.Value);
            else
                throw new NotImplementedException();

            this.db.SaveChanges();

            var operation = new PayPalDoExpressCheckoutPaymentOperation
            {
                PayerId = data.PayerId,
                Token = data.Token,
            };
            ConfigAccountController.FillOperationDetails(operation, dbPractice, mainContract, billing);

            this.DoExpressCheckoutPayment(operation);

            return this.RedirectToAction("PayPalConfirmed", new { practice });
        }

        [AuthorizeAndValidate(false)]
        public ActionResult PayPalConfirmed(string practice)
        {
            var utcNow = this.GetUtcNow();

            // Creating confirmation email, with a token.
            using (var emailMessageToUser = this.EmailMessageToUser(this.dbUser, utcNow, isTrial: false))
            {
                var createAccountViewModel = new CreateAccountViewModel
                    {
                        DateOfBirth = this.dbUser.Person.DateOfBirth,
                        PracticeName = this.dbPractice.Name,
                        PracticePhone = this.dbPractice.PhoneMain,
                        Password = "",
                        Subscription =
                            string.Format(
                                "{0}{1}", this.dbPractice.AccountContract.BillingPeriodSize, this.dbPractice.AccountContract.BillingPeriodType),
                        UserName = this.dbUser.UserName,
                        ConfirmPassword = "",
                        EMail = this.dbUser.Person.Email,
                        AsTrial = null,
                        FullName = this.dbUser.Person.FullName,
                        Gender = this.dbUser.Person.Gender,
                        IsDoctor = this.dbUser.DoctorId.HasValue,
                        PracticeProvince = this.dbPractice.Province,
                    };

                var doctor = this.dbUser.Doctor;
                if (doctor != null)
                {
                    var medicalEntity = UsersController.GetDoctorEntity(this.db.SYS_MedicalEntity, doctor);
                    var medicalSpecialty = UsersController.GetDoctorSpecialty(this.db.SYS_MedicalSpecialty, doctor);

                    // Getting all patients data.
                    var viewModel = new UserViewModel();
                    UsersController.FillDoctorViewModel(this.dbUser, medicalEntity, medicalSpecialty, viewModel, doctor);

                    createAccountViewModel.MedicCRM = viewModel.MedicCRM;
                    createAccountViewModel.MedicalEntityId = viewModel.MedicalEntityId;
                    createAccountViewModel.MedicalEntityJurisdiction = viewModel.MedicalEntityJurisdiction;
                    createAccountViewModel.MedicalSpecialtyId = viewModel.MedicalSpecialtyId;
                    createAccountViewModel.MedicalSpecialtyName = viewModel.MedicalSpecialtyName;
                }

                this.SendAccountCreatedSelfEmail(createAccountViewModel, this.dbUser);

                if (emailMessageToUser != null)
                    this.TrySendEmail(emailMessageToUser);
            }

            return this.RedirectToAction("Welcome", "Home", new { practice });
        }

        /// <summary>
        /// Action that cancels the payment process, and then redirects to PaymentCanceled action.
        /// </summary>
        /// <returns></returns>
        [AuthorizeAndValidate]
        public ActionResult PayPalCancel(string practice)
        {
            return this.CancelPartialAccount();
        }

        public static string UseExternalIpIfDebug(string url)
        {
            if (DebugConfig.IsDebug)
            {
                var uri = new Uri(url);
                if (uri.IsLoopback)
                {
                    var uriBuilder = new UriBuilder(url);
                    uriBuilder.Host = GetExternalIp();
                    return uriBuilder.ToString();
                }
            }

            return url;
        }

        public ActionResult PayPalNotification()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Debug

        public static string GetExternalIp()
        {
            var wr = WebRequest.Create("http://www.dmiws.com/myip/");
            wr.Method = "GET";
            string str = "";
            using (var r = wr.GetResponse())
            using (var s = r.GetResponseStream())
            {
                if (s != null)
                    using (var sr = new StreamReader(s))
                        str = sr.ReadToEnd();
            }

            var m = Regex.Match(str, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");

            return m.Value;
        }

        public ActionResult Ip()
        {
            return this.Content(System.Web.HttpContext.Current.Request.UserHostAddress);
        }

        #endregion

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
                    practice.VerificationMethod = "EMAIL";
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

            if (this.ModelState.IsValid && user != null && practice != null)
            {
                return this.RedirectToAction(
                    "Welcome",
                    "Home",
                    new { area = "", practice = practice.UrlIdentifier });
            }

            viewModel.Password = null; // cannot allow password going to the view.
            return this.View(viewModel);
        }

        #region ResetPassword: Request
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
            {
                this.ModelState.AddModelError(
                    () => viewModel.PracticeIdentifier,
                    "Não é possível resetar a senha pois o consultório ainda não foi verificado. "
                    + "Confirme o seu e-mail antes de tentar mudar a senha.");
            }

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
                        token.ExpirationDate = utcNow.AddDays(Constants.MAX_DAYS_TO_RESET_PASSWORD);
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

        class AuthorizeAndValidateAttribute : AuthorizeAttribute
        {
            private readonly bool mustBePartialContract;

            public AuthorizeAndValidateAttribute(bool mustBePartialContract = true)
            {
                this.mustBePartialContract = mustBePartialContract;
            }

            public override void OnAuthorization(AuthorizationContext filterContext)
            {
                var controller = (AuthenticationController)filterContext.Controller;
                var user = (AuthenticatedPrincipal)controller.User;
                var practice = StringHelper.FirstNonEmpty(filterContext.HttpContext.Request.QueryString["practice"], user.Profile.PracticeIdentifier);
                if (practice != user.Profile.PracticeIdentifier)
                {
                    filterContext.Result = new StatusCodeResult(HttpStatusCode.NotFound);
                }
                else
                {
                    var dbPractice = controller.db.Practices.Include("Users").SingleOrDefault(p => p.UrlIdentifier == practice);

                    controller.dbPractice = dbPractice;

                    if (dbPractice != null)
                    {
                        controller.dbUser = dbPractice.Users.Single();

                        if (dbPractice.AccountContract == null
                            || (this.mustBePartialContract && !dbPractice.AccountContract.IsPartialBillingInfo)
                            || user.Profile.PracticeIdentifier != practice
                            || user.Profile.Id != controller.dbUser.Id)
                        {
                            filterContext.Result = new StatusCodeResult(HttpStatusCode.NotFound);
                        }
                    }
                }

                base.OnAuthorization(filterContext);
            }
        }
    }
}
