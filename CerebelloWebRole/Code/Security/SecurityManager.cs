using System;
using System.Web;
using System.Web.Security;
using CerebelloWebRole.Code.Security;
using CerebelloWebRole.Code.Security.Principals;
using System.Linq;
using CerebelloWebRole.Models;
using Cerebello.Model;

namespace CerebelloWebRole.Code
{
    public static class SecurityManager
    {
        /// <summary>
        /// Creates a new user and adds it to the storage object context.
        /// </summary>
        /// <param name="createdUser">Output paramater that returns the new user.</param>
        /// <param name="registrationData">Object containing informations about the user to be created.</param>
        /// <param name="db">Storage object context used to add the new user. It won't be saved, just changed.</param>
        /// <param name="utcNow"> </param>
        /// <param name="practiceId">The id of the practice that the new user belongs to.</param>
        /// <returns>An enumerated value indicating what has happened.</returns>
        public static CreateUserResult CreateUser(out User createdUser, CreateAccountViewModel registrationData, CerebelloEntities db, DateTime utcNow, int? practiceId)
        {
            // Password cannot be null, nor empty.
            if (string.IsNullOrEmpty(registrationData.Password))
            {
                createdUser = null;
                return CreateUserResult.InvalidUserNameOrPassword;
            }

            // User-name cannot be null, nor empty.
            if (string.IsNullOrEmpty(registrationData.UserName))
            {
                createdUser = null;
                return CreateUserResult.InvalidUserNameOrPassword;
            }

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
                db.Users.Any(u => u.UserNameNormalized == normalizedUserName && u.PracticeId == practiceId);

            if (isUserNameAlreadyInUse)
            {
                createdUser = null;
                return CreateUserResult.UserNameAlreadyInUse;
            }

            // Creating user.
            createdUser = new User()
            {
                Person = new Person()
                {
                    // Note: DateOfBirth property cannot be set in this method because of Utc/Local conversions.
                    // The caller of this method must set the property.
                    Gender = registrationData.Gender,
                    FullName = registrationData.FullName,
                    CreatedOn = utcNow,
                    Email = registrationData.EMail,
                    EmailGravatarHash = GravatarHelper.GetGravatarHash(registrationData.EMail)
                },
                UserName = registrationData.UserName,
                UserNameNormalized = normalizedUserName,
                PasswordSalt = passwordSalt,
                Password = passwordHash,
                LastActiveOn = utcNow,
            };

            if (practiceId != null)
                createdUser.PracticeId = (int)practiceId;

            db.Users.AddObject(createdUser);

            return CreateUserResult.Ok;
        }

        /// <summary>
        /// Logs an user in.
        /// </summary>
        /// <param name="cookieCollection">
        /// Cookie collection that is going to hold an encrypted cookie with informations about the user.
        /// </param>
        /// <param name="loginModel">
        /// Model containing login informations such as practice-name, user-name and password.
        /// </param>
        /// <param name="db">
        /// Database context used to get informations about the user.
        /// No data will be saved to the DB.
        /// </param>
        /// <param name="loggedInUser">
        /// Out parameter returning the database User object representing the logged in user, only if the
        /// login succeded. Otherwise null.
        /// </param>
        /// <returns>Returns whether the login succeded or not.</returns>
        public static bool Login(HttpCookieCollection cookieCollection, LoginViewModel loginModel, CerebelloEntities db, out User loggedInUser)
        {
            loggedInUser = null;

            try
            {
                var securityToken = AuthenticateUser(loginModel.UserNameOrEmail, loginModel.Password, loginModel.PracticeIdentifier, db, out loggedInUser);

                var expiryDate = DateTime.UtcNow.AddYears(1);
                var ticket = new FormsAuthenticationTicket(
                     1, loginModel.UserNameOrEmail, DateTime.UtcNow, expiryDate, true,
                     securityToken, FormsAuthentication.FormsCookiePath);

                var encryptedTicket = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket) { Expires = expiryDate };

                cookieCollection.Add(cookie);

                return true;
            }
            catch (Exception)
            {
                // add log information about this exception
                FormsAuthentication.SignOut();
                return false;
            }
        }

        public static void SetPrincipal(HttpContextBase httpContext)
        {
            Principal principal = null;

            if (httpContext.Request.IsAuthenticated)
            {
                var identity = (FormsIdentity)httpContext.User.Identity;

                try
                {
                    UserData userProfile = SecurityTokenHelper.FromString(((FormsIdentity)identity).Ticket.UserData).UserData;
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

            httpContext.User = principal;
        }

        /// <summary>
        /// Authenticates the given user and returns a string corresponding to his/her
        /// identity
        /// </summary>
        /// <param name="practiceIdentifier"> </param>
        /// <param name="db"></param>
        /// <param name="userNameOrEmail"> </param>
        /// <param name="password"> </param>
        /// <param name="loggedInUser"> </param>
        /// <returns></returns>
        public static String AuthenticateUser(String userNameOrEmail, String password, string practiceIdentifier, CerebelloEntities db, out User loggedInUser)
        {
            // Note: this method was setting the user.LastActiveOn property, but now the caller must do this.
            // This is because it is not allowed to use DateTime.Now, because this makes the value not mockable.

            loggedInUser = GetUser(db, practiceIdentifier, userNameOrEmail);

            if (loggedInUser == null)
                throw new Exception("UserName/Email [" + userNameOrEmail + "] not found");

            // comparing password
            var passwordHash = CipherHelper.Hash(password, loggedInUser.PasswordSalt);
            if (loggedInUser.Password != passwordHash)
                throw new Exception("Password [" + password + "] is invalid");

            var securityToken = new SecurityToken
            {
                Salt = new Random().Next(0, 2000),
                UserData = new UserData()
                {
                    Id = loggedInUser.Id,
                    Email = loggedInUser.Person.Email,
                    FullName = loggedInUser.Person.FullName,
                    IsUsingDefaultPassword = password == Constants.DEFAULT_PASSWORD,
                }
            };

            return SecurityTokenHelper.ToString(securityToken);
        }

        /// <summary>
        /// Gets an user given user name or email, and the practice identifier.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="practiceIdentifier"></param>
        /// <param name="userNameOrEmail"></param>
        /// <returns></returns>
        public static User GetUser(CerebelloEntities db, string practiceIdentifier, string userNameOrEmail)
        {
            var isEmail = userNameOrEmail.Contains("@");

            User loggedInUser = isEmail
                ? db.Users.FirstOrDefault(
                    u => u.Person.Email == userNameOrEmail && u.Practice.UrlIdentifier == practiceIdentifier)
                : db.Users.FirstOrDefault(
                    u => u.UserName == userNameOrEmail && u.Practice.UrlIdentifier == practiceIdentifier);

            return loggedInUser;
        }

        /// <summary>
        /// Sets the password of an user.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="practiceIdentifier"></param>
        /// <param name="userNameOrEmail"></param>
        /// <param name="password"></param>
        /// <param name="token"></param>
        public static void SetUserPassword(CerebelloEntities db, string practiceIdentifier, string userNameOrEmail, string password)
        {
            var user = GetUser(db, practiceIdentifier, userNameOrEmail);

            // Password salt and hash.
            string passwordSalt = CipherHelper.GenerateSalt();
            var passwordHash = CipherHelper.Hash(password, passwordSalt);

            user.Password = passwordHash;
            user.PasswordSalt = passwordSalt;
        }
    }
}