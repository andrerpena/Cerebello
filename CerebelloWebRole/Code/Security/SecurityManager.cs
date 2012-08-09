using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Security.Principal;
using CerebelloWebRole.Code.Security;
using CerebelloWebRole.Code.Security.Principals;
using System.Linq;
using CerebelloWebRole.Models;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;

namespace CerebelloWebRole.Code
{
    public class SecurityManager
    {
        public static void Logout()
        {
            FormsAuthentication.SignOut();
        }

        public static string GetLoggedUserSecurityToken()
        {
            return ((FormsIdentity)HttpContext.Current.User.Identity).Ticket.UserData;
        }

        /// <summary>
        /// Creates a new user and adds it to the storage object context.
        /// </summary>
        /// <param name="registrationData">Object containing informations about the user to be created.</param>
        /// <param name="db">Storage object context used to add the new user. It won't be saved, just changed.</param>
        /// <returns>The new User object.</returns>
        public static CreateUserResult CreateUser(out User createdUser, CreateAccountViewModel registrationData, CerebelloEntities db, int? practiceId = null)
        {
            // Password salt and hash.
            string passwordSalt = CipherHelper.GenerateSalt();
            var passwordHash = CipherHelper.Hash(registrationData.Password, passwordSalt);

            // Normalizing user name.
            // The normalized user-name will be used to discover if another user with the same user-name already exists.
            // This is a security measure. This makes it very difficult to guess what a person's user name may be.
            // You can only login with the exact user name that you provided the first time,
            // but if someone tries to register a similar user name just to know if that one is the one you used...
            // the attacker won't be sure... because it could be any other variation.
            // e.g. I register user-name "Miguel.Angelo"... the attacker tries to register "miguelangelo", he'll be denied...
            // but that doesn't mean the exact user-name "miguelangelo" is the one I used, in fact it is not.
            var normalizedUserName = StringHelper.NormalizeUserName(registrationData.UserName);

            bool isUserNameAlreadyInUse =
                practiceId != null &&
                db.Users.Where(u => u.UserNameNormalized == normalizedUserName && u.PracticeId == practiceId).Any();

            if (isUserNameAlreadyInUse)
            {
                createdUser = null;
                return CreateUserResult.UserNameAlreadyInUse;
            }

            // Creating an unique UrlIdentifier for this user.
            // This does not consider UrlIdentifier's used by patients.
            var urlId = UsersController.GetUniqueUserUrlId(db, registrationData.FullName, practiceId);
            if (urlId == null)
            {
                createdUser = null;
                return CreateUserResult.CouldNotCreateUrlIdentifier;
            }

            // Creating user.
            createdUser = new User()
            {
                Person = new Person()
                {
                    DateOfBirth = registrationData.DateOfBirth,
                    Gender = registrationData.Gender,
                    FullName = registrationData.FullName,
                    UrlIdentifier = urlId,
                    CreatedOn = DateTime.Now,
                },
                UserName = registrationData.UserName,
                UserNameNormalized = normalizedUserName,
                PasswordSalt = passwordSalt,
                Password = passwordHash,
                LastActiveOn = DateTime.Now,
                Email = registrationData.EMail,
            };

            // E-mail is optional.
            if (!string.IsNullOrEmpty(registrationData.EMail))
                createdUser.Person.Emails.Add(new Email() { Address = registrationData.EMail });

            if (practiceId != null)
                createdUser.PracticeId = (int)practiceId;

            db.Users.AddObject(createdUser);

            return CreateUserResult.Ok;
        }

        public static bool Login(LoginViewModel loginModel, CerebelloEntities entities)
        {
            try
            {
                string securityToken = AuthenticateUser(loginModel.UserNameOrEmail, loginModel.Password, loginModel.PracticeIdentifier, entities);

                DateTime expiryDate = DateTime.UtcNow.AddYears(1);
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                     1, loginModel.UserNameOrEmail, DateTime.UtcNow, expiryDate, true,
                     securityToken, FormsAuthentication.FormsCookiePath);

                string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                cookie.Expires = expiryDate;

                HttpContext.Current.Response.Cookies.Add(cookie);

                return true;
            }
            catch (Exception ex)
            {
                // add log information about this exception
                FormsAuthentication.SignOut();
                return false;
            }
        }

        public static void SetPrincipal()
        {
            Principal principal = null;
            FormsIdentity identity;

            if (HttpContext.Current.Request.IsAuthenticated)
            {
                identity = (FormsIdentity)HttpContext.Current.User.Identity;

                UserData userProfile;
                try
                {
                    userProfile = SecurityTokenHelper.FromString(((FormsIdentity)identity).Ticket.UserData).UserData;
                    // UserHelper.UpdateLastActiveOn(userProfile);
                    principal = new AuthenticatedPrincipal(identity, userProfile);
                }
                catch
                {
                    //TODO: Log an exception
                    FormsAuthentication.SignOut();
                    principal = new AnonymousPrincipal(new GuestIdentity());
                }

            }
            else
                principal = new AnonymousPrincipal(new GuestIdentity());

            HttpContext.Current.User = principal;
        }

        /// <summary>
        /// Authenticates the given user and returns a string corresponding to his/her
        /// identity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static String AuthenticateUser(String userNameOrEmail, String password, string practiceIdentifier, CerebelloEntities entities)
        {
            var isEmail = userNameOrEmail.Contains("@");

            User user = isEmail ?
                entities.Users.Where(u => u.Email == userNameOrEmail && u.Practice.UrlIdentifier == practiceIdentifier).FirstOrDefault() :
                entities.Users.Where(u => u.UserName == userNameOrEmail && u.Practice.UrlIdentifier == practiceIdentifier).FirstOrDefault();

            if (user == null)
                throw new Exception("UserName/Email [" + userNameOrEmail + "] not found");

            // comparing password
            var passwordHash = CipherHelper.Hash(password, user.PasswordSalt);
            if (user.Password != passwordHash)
                throw new Exception("Password [" + password + "] is invalid");

            user.LastActiveOn = DateTime.UtcNow;

            SecurityToken securityToken = new SecurityToken()
            {
                Salt = new Random().Next(0, 2000),
                UserData = new UserData()
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.Person.FullName,
                    IsUsingDefaultPassword = password == CerebelloWebRole.Code.Constants.DEFAULT_PASSWORD,
                }
            };

            return SecurityTokenHelper.ToString(securityToken);
        }
    }
}