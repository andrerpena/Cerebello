using System;
using System.Configuration;
using System.IO;
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
        public MockRepository(bool initForOldTests = false)
        {
            if (initForOldTests)
                this.Reset();
        }

        private void Reset()
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
            this.IsAuthenticated = true;
            this.FullName = "André Rodrigues Pena";
            this.UserNameOrEmail = "andrerpena@gmail.com";
            this.Password = "ph4r40h";

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
            this.IsAuthenticated = true;
            this.FullName = user.Person.FullName;
            this.UserNameOrEmail = loginWithUserName ? user.UserName : (user.Email ?? "");
            this.Password = CerebelloWebRole.Code.Constants.DEFAULT_PASSWORD;

            // Setting DB info.
            this.UserDbId = user.Id;
        }

        public void SetCurrentUser(User user, string password)
        {
            this.IsAuthenticated = true;
            this.FullName = user.Person.FullName;
            this.UserNameOrEmail = user.UserName;
            this.Password = password;
            this.UserDbId = user.Id;
        }

        public void SetRouteData_ConsultorioDrHourse_GregoryHouse(Type controllerType, string action)
        {
            this.RouteData = new RouteData();

            FillRouteData_App_Controller_Action(this.RouteData, controllerType, action);

            this.RouteData.Values["practice"] = "consultoriodrhourse";
            this.RouteData.Values["doctor"] = "gregoryhouse";
        }

        public void SetRouteData_OutroConsultorio_GregoryHouse(Type controllerType, string action)
        {
            this.RouteData = new RouteData();

            FillRouteData_App_Controller_Action(this.RouteData, controllerType, action);

            this.RouteData.Values["practice"] = "outro_consultorio";
            this.RouteData.Values["doctor"] = "gregoryhouse";
        }

        public void SetRouteData<T>(Practice p, Doctor d, string action) where T : Controller
        {
            var type = typeof(T);

            this.SetRouteData(type, p, d, action);
        }

        public void SetRouteData(Type controllerType, Practice p, Doctor d, string action)
        {
            this.RouteData = new RouteData();

            FillRouteData_App_Controller_Action(this.RouteData, controllerType, action);

            if (p != null)
                this.RouteData.Values["practice"] = p.UrlIdentifier;

            if (d != null)
                this.RouteData.Values["doctor"] = d.UrlIdentifier;
        }

        public void SetRouteData(Type controllerType, string action)
        {
            this.RouteData = new RouteData();
            FillRouteData_App_Controller_Action(this.RouteData, controllerType, action);
        }

        public void SetRouteData(string action, string controller, string area = null, string practice = null)
        {
            this.RouteData = new RouteData();

            if (string.IsNullOrEmpty(controller))
                throw new Exception("'controller' cannot be null nor empty.");

            this.RouteData.Values["controller"] = controller;

            if (area != null)
                this.RouteData.DataTokens["area"] = area;

            if (string.IsNullOrEmpty(action))
                throw new Exception("'action' cannot be null nor empty.");

            this.RouteData.Values["action"] = action;

            if (practice != null)
                this.RouteData.Values["practice"] = practice;
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

            if (string.IsNullOrEmpty(action))
                throw new Exception("'action' cannot be null nor empty.");

            routeData.Values["action"] = action;
        }

        public void SetRouteData_ControllerAndActionOnly(string controller, string action)
        {
            this.RouteData.Values["controller"] = controller;
            this.RouteData.Values["action"] = action;
        }

        /// <summary>
        /// Indicates whether an user is loged-in or not.
        /// If not, all the other properties related to the user are invalid.
        /// </summary>
        public bool IsAuthenticated { get; set; }

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
            // todo: maybe replace this class with a Mock<HttpServerUtilityBase>

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

            mock.SetupGet(rq => rq.RouteData).Returns(this.RouteData);
            mock.SetupGet(rq => rq.HttpContext).Returns(this.GetHttpContext());

            return mock.Object;
        }

        public HttpRequestBase GetRequest()
        {
            var mock = new Mock<HttpRequestBase>();
            mock.SetupGet(m => m.IsAuthenticated).Returns(this.IsAuthenticated);
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
                Principal principal;

                if (this.IsAuthenticated)
                {
                    User user;

                    var securityToken = SecurityManager.AuthenticateUser(
                        this.UserNameOrEmail,
                        this.Password,
                        string.Format("{0}", this.RouteData.Values["practice"]),
                        db,
                        out user);

                    user.LastActiveOn = DateTime.UtcNow;

                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                         version: 1,
                         name: this.UserNameOrEmail,
                         issueDate: DateTime.UtcNow,
                         expiration: DateTime.UtcNow.AddYears(1),
                         isPersistent: true,
                         userData: securityToken,
                         cookiePath: FormsAuthentication.FormsCookiePath);

                    principal =
                        new AuthenticatedPrincipal(new FormsIdentity(ticket), new UserData()
                        {
                            FullName = this.FullName,
                            Email = this.UserNameOrEmail,
                            Id = this.UserDbId,
                            IsUsingDefaultPassword = this.Password == CerebelloWebRole.Code.Constants.DEFAULT_PASSWORD,
                        });
                }
                else
                {
                    principal = new AnonymousPrincipal(new GuestIdentity());
                }

                mock.SetupGet(m => m.Request).Returns(GetRequest());
                mock.SetupGet(m => m.Response).Returns(GetResponse());
                mock.SetupGet(m => m.User).Returns(principal);
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

        /// <summary>
        /// Mocks the value of HttpContext.Current,
        /// and returns a disposable object to set it back to null,
        /// so that one test does not affect the others.
        /// </summary>
        /// <returns>Returns the created HttpContext.</returns>
        public HttpContext SetupHttpContext(Disposer disposer)
        {
            var httpContext = this.GetHttpContext();
            var request = httpContext.Request;
            var response = httpContext.Response;

            var oldHttpContext = HttpContext.Current;
            HttpContext.Current = new HttpContext(
                new HttpRequest("filename", request.Url.AbsoluteUri, request.Url.Query),
                new HttpResponse(new StreamWriter(response.OutputStream ?? new MemoryStream())));

            disposer.Disposing += new Action(() => { HttpContext.Current = oldHttpContext; });

            return HttpContext.Current;
        }

        /// <summary>
        /// Creates a mocked ViewEngine and replaces all the other existing view engines.
        /// The returned object can be used to configure the new ViewEngine.
        /// </summary>
        /// <param name="disposer">Disposer that will be given the task of reverting the actions of this method.</param>
        /// <returns>Returns an object that can be used to configure the views.</returns>
        public Mock<IViewEngine> SetupViewEngine(Disposer disposer)
        {
            var oldViewEngines = ViewEngines.Engines.ToArray();

            var result = new Mock<IViewEngine>();
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(result.Object);

            disposer.Disposing += new Action(() =>
            {
                ViewEngines.Engines.Clear();
                foreach (var eachViewEngine in oldViewEngines)
                    ViewEngines.Engines.Add(eachViewEngine);
            });

            return result;
        }
    }
}
