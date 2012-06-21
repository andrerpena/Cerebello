using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
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
            // The default user is André, in a valid context.
            SetCurrentUser_Andre_Valid();
        }

        /// <summary>
        /// Sets up André as the current logged user, in a valid sittuation.
        /// </summary>
        public static void SetCurrentUser_Andre_Valid()
        {
            // Setting user details.
            FullName = "André Rodrigues Pena";
            Email = "andrerpena@gmail.com";
            Password = "ph4r40h";

            // Setting DB info.
            UserDbId = 1;

            // Setting route info.
            RoutePractice = "consultoriodrhourse";
            RouteDoctor = "gregoryhouse";
        }

        /// <summary>
        /// Sets up André as the current logged user, in an invalid sittuation.
        /// </summary>
        public static void SetCurrentUser_Andre_InvalidAccessToPractice()
        {
            // Setting user details.
            FullName = "André Rodrigues Pena";
            Email = "andrerpena@gmail.com";
            Password = "ph4r40h";

            // Setting DB info.
            UserDbId = 1;

            // Setting route info, so that it is invalid.
            // - The user André does not have acces to the practice "outro_consultorio",
            //      so access to it should be denied.
            RoutePractice = "outro_consultorio";
            RouteDoctor = "gregoryhouse";
        }

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
        /// Returns a RequestContext that can be used for testing
        /// </summary>
        /// <returns></returns>
        public static RequestContext GetRequestContext()
        {
            var mock = new Mock<RequestContext>();

            RouteData routeData = new RouteData();
            routeData.Values["practice"] = RoutePractice;
            routeData.Values["doctor"] = RouteDoctor;

            mock.SetupGet(rq => rq.RouteData).Returns(routeData);
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

            using (var context = new CerebelloEntities(ConfigurationManager.ConnectionStrings[Constants.CONNECTION_STRING_EF].ConnectionString))
            {
                var securityToken = SecurityManager.AuthenticateUser(Email, Password, context);

                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                     version: 1,
                     name: Email,
                     issueDate: DateTime.UtcNow,
                     expiration: DateTime.Now.AddYears(1),
                     isPersistent: true,
                     userData: securityToken,
                     cookiePath: FormsAuthentication.FormsCookiePath);

                var userData =
                    new AuthenticatedPrincipal(new FormsIdentity(ticket), new UserData()
                    {
                        FullName = FullName,
                        Email = Email,
                        Id = UserDbId,
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
            return mock.Object;
        }

        /// <summary>
        /// Full name of the logged user.
        /// </summary>
        public static string FullName { get; set; }

        /// <summary>
        /// E-mail of the logged user.
        /// </summary>
        public static string Email { get; set; }

        /// <summary>
        /// Password of the logged user.
        /// </summary>
        public static string Password { get; set; }

        /// <summary>
        /// Id of the User object, that represents the current user in the data-store.
        /// </summary>
        public static int UserDbId { get; set; }

        /// <summary>
        /// Practice name that should be used in the route.
        /// </summary>
        public static string RoutePractice { get; set; }

        /// <summary>
        /// Doctor name that should be used in the route.
        /// </summary>
        public static string RouteDoctor { get; set; }
    }
}
