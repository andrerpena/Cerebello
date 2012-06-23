using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Cerebello;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests
{
    /// <summary>
    /// Generates controllers for testing
    /// </summary>
    public class ControllersRepository
    {
        public static T CreateControllerForTesting<T>(CerebelloEntities db, bool callOnActionExecuting = true) where T : Controller, new()
        {
            var routes = new RouteCollection();
            MvcApplication.RegisterRoutes(routes);

            T controller = new T(); // TODO: Initialize to an appropriate value

            var privateObject = new PrivateObject(controller);
            privateObject.SetField("db", db);
            privateObject.Invoke("Initialize", MockRepository.GetRequestContext());
            if (callOnActionExecuting)
                privateObject.Invoke("OnActionExecuting", MockRepository.GetActionExecutingContext());
            controller.Url = new UrlHelper(MockRepository.GetRequestContext(), routes);
            return controller;
        }

        public static ActionResult ActionExecutingAndGetActionResult(Controller controller)
        {
            var privateObject = new PrivateObject(controller);
            var actionExecutingContext = MockRepository.GetActionExecutingContext();
            privateObject.Invoke("OnActionExecuting", actionExecutingContext);
            return actionExecutingContext.Result;
        }
    }
}
