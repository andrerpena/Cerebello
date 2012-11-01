using System;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Security;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Controllers;
using CerebelloWebRole.Code.Helpers;
using CerebelloWebRole.Code.Security;
using CerebelloWebRole.Models;

namespace CerebelloWebRole.Areas.Site.Controllers
{
    public class AuthenticationController : RootController
    {
        private CerebelloEntities db = null;

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Requiriments:
        ///     - Should populate the practice identifier if it's present in the passed returnUrl
        /// </remarks>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            var viewModel = new LoginViewModel();

            if (!string.IsNullOrEmpty(returnUrl))
                try
                {
                    // extract practice name from returnUrl
                    var routeData = RouteHelper.GetRouteDataByUrl("~" + returnUrl);
                    if (routeData.Values.ContainsKey("practice"))
                        viewModel.PracticeIdentifier = (string)routeData.Values["practice"];
                }
                catch
                {
                    // the returnUrl must be invalid, let's just ignore it
                }

            return View(viewModel);
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            this.db = this.CreateNewCerebelloEntities();

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

            var cookieCollection = this.HttpContext.Response.Cookies;
            if (!this.ModelState.IsValid || !SecurityManager.Login(cookieCollection, loginModel, db.Users, out user))
            {
                ViewBag.LoginFailed = true;
                return View(loginModel);
            }

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
            var result = SecurityManager.CreateUser(out user, registrationData, db.Users, utcNow, null);

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
                var timeZoneId = TimeZoneDataAttribute.GetAttributeFromEnumValue((TypeTimeZone)registrationData.PracticeTimeZone).Id;

                // Creating a new medical practice.
                user.Practice = new Practice
                {
                    Name = registrationData.PracticeName,
                    UrlIdentifier = urlPracticeId,
                    CreatedOn = utcNow,
                    WindowsTimeZoneId = timeZoneId,
                    ShowWelcomeScreen = true,
                };

                // Setting the BirthDate of the user as a person.
                user.Person.DateOfBirth = PracticeController.ConvertToUtcDateTime(user.Practice, registrationData.DateOfBirth);

                user.IsOwner = true;

                // when the user is a doctor, we need to fill the properties of the doctor
                if (registrationData.IsDoctor)
                {
                    // if user is already a doctor, we just edit the properties
                    // otherwise we create a new doctor instance
                    if (user.Doctor == null)
                        user.Doctor = db.Doctors.CreateObject();

                    var ms = this.db.SYS_MedicalSpecialty
                        .Single(ms1 => ms1.Id == registrationData.MedicalSpecialtyId);

                    var me = this.db.SYS_MedicalEntity
                        .Single(me1 => me1.Id == registrationData.MedicalEntityId);

                    user.Doctor.CRM = registrationData.MedicCRM;
                    user.Doctor.MedicalSpecialtyCode = ms.Code;
                    user.Doctor.MedicalSpecialtyName = ms.Name;
                    user.Doctor.MedicalEntityCode = me.Code;
                    user.Doctor.MedicalEntityName = me.Name;
                    user.Doctor.MedicalEntityJurisdiction = registrationData.MedicalEntityJurisdiction.ToString();

                    // Creating an unique UrlIdentifier for this doctor.
                    // This is the first doctor, so there will be no conflicts.
                    string urlId = UsersController.GetUniqueDoctorUrlId(this.db.Doctors, registrationData.FullName, null);
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

                // Creating confirmation email, with the token.
                MailMessage message;

                {
                    TokenId tokenId;

                    // Setting verification token.
                    // Note: tokens are safe to save even if validation fails.
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
                    var partialViewModel = new EmailViewModel
                    {
                        PersonName = user.Person.FullName,
                        UserName = user.UserName,
                        Token = tokenId.ToString(),
                        PracticeUrlIdentifier = user.Practice.UrlIdentifier,
                    };
                    var bodyText = this.RenderPartialViewToString("ConfirmationEmail", partialViewModel);

                    partialViewModel.IsBodyHtml = true;
                    var bodyHtml = this.RenderPartialViewToString("ConfirmationEmail", partialViewModel);

                    var toAddress = new MailAddress(user.Person.Email, user.Person.FullName);

                    message = this.CreateEmailMessage(
                        toAddress,
                        "Bem vindo ao Cerebello! Por favor, confirme a criação de sua conta.",
                        bodyHtml,
                        bodyText);
                }

                // If the ModelState is still valid, then save objects to the database,
                // and send confirmation email message to the user.
                using (message)
                {
                    if (this.ModelState.IsValid)
                    {
                        // Saving changes to the DB.
                        db.SaveChanges();

                        user.Practice.Owner = user;
                        user.Person.PracticeId = user.PracticeId;
                        db.SaveChanges();

                        // Sending the confirmation e-mail to the new user.
                        this.SendEmail(message);

                        // Log the user in.
                        var loginModel = new LoginViewModel
                        {
                            Password = registrationData.Password,
                            PracticeIdentifier = user.Practice.UrlIdentifier,
                            RememberMe = false,
                            UserNameOrEmail = registrationData.UserName,
                        };

                        if (!SecurityManager.Login(this.HttpContext.Response.Cookies, loginModel, db.Users, out user))
                        {
                            throw new Exception("Login cannot fail.");
                        }

                        return RedirectToAction("CreateAccountCompleted", new { practice = user.Practice.UrlIdentifier });
                    }
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

        public ActionResult CreateAccountCompleted(string practice)
        {
            return View(new VerifyPracticeAndEmailViewModel { Practice = practice });
        }

        #region ResetPassword: Request
        [AcceptVerbs(new[] { "Get", "Post" })]
        public ActionResult VerifyPracticeAndEmail(VerifyPracticeAndEmailViewModel viewModel, bool allowEditToken = false)
        {
            this.ViewBag.AllowEditToken = allowEditToken || !this.ModelState.IsValid;

            var utcNow = this.GetUtcNow();

            User user = null;

            // If user is not logged yet, we will use the userName and password to login.
            if (this.Request.IsAuthenticated)
            {
                var authenticatedPrincipal = this.User as AuthenticatedPrincipal;

                if (authenticatedPrincipal == null)
                    throw new Exception(
                        "HttpContext.User should be a AuthenticatedPrincipal when the user is authenticated");

                user = this.db.Users.FirstOrDefault(u => u.Id == authenticatedPrincipal.Profile.Id);
            }
            else
            {
                var loginModel = new LoginViewModel
                {
                    PracticeIdentifier = viewModel.Practice,
                    UserNameOrEmail = viewModel.UserNameOrEmail,
                    Password = viewModel.Password,
                };

                var cookieCollection = this.HttpContext.Response.Cookies;
                if (!this.ModelState.IsValid ||
                    !SecurityManager.Login(cookieCollection, loginModel, this.db.Users, out user))
                {
                    ViewBag.LoginFailed = true;
                }
                else
                {
                    user.LastActiveOn = this.GetUtcNow();

                    this.db.SaveChanges();

                    if (loginModel.Password == Constants.DEFAULT_PASSWORD)
                        throw new Exception("Cannot create initial user with a default password.");
                }
            }

            if (user == null)
                this.ModelState.AddModelError(() => viewModel.UserNameOrEmail, "Nome de usuário ou senha incorretos.");

            var isTokenValid = TokenId.IsValid(viewModel.Token);
            GLB_Token token = null;
            if (isTokenValid)
            {
                var tokenId = new TokenId(viewModel.Token);

                // Getting verification token, using the informations.
                var tokenName = string.Format("Practice={0}&UserName={1}", viewModel.Practice, user.UserName);
                token = this.db.GLB_Token.SingleOrDefault(tk =>
                                                              tk.Id == tokenId.Id
                                                              && tk.Value == tokenId.Value
                                                              && tk.Type == "VerifyPracticeAndEmail"
                                                              && tk.Name == tokenName);
            }

            if (token == null)
                isTokenValid = false;

            var practice = this.db.Practices
                .SingleOrDefault(p => p.UrlIdentifier == viewModel.Practice);

            if (practice == null)
                this.ModelState.AddModelError(() => viewModel.Practice, "Consultório não foi achado.");

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

            if (!isTokenValid)
            {
                this.ModelState.AddModelError(() => viewModel.Token, "Token é inválido, não existe, ou passou do prazo de validade.");
            }

            if (this.ModelState.IsValid)
            {
                return this.RedirectToAction("Welcome", "PracticeHome",
                                             new { area = "App", practice = practice.UrlIdentifier });
            }

            viewModel.Token = null;
            viewModel.Password = null; // cannot allow password going to the view.
            return View(viewModel);
        }

        public ActionResult ResetPasswordRequest()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult ResetPasswordRequest(ResetPasswordRequestViewModel viewModel)
        {
            var user = SecurityManager.GetUser(this.db.Users, viewModel.PracticeIdentifier, viewModel.UserNameOrEmail);

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
                    token.Name = string.Format("Practice={0}&UserName={1}", user.Practice.UrlIdentifier,
                                               user.UserName);
                    token.ExpirationDate = utcNow.AddDays(30);
                    db2.GLB_Token.AddObject(token);
                    db2.SaveChanges();

                    tokenId = new TokenId(token.Id, token.Value);
                }

                // Rendering message bodies from partial view.
                var partialViewModel = new EmailViewModel
                                           {
                                               PersonName = user.Person.FullName,
                                               UserName = user.UserName,
                                               Token = tokenId.ToString(),
                                               PracticeUrlIdentifier = user.Practice.UrlIdentifier,
                                           };
                var bodyText = this.RenderPartialViewToString("ResetPasswordEmail", partialViewModel);

                partialViewModel.IsBodyHtml = true;
                var bodyHtml = this.RenderPartialViewToString("ResetPasswordEmail", partialViewModel);

                var toAddress = new MailAddress(user.Person.Email, user.Person.FullName);

                message = this.CreateEmailMessage(
                    toAddress,
                    "Redefinir senha da conta no Cerebello",
                    bodyHtml,
                    bodyText);

                #endregion
            }
            else
            {
                return RedirectToAction("ResetPasswordManually");
            }

            // If the ModelState is still valid, then save objects to the database,
            // and send confirmation email message to the user.
            using (message)
            {
                if (this.ModelState.IsValid)
                {
                    // Sending the confirmation e-mail to the new user.
                    this.SendEmail(message);

                    return RedirectToAction("ResetPasswordEmailSent");
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
        public ActionResult ResetPassword(ResetPasswordViewModel viewModel, bool allowEditToken = false)
        {
            this.ViewBag.AllowEditToken = allowEditToken;

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

                if (token != null && this.ModelState.IsValid)
                {
                    SecurityManager.SetUserPassword(this.db.Users, viewModel.PracticeIdentifier, viewModel.UserNameOrEmail, viewModel.NewPassword);

                    this.db.GLB_Token.DeleteObject(token);

                    this.db.SaveChanges();

                    return this.RedirectToAction("ResetPasswordSuccess");
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

                this.db.GLB_Token.DeleteObject(token);

                this.db.SaveChanges();
            }

            return this.View();
        }

        public ActionResult ResetPasswordSuccess()
        {
            return this.View();
        }
        #endregion
    }
}
