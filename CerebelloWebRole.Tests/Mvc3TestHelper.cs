using System.Web.Mvc;
using System.Web.Routing;
using Cerebello;
using Cerebello.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Globalization;
using Moq;
using System.IO;
using CerebelloWebRole.Code.Controllers;

namespace CerebelloWebRole.Tests
{
    /// <summary>
    /// Generates controllers for testing
    /// </summary>
    public class Mvc3TestHelper
    {
        public static T CreateControllerForTesting<T>(CerebelloEntities db, MockRepository mr, bool callOnActionExecuting = true) where T : Controller, new()
        {
            T controller = new T(); // TODO: Initialize to an appropriate value
            SetupControllerForTesting(controller, db, mr, callOnActionExecuting);
            return controller;
        }

        public static void SetupControllerForTesting(Controller controller, CerebelloEntities mainDb, MockRepository mr, bool callOnActionExecuting = true, Action<CerebelloEntities> setupNewDb = null, bool allowEmailSending = false)
        {
            var routes = new RouteCollection();
            MvcApplication.RegisterRoutes(routes);

            var rootController = controller as RootController;
            if (rootController != null)
            {
                rootController.CerebelloEntitiesCreator = () =>
                                                              {
                                                                  var db = DbTestBase.CreateNewCerebelloEntities();
                                                                  if (setupNewDb != null)
                                                                      setupNewDb(db);
                                                                  return db;
                                                              };

                if (!allowEmailSending)
                    rootController.EmailSender = eml =>
                                                     {
                                                         // Nothing to do, beacause we don't want to send any e-mail.
                                                     };
            }

            if (setupNewDb != null)
                setupNewDb(mainDb);

            var privateObject = new PrivateObject(controller);
            privateObject.SetField("db", mainDb);
            privateObject.Invoke("Initialize", mr.GetRequestContext());
            if (callOnActionExecuting)
                privateObject.Invoke("OnActionExecuting", mr.CreateActionExecutingContext());
            controller.Url = new UrlHelper(mr.GetRequestContext(), routes);
        }

        public static ActionResult ActionExecutingAndGetActionResult(Controller controller, MockRepository mr)
        {
            var privateObject = new PrivateObject(controller);
            var actionExecutingContext = mr.CreateActionExecutingContext();
            privateObject.Invoke("OnActionExecuting", actionExecutingContext);
            return actionExecutingContext.Result;
        }

        /// <summary>
        /// Validates the model object, and adds the validation messages to the ModelState of the controller.
        /// </summary>
        /// <param name="controller">Controller to which ModelState will have validation messages added to.</param>
        /// <param name="model">Model object that will be validated.</param>
        public static void SetModelStateErrors(Controller controller, object model)
        {
            // Validating the model object.
            var validationContext = new ValidationContext(model, null, null);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);

            // Adding the errors to the ModelState.
            var ms = controller.ModelState;
            foreach (var eachValidationResult in validationResults)
                foreach (var eachMemberName in eachValidationResult.MemberNames)
                    ms.AddModelError(eachMemberName, eachValidationResult.ErrorMessage);

            // Adding the remaining properties of the model to the ModelState.
            var type = model.GetType();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
            foreach (var eachPropInfo in props)
            {
                if (!ms.ContainsKey(eachPropInfo.Name))
                    ms.Add(eachPropInfo.Name, new ModelState() { });

                // todo: attemptedValue is unknown... this is a supposition. Maybe there is another way.
                var rawValue = eachPropInfo.GetValue(model, null);
                var attemptedValue = string.Format(CultureInfo.InvariantCulture, "{0}", rawValue);
                ms[eachPropInfo.Name].Value = new ValueProviderResult(rawValue, attemptedValue, CultureInfo.InvariantCulture);
            }
        }
    }
}
