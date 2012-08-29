using System;
using System.Configuration;
using System.Linq;
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
    public class MockRepository
    {
        /// <summary>
        /// Initializes a new MockRepository with the default configuration:
        /// User = André; Route:Practice = ConsultorioDrHouse; Route:Doctor = GregoryHouse.
        /// </summary>
        public MockRepository()
        {
            this.Reset();
        }

        public void Reset()
        {
            // Some old tests need these values to be initialized with default values,
            // because they don't initialize them. These values are incomplete,
            // e.g. the "action" route value is missing... new tests should not rely
            // on the default values provided by this class.

            // The default user is André.
            SetCurrentUser_Andre_CorrectPassword();

            // The route context is valid by default, for the user André.
            RouteData = new RouteData();
            RouteData.DataTokens["area"] = "App";
            RouteData.Values["practice"] = "consultoriodrhourse";
            RouteData.Values["doctor"] = "gregoryhouse";
        }

        /// <summary>
        /// Sets up André as the current logged user, in a valid sittuation.
        /// </summary>
        public void SetCurrentUser_Andre_CorrectPassword(int? userId = null)
        {
            // Setting user details.
            FullName = "André Rodrigues Pena";
            UserNameOrEmail = "andrerpena@gmail.com";
            Password = "ph4r40h";

            // Setting DB info.
            if (userId.HasValue)
            {
                this.UserDbId = userId.Value;
            }
            else
            {
                using (var db = new CerebelloEntities(string.Format("name={0}", Constants.CONNECTION_STRING_EF)))
                    this.UserDbId = db.Users.Where(u => u.UserName == "andrerpena").Single().Id;
            }
        }

        /// <summary>
        /// Sets up a User as the current logged user, using the default password.
        /// </summary>
        public void SetCurrentUser_WithDefaultPassword(User user, bool loginWithUserName = false)
        {
            // Setting user details.
            FullName = user.Person.FullName;
            UserNameOrEmail = loginWithUserName ? user.UserName : user.Person.Email;
            Password = CerebelloWebRole.Code.Constants.DEFAULT_PASSWORD;

            // Setting DB info.
            UserDbId = user.Id;
        }

        public void SetRouteData_ConsultorioDrHourse_GregoryHouse(Type controllerType, string action)
        {
            RouteData = new RouteData();

            FillRouteData_App_Controller_Action(this.RouteData, controllerType, action);

            RouteData.Values["practice"] = "consultoriodrhourse";
            RouteData.Values["doctor"] = "gregoryhouse";
        }

        public void SetRouteData_OutroConsultorio_GregoryHouse(Type controllerType, string action)
        {
            RouteData = new RouteData();

            FillRouteData_App_Controller_Action(this.RouteData, controllerType, action);

            RouteData.Values["practice"] = "outro_consultorio";
            RouteData.Values["doctor"] = "gregoryhouse";
        }

        public void SetRouteData<T>(Practice p, Doctor d, string action) where T : Controller
        {
            var type = typeof(T);

            SetRouteData(type, p, d, action);
        }

        public void SetRouteData(Type controllerType, Practice p, Doctor d, string action)
        {
            RouteData = new RouteData();

            FillRouteData_App_Controller_Action(this.RouteData, controllerType, action);

            if (p != null)
                RouteData.Values["practice"] = p.UrlIdentifier;

            if (d != null)
                RouteData.Values["doctor"] = d.Users.First().Person.UrlIdentifier;
        }

        private static void FillRouteData_App_Controller_Action(RouteData routeData, Type controllerType, string action)
        {
            var matchController = Regex.Match(controllerType.Name, @"(?<CONTROLLER>.*?)Controller");
            var matchArea = Regex.Match(controllerType.Namespace, @"Areas\.(?<AREA>.*?)(?=\.Controllers)");
            
            if (matchArea.Success)
                routeData.Values["controller"] = matchController.Groups["CONTROLLER"].Value.ToLowerInvariant();
            else
                throw new Exception("Could not determine controller.");

            if (matchArea.Success)
                routeData.DataTokens["area"] = matchArea.Groups["AREA"].Value.ToLowerInvariant();

            if (action != null)
                routeData.Values["action"] = action;
        }

        /// <summary>
        /// Full name of the logged user.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// E-mail of the logged user.
        /// </summary>
        public string UserNameOrEmail { get; set; }

        /// <summary>
        /// Password of the logged user.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Id of the User object, that represents the current user in the data-store.
        /// </summary>
        public int UserDbId { get; set; }

        /// <summary>
        /// RouteData that will be used.
        /// </summary>
        public RouteData RouteData { get; set; }

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
        public RequestContext GetRequestContext()
        {
            var mock = new Mock<RequestContext>();

            mock.SetupGet(rq => rq.RouteData).Returns(RouteData);
            mock.SetupGet(m => m.HttpContext).Returns(GetHttpContext());

            return mock.Object;
        }

        public HttpRequestBase GetRequest()
        {
            var mock = new Mock<HttpRequestBase>();
            mock.SetupGet(m => m.IsAuthenticated).Returns(true);
            mock.SetupGet(m => m.ApplicationPath).Returns("/");
            mock.SetupGet(m => m.Url).Returns(new Uri("http://localhost/unittests", UriKind.Absolute));
            mock.SetupGet(m => m.ServerVariables).Returns(new System.Collections.Specialized.NameValueCollection());

            return mock.Object;
        }

        public HttpResponseBase GetResponse()
        {
            var mock = new Mock<HttpResponseBase>();
            mock.Setup(x => x.ApplyAppPathModifier(Moq.It.IsAny<String>())).Returns((String url) => url);

            return mock.Object;
        }

        public HttpContextBase GetHttpContext()
        {
            var mock = new Mock<HttpContextBase>();

            using (var db = new CerebelloEntities(string.Format("name={0}", Constants.CONNECTION_STRING_EF)))
            {
                var securityToken = SecurityManager.AuthenticateUser(
                    this.UserNameOrEmail,
                    this.Password,
                    string.Format("{0}", this.RouteData.Values["practice"]),
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

        public ActionExecutingContext CreateActionExecutingContext()
        {
            var mock = new Mock<ActionExecutingContext>();
            mock.SetupGet(m => m.HttpContext).Returns(GetHttpContext());
            mock.SetupGet(m => m.RouteData).Returns(RouteData);
            return mock.Object;
        }

        public void SetRouteData_ControllerAndActionOnly(string controller, string action)
        {
            this.RouteData.Values["controller"] = controller;
            this.RouteData.Values["action"] = action;
        }
    }
}
