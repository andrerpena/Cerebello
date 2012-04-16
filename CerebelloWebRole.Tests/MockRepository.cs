using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using Moq;
using System.Web.Mvc;
using System.Web;
using CerebelloWebRole.Code.Security;
using CerebelloWebRole.Code;
using Cerebello.Model;
using System.Configuration;
using CerebelloWebRole.Code.Security.Principals;
using System.Web.Security;

namespace CerebelloWebRole.Tests
{
    public class MockRepository
    {
        /// <summary>
        /// Returns a RequestContext that can be used for testing
        /// </summary>
        /// <returns></returns>
        public static RequestContext GetRequestContext()
        {
            var mock = new Mock<RequestContext>();

            RouteData routeData = new RouteData();
            routeData.Values["practice"] = "consultoriodrhourse";
            routeData.Values["doctor"] = "gregoryhouse";

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
                var fullName = "André Rodrigues Pena";
                var email = "andrerpena@gmail.com";
                var password = "ph4r40h";

                var securityToken = SecurityManager.AuthenticateUser(email, password, context);

                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                     1, email, DateTime.UtcNow, DateTime.Now.AddYears(1), true,
                     securityToken, FormsAuthentication.FormsCookiePath);

                mock.SetupGet(m => m.Request).Returns(GetRequest());
                mock.SetupGet(m => m.Response).Returns(GetResponse());
                mock.SetupGet(m => m.User).Returns(new AuthenticatedPrincipal(new FormsIdentity(ticket), new UserData() { FullName = fullName, Email = email, Id = 1 }));
            }
            return mock.Object;
        }

        public static ActionExecutingContext GetActionExecutingContext()
        {
            var mock = new Mock<ActionExecutingContext>();
            mock.SetupGet(m => m.HttpContext).Returns(GetHttpContext());
            return mock.Object;
        }
    }
}
