using System.Web.Mvc;
using System.Web.Routing;
using Cerebello;
using Cerebello.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CerebelloWebRole.Tests
{
    /// <summary>
    /// Generates controllers for testing
    /// </summary>
    public class ControllersRepository
    {
        public static T CreateControllerForTesting<T>(CerebelloEntities db, MockRepository mr, bool callOnActionExecuting = true) where T : Controller, new()
        {
            var routes = new RouteCollection();
            MvcApplication.RegisterRoutes(routes);

            T controller = new T(); // TODO: Initialize to an appropriate value

            var privateObject = new PrivateObject(controller);
            privateObject.SetField("db", db);
            privateObject.Invoke("Initialize", mr.GetRequestContext());
            if (callOnActionExecuting)
                privateObject.Invoke("OnActionExecuting", mr.CreateActionExecutingContext());
            controller.Url = new UrlHelper(mr.GetRequestContext(), routes);
            return controller;
        }

        public static ActionResult ActionExecutingAndGetActionResult(Controller controller, MockRepository mr)
        {
            var privateObject = new PrivateObject(controller);
            var actionExecutingContext = mr.CreateActionExecutingContext();
            privateObject.Invoke("OnActionExecuting", actionExecutingContext);
            return actionExecutingContext.Result;
        }
    }
}
