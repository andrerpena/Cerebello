using System.Web.Mvc;
using System.Web.Routing;
using CerebelloWebRole.Code.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests.Self
{
    [TestClass]
    public class Mvc3HelperTests
    {
        [TestMethod]
        public void Test_RunOnActionExecuting()
        {
            var controller = new MyController();
            controller.CallInitialize();
            Mvc3TestHelper.RunOnActionExecuting(controller, "Action");
            Assert.IsTrue(controller.OnActionExecutingCalled);
            Assert.IsTrue(controller.Attribute_OnActionExecutingCalled);
        }

        [TestMethod]
        public void Test_RunOnAuthorization()
        {
            var controller = new MyController();
            controller.CallInitialize();
            Mvc3TestHelper.RunOnAuthorization(controller, "Action");
            Assert.IsTrue(controller.OnAuthorizationCalled);
            Assert.IsTrue(controller.Attribute_OnAuthorizationCalled);
        }

        class MyController : Controller
        {
            protected override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                this.OnActionExecutingCalled = true;
            }

            [AuthorizationFilter]
            [ActionFilter]
            public ActionResult Action()
            {
                this.ActionCalled = true;
                return this.View();
            }

            public bool ActionCalled { get; set; }

            protected override void OnAuthorization(AuthorizationContext filterContext)
            {
                this.OnAuthorizationCalled = true;
            }

            public bool OnActionExecutingCalled { get; set; }

            public void CallInitialize()
            {
                this.Initialize(new RequestContext(
                    new MvcHelper.MockHttpContext { Request2 = new MvcHelper.MockHttpRequest() },
                    new RouteData()));
            }

            public bool OnAuthorizationCalled { get; set; }

            public class AuthorizationFilterAttribute : FilterAttribute, IAuthorizationFilter
            {
                public void OnAuthorization(AuthorizationContext filterContext)
                {
                    (filterContext.Controller as MyController).Attribute_OnAuthorizationCalled = true;
                }
            }

            public bool Attribute_OnAuthorizationCalled { get; set; }

            public class ActionFilterAttribute : FilterAttribute, IActionFilter
            {
                public void OnActionExecuting(ActionExecutingContext filterContext)
                {
                    (filterContext.Controller as MyController).Attribute_OnActionExecutingCalled = true;
                }
                public void OnActionExecuted(ActionExecutedContext filterContext)
                {
                    (filterContext.Controller as MyController).Attribute_OnActionExecutedCalled = true;
                }
            }

            public bool Attribute_OnActionExecutingCalled { get; set; }

            public bool Attribute_OnActionExecutedCalled { get; set; }
        }
    }
}
