using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Cerebello.Model;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Security;
using CerebelloWebRole.Code.Security.Principals;
using Moq;

namespace CerebelloWebRole.Tests
{
    public static class MockRepository
    {
        static MockRepository()
        {
            Reset();
        }

        public static void Reset()
        {
            // The default user is André.
            SetCurrentUser_Andre_CorrectPassword();

            // The route context is valid by default, for the user André.
            SetRouteData_App_ConsultorioDrHourse_GregoryHouse();
        }

        /// <summary>
        /// Sets up André as the current logged user, in a valid sittuation.
        /// </summary>
        public static void SetCurrentUser_Andre_CorrectPassword(int userId = 1)
        {
            // Setting user details.
            FullName = "André Rodrigues Pena";
            UserNameOrEmail = "andrerpena@gmail.com";
            Password = "ph4r40h";

            // Setting DB info.
            UserDbId = userId;
        }

        /// <summary>
        /// Sets up a User as the current logged user, using the default password.
        /// </summary>
        public static void SetCurrentUser_WithDefaultPassword(User user, bool loginWithUserName = false)
        {
            // Setting user details.
            FullName = user.Person.FullName;
            UserNameOrEmail = loginWithUserName ? user.UserName : (user.Email ?? "");
            Password = CerebelloWebRole.Code.Constants.DEFAULT_PASSWORD;

            // Setting DB info.
            UserDbId = user.Id;
        }

        public static void SetRouteData_App_ConsultorioDrHourse_GregoryHouse()
        {
            RouteData = new RouteData();
            RouteData.DataTokens["area"] = "App";
            RouteData.Values["practice"] = "consultoriodrhourse";
            RouteData.Values["doctor"] = "gregoryhouse";
        }

        public static void SetRouteData_App_OutroConsultorio_GregoryHouse()
        {
            RouteData = new RouteData();
            RouteData.DataTokens["area"] = "App";
            RouteData.Values["practice"] = "outro_consultorio";
            RouteData.Values["doctor"] = "gregoryhouse";
        }

        public static void SetRouteData<T>(Practice p, Doctor d, string action) where T : Controller
        {
            var type = typeof(T);

            SetRouteData(type, p, d, action);
        }

        public static void SetRouteData(Type controllerType, Practice p, Doctor d, string action)
        {
            var matchController = Regex.Match(controllerType.Name, @"(?<CONTROLLER>.*?)Controller");
            var matchArea = Regex.Match(controllerType.Namespace, @"Areas\.(?<AREA>.*?)(?=\.Controllers)");

            RouteData = new RouteData();

            if (matchArea.Success)
                RouteData.Values["controller"] = matchController.Groups["CONTROLLER"].Value.ToLowerInvariant();
            else
                throw new Exception("Could not determine controller.");

            if (matchArea.Success)
                RouteData.DataTokens["area"] = matchArea.Groups["AREA"].Value.ToLowerInvariant();

            if (p != null)
                RouteData.Values["practice"] = p.UrlIdentifier;

            if (d != null)
                RouteData.Values["doctor"] = d.Users.First().Person.UrlIdentifier;

            if (action != null)
                RouteData.Values["action"] = action;
        }

        /// <summary>
        /// Full name of the logged user.
        /// </summary>
        public static string FullName { get; set; }

        /// <summary>
        /// E-mail of the logged user.
        /// </summary>
        public static string UserNameOrEmail { get; set; }

        /// <summary>
        /// Password of the logged user.
        /// </summary>
        public static string Password { get; set; }

        /// <summary>
        /// Id of the User object, that represents the current user in the data-store.
        /// </summary>
        public static int UserDbId { get; set; }

        /// <summary>
        /// RouteData that will be used.
        /// </summary>
        public static RouteData RouteData { get; set; }

        /// <summary>
        /// HttpServerUtilityBase stub.
        /// </summary>
        public class HttpServerUtilityBaseStub : HttpServerUtilityBase
        {
            /// <summary>
            /// Resolves MapPath
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public override string MapPath(string path)
            {
                if (!path.StartsWith(@"~\"))
                    throw new Exception("Invalid path. Path doesn't start with '~\'");

                var appRootPath = ConfigurationManager.AppSettings["AppRootPath"];

                if (string.IsNullOrEmpty(appRootPath))
                    throw new Exception("AppRootPath is not defined in AppSettings in the application configuration file");

                if (!appRootPath.EndsWith(@"\"))
                    appRootPath += @"\";

                return path.Replace(@"~\", appRootPath);
            }
        }

        /// <summary>
        /// Returns a RequestContext that can be used for testing.
        /// </summary>
        /// <returns></returns>
        public static RequestContext GetRequestContext()
        {
            var mock = new Mock<RequestContext>();

            mock.SetupGet(rq => rq.RouteData).Returns(RouteData);
            mock.SetupGet(m => m.HttpContext).Returns(GetHttpContext());

            return mock.Object;
        }

        public static HttpRequestBase GetRequest()
        {
            var mock = new Mock<HttpRequestBase>();
            mock.SetupGet(m => m.IsAuthenticated).Returns(true);
            mock.SetupGet(m => m.ApplicationPath).Returns("/");
            mock.SetupGet(m => m.Url).Returns(new Uri("http://localhost/unittests", UriKind.Absolute));
            mock.SetupGet(m => m.ServerVariables).Returns(new System.Collections.Specialized.NameValueCollection());

            return mock.Object;
        }

        public static HttpResponseBase GetResponse()
        {
            var mock = new Mock<HttpResponseBase>();
            mock.Setup(x => x.ApplyAppPathModifier(Moq.It.IsAny<String>())).Returns((String url) => url);

            return mock.Object;
        }

        public static HttpContextBase GetHttpContext()
        {
            var mock = new Mock<HttpContextBase>();

            using (var db = new CerebelloEntities(ConfigurationManager.ConnectionStrings[Constants.CONNECTION_STRING_EF].ConnectionString))
            {
                var securityToken = SecurityManager.AuthenticateUser(
                    UserNameOrEmail,
                    Password,
                    string.Format("{0}", RouteData.Values["practice"]),
                    db);

                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                     version: 1,
                     name: UserNameOrEmail,
                     issueDate: DateTime.UtcNow,
                     expiration: DateTime.Now.AddYears(1),
                     isPersistent: true,
                     userData: securityToken,
                     cookiePath: FormsAuthentication.FormsCookiePath);

                var userData =
                    new AuthenticatedPrincipal(new FormsIdentity(ticket), new UserData()
                    {
                        FullName = FullName,
                        Email = UserNameOrEmail,
                        Id = UserDbId,
                        IsUsingDefaultPassword = Password == CerebelloWebRole.Code.Constants.DEFAULT_PASSWORD,
                    });

                mock.SetupGet(m => m.Request).Returns(GetRequest());
                mock.SetupGet(m => m.Response).Returns(GetResponse());
                mock.SetupGet(m => m.User).Returns(userData);
                mock.SetupGet(m => m.Server).Returns(new HttpServerUtilityBaseStub());
            }
            return mock.Object;
        }

        public static ActionExecutingContext GetActionExecutingContext()
        {
            var mock = new Mock<ActionExecutingContext>();
            mock.SetupGet(m => m.HttpContext).Returns(GetHttpContext());
            mock.SetupGet(m => m.RouteData).Returns(RouteData);
            return mock.Object;
        }
    }
}
