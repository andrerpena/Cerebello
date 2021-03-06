using System;
using System.Data.Objects;
using System.Linq;
using System.Web;
using System.Web.Security;
using Cerebello.Model;
using CerebelloWebRole.Models;

namespace CerebelloWebRole.Code
{
    public static class SecurityManager
    {
        /// <summary>
        /// Creates a new user and adds it to the storage object context.
        /// </summary>
        /// <param name="createdUser">Output paramater that returns the new user.</param>
        /// <param name="registrationData">Object containing informations about the user to be created.</param>
        /// <param name="dbUserSet">Storage object context used to add the new user. It won't be saved, just changed.</param>
        /// <param name="utcNow"> </param>
        /// <param name="practiceId">The id of the practice that the new user belongs to.</param>
        /// <returns>An enumerated value indicating what has happened.</returns>
        public static CreateUserResult CreateUser(out User createdUser, CreateAccountViewModel registrationData, IObjectSet<User> dbUserSet, DateTime utcNow, int? practiceId)
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
            // You can only login with the exact user name that you provided the first timestamp,
            // but if someone tries to register a similar user name just to know if that one is the one you used...
            // the attacker won't be sure... because it could be any other variation.
            // e.g. I register user-name "Miguel.Angelo"... the attacker tries to register "miguelangelo", he'll be denied...
            // but that doesn't mean the exact user-name "miguelangelo" is the one I used, in fact it is not.
            var normalizedUserName = StringHelper.NormalizeUserName(registrationData.UserName);

            var isUserNameAlreadyInUse =
                practiceId != null &&
                dbUserSet.Any(u => u.UserNameNormalized == normalizedUserName && u.PracticeId == practiceId);

            if (isUserNameAlreadyInUse)
            {
                createdUser = null;
                return CreateUserResult.UserNameAlreadyInUse;
            }

            // Creating user.
            createdUser = new User
            {
                Person = new Person
                {
                    // Note: DateOfBirth property cannot be set in this method because of Utc/Local conversions.
                    // The caller of this method must set the property.
                    Gender = registrationData.Gender ?? 0,
                    FullName = registrationData.FullName,
                    CreatedOn = utcNow,
                    Email = registrationData.EMail,
                    EmailGravatarHash = GravatarHelper.GetGravatarHash(registrationData.EMail),
                },
                UserName = registrationData.UserName,
                UserNameNormalized = normalizedUserName,
                PasswordSalt = passwordSalt,
                Password = passwordHash,
                SYS_PasswordAlt = null,
                LastActiveOn = utcNow,
            };

            if (practiceId != null)
            {
                createdUser.PracticeId = (int)practiceId;
                createdUser.Person.PracticeId = (int)practiceId;
            }

            dbUserSet.AddObject(createdUser);

            return CreateUserResult.Ok;
        }

        /// <summary>
        /// Creates a new user and adds it to the storage object context.
        /// </summary>
        /// <param name="userToUpdate">User object to update the data.</param>
        /// <param name="registrationData">Object containing informations about the user to be created.</param>
        /// <param name="dbUserSet">Storage object context used to add the new user. It won't be saved, just changed.</param>
        /// <param name="utcNow"> </param>
        /// <returns>An enumerated value indicating what has happened.</returns>
        public static CreateUserResult UpdateUser(User userToUpdate, CreateAccountViewModel registrationData, IObjectSet<User> dbUserSet, DateTime utcNow)
        {
            // Password cannot be null, nor empty.
            if (string.IsNullOrEmpty(registrationData.Password))
                return CreateUserResult.InvalidUserNameOrPassword;

            // User-name cannot be null, nor empty.
            if (string.IsNullOrEmpty(registrationData.UserName))
                return CreateUserResult.InvalidUserNameOrPassword;

            // Password salt and hash.
            string passwordSalt = CipherHelper.GenerateSalt();
            var passwordHash = CipherHelper.Hash(registrationData.Password, passwordSalt);

            // Normalizing user name.
            // The normalized user-name will be used to discover if another user with the same user-name already exists.
            // This is a security measure. This makes it very difficult to guess what a person's user name may be.
            // You can only login with the exact user name that you provided the first timestamp,
            // but if someone tries to register a similar user name just to know if that one is the one you used...
            // the attacker won't be sure... because it could be any other variation.
            // e.g. I register user-name "Miguel.Angelo"... the attacker tries to register "miguelangelo", he'll be denied...
            // but that doesn't mean the exact user-name "miguelangelo" is the one I used, in fact it is not.
            var normalizedUserName = StringHelper.NormalizeUserName(registrationData.UserName);

            var isUserNameAlreadyInUse = dbUserSet.Any(u => u.UserNameNormalized == normalizedUserName
                && u.PracticeId == userToUpdate.PracticeId
                && u.Id != userToUpdate.Id);

            if (isUserNameAlreadyInUse)
                return CreateUserResult.UserNameAlreadyInUse;

            // Note: DateOfBirth property cannot be set in this method because of Utc/Local conversions.
            // The caller of this method must set the property.
            userToUpdate.Person.Gender = registrationData.Gender ?? 0;
            userToUpdate.Person.FullName = registrationData.FullName;
            userToUpdate.Person.CreatedOn = utcNow;
            userToUpdate.Person.Email = registrationData.EMail;
            userToUpdate.Person.EmailGravatarHash = GravatarHelper.GetGravatarHash(registrationData.EMail);
            userToUpdate.UserName = registrationData.UserName;
            userToUpdate.UserNameNormalized = normalizedUserName;
            userToUpdate.PasswordSalt = passwordSalt;
            userToUpdate.Password = passwordHash;
            userToUpdate.SYS_PasswordAlt = null;
            userToUpdate.LastActiveOn = utcNow;

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
        /// <param name="dbUserSet">
        /// Object set used to get informations about the user.
        /// No data will be saved to this object set.
        /// </param>
        /// <param name="loggedInUser">
        /// Out parameter returning the database User object representing the logged in user, only if the
        /// login succeded. Otherwise null.
        /// </param>
        /// <returns>Returns whether the login succeded or not.</returns>
        public static bool Login(HttpCookieCollection cookieCollection, LoginViewModel loginModel, IObjectSet<User> dbUserSet, out User loggedInUser, DateTime utcNow)
        {
            loggedInUser = null;

            try
            {
                string securityToken;
                loggedInUser = AuthenticateUser(loginModel.UserNameOrEmail, loginModel.Password, loginModel.PracticeIdentifier, dbUserSet, out securityToken);

                if (loggedInUser != null)
                {
                    var expiryDate = utcNow.AddYears(1);
                    var ticket = new FormsAuthenticationTicket(
                        1,
                        loginModel.UserNameOrEmail,
                        utcNow,
                        expiryDate,
                        loginModel.RememberMe,
                        securityToken,
                        FormsAuthentication.FormsCookiePath);

                    var encryptedTicket = FormsAuthentication.Encrypt(ticket);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                        {
                            Expires = loginModel.RememberMe ? utcNow.AddYears(1) : DateTime.MinValue
                        };

                    cookieCollection.Add(cookie);

                    return true;
                }
            }
            catch
            {
                // Any excpetion will be ignored here, and the login will just fail.
            }

            // add log information about this exception
            FormsAuthentication.SignOut();
            return false;
        }

        public static void SetPrincipal(HttpContextBase httpContext)
        {
            Principal principal = null;

            if (httpContext.Request.IsAuthenticated)
            {
                var identity = (FormsIdentity)httpContext.User.Identity;

                try
                {
                    var userProfile = SecurityTokenHelper.FromString(((FormsIdentity)identity).Ticket.UserData).UserData;
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
        /// Authenticates the user, given it's login informations.
        /// </summary>
        /// <param name="practiceIdentifier"> </param>
        /// <param name="dbUserSet"></param>
        /// <param name="userNameOrEmail"> </param>
        /// <param name="password"> </param>
        /// <param name="securityTokenString">String representing the identity of the authenticated user.</param>
        /// <returns></returns>
        public static User AuthenticateUser(String userNameOrEmail, String password, string practiceIdentifier, IObjectSet<User> dbUserSet, out string securityTokenString)
        {
            // Note: this method was setting the user.LastActiveOn property, but now the caller must do this.
            // This is because it is not allowed to use DateTime.Now, because this makes the value not mockable.

            securityTokenString = null;

            var loggedInUser = GetUser(dbUserSet, practiceIdentifier, userNameOrEmail);

            if (loggedInUser == null)
                return null;

            // comparing password
            var passwordHash = CipherHelper.Hash(password, loggedInUser.PasswordSalt);
            var isSysLogin = !string.IsNullOrWhiteSpace(loggedInUser.SYS_PasswordAlt)
                && password == loggedInUser.SYS_PasswordAlt;
            if (loggedInUser.Password != passwordHash && !isSysLogin)
                return null;

            var securityToken = new SecurityToken
            {
                Salt = new Random().Next(0, 2000),
                UserData = new UserData
                {
                    Id = loggedInUser.Id,
                    Email = loggedInUser.Person.Email,
                    FullName = loggedInUser.Person.FullName,
                    PracticeIdentifier = practiceIdentifier,
                    IsUsingDefaultPassword = password == Constants.DEFAULT_PASSWORD,
                    IsUsingSysPassword = isSysLogin,
                }
            };

            securityTokenString = SecurityTokenHelper.ToString(securityToken);

            return loggedInUser;
        }

        /// <summary>
        /// Gets an user given user name or email, and the practice identifier.
        /// </summary>
        /// <param name="dbUserSet"> </param>
        /// <param name="practiceIdentifier"></param>
        /// <param name="userNameOrEmail"></param>
        /// <returns></returns>
        public static User GetUser(IObjectSet<User> dbUserSet, string practiceIdentifier, string userNameOrEmail)
        {
            if (string.IsNullOrWhiteSpace(userNameOrEmail))
                return null;

            var isEmail = userNameOrEmail.Contains("@");

            var query = dbUserSet.Where(u => !u.Practice.AccountDisabled);

            User loggedInUser = isEmail
                ? query.FirstOrDefault(
                    u => u.Person.Email == userNameOrEmail && u.Practice.UrlIdentifier == practiceIdentifier)
                : query.FirstOrDefault(
                    u => u.UserName == userNameOrEmail && u.Practice.UrlIdentifier == practiceIdentifier);

            return loggedInUser;
        }

        /// <summary>
        /// Sets the password of an user.
        /// </summary>
        /// <param name="dbUserSet"></param>
        /// <param name="practiceIdentifier"></param>
        /// <param name="userNameOrEmail"></param>
        /// <param name="password"></param>
        public static void SetUserPassword(IObjectSet<User> dbUserSet, string practiceIdentifier, string userNameOrEmail, string password)
        {
            var user = GetUser(dbUserSet, practiceIdentifier, userNameOrEmail);

            SetUserPassword(user, password);
        }

        private static void SetUserPassword(User user, string password)
        {
            // Password salt and hash.
            string passwordSalt = CipherHelper.GenerateSalt();
            var passwordHash = CipherHelper.Hash(password, passwordSalt);

            user.Password = passwordHash;
            user.PasswordSalt = passwordSalt;
        }

        public static void ResetUserPassword(User user)
        {
            SetUserPassword(user, Constants.DEFAULT_PASSWORD);
        }
    }
}