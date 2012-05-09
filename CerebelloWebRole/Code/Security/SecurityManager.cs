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

namespace CerebelloWebRole.Code
{
    public class SecurityManager
    {

        public static void Logout()
        {
            // remover o usuário no ato do Logout() não é mais necessário
            // agora que o cache é administrado pelo XPO
            FormsAuthentication.SignOut();
        }

        public static string GetLoggedUserSecurityToken()
        {
            return ((FormsIdentity)HttpContext.Current.User.Identity).Ticket.UserData;
        }

        public static User CreateUser(CreateAccountViewModel registrationData, CerebelloEntities entities)
        {
            string passwordSalt = CipherHelper.GenerateSalt();
            var passwordHash = CipherHelper.Hash(registrationData.Password, passwordSalt);

            User user = new User()
            {
                Person = new Person()
                {
                    DateOfBirth = registrationData.DateOfBirth,
                    Gender = registrationData.Gender,
                    FullName = registrationData.FullName,
                    UrlIdentifier = StringHelper.GenerateUrlIdentifier(registrationData.FullName),
                    CreatedOn = DateTime.Now,
                },
                PasswordSalt = passwordSalt,
                Password = passwordHash,
                LastActiveOn = DateTime.Now,
                Email = registrationData.EMail,
            };

            user.Person.Emails.Add(new Email() { Address = registrationData.EMail });

            entities.Users.AddObject(user);
            return user;
        }

        public static bool Login(LoginViewModel loginModel, CerebelloEntities entities)
        {
            try
            {
                string securityToken = AuthenticateUser(loginModel.Email, loginModel.Password, entities);

                DateTime expiryDate = DateTime.UtcNow.AddYears(1);
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                     1, loginModel.Email, DateTime.UtcNow, expiryDate, true,
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
        public static String AuthenticateUser(String email, String password, CerebelloEntities entities)
        {
            User user = (from u in entities.Users where u.Email == email select u).FirstOrDefault();

            if (user == null)
                throw new Exception("Email [" + email + "] not found");

            // comparing password
            var passwordHash = CipherHelper.Hash(password, user.PasswordSalt);
            if (user.Password != passwordHash)
                throw new Exception("Password [" + password + "] is invalid");

            user.LastActiveOn = DateTime.UtcNow;

            SecurityToken securityToken = new SecurityToken()
            {
                Salt = new Random().Next(0, 2000),
                UserData = new UserData()
            };

            securityToken.UserData.Id = user.Id;
            securityToken.UserData.Email = user.Email;
            securityToken.UserData.FullName = user.Person.FullName;

            return SecurityTokenHelper.ToString(securityToken);
        }
    }
}