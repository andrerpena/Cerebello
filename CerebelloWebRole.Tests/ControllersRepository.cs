using System.Web.Mvc;
using System.Web.Routing;
using Cerebello;
using Cerebello.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace CerebelloWebRole.Tests
{
    /// <summary>
    /// Generates controllers for testing
    /// </summary>
#warning [Rename] Suggestion Mvc3TestHelper.
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
        }
    }
}
