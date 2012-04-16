using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CerebelloWebRole.Areas.App.Controllers;
using System.Web.Routing;
using Cerebello;
using Cerebello.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Mvc;
using System.Configuration;

namespace CerebelloWebRole.Tests
{
    /// <summary>
    /// Generates controllers for testing
    /// </summary>
    public class ControllersRepository
    {
        public static T CreateControllerForTesting<T>(CerebelloEntities db) where T : Controller, new()
        {
            var routes = new RouteCollection();
            MvcApplication.RegisterRoutes(routes);

            T controller = new T(); // TODO: Initialize to an appropriate value

            var privateObject = new PrivateObject(controller);
            privateObject.SetField("db", db);
            privateObject.Invoke("Initialize", MockRepository.GetRequestContext());
            privateObject.Invoke("OnActionExecuting", MockRepository.GetActionExecutingContext());
            controller.Url = new UrlHelper(MockRepository.GetRequestContext(), routes);
            return controller;
        }
    }
}
