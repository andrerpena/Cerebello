using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Cerebello;
using CerebelloWebRole.Code.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests
{
    /// <summary>
    /// Generates controllers for testing
    /// </summary>
    public class Mvc3TestHelper
    {
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

        /// <summary>
        /// Runs all authorization filters just like MVC does.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="actionName"></param>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        public static ActionResult RunOnAuthorization(Controller controller, string actionName, string httpMethod = "GET")
        {
            // reference: http://haacked.com/archive/2008/08/13/aspnetmvc-filters.aspx
            // reference: http://www.asp.net/mvc/tutorials/older-versions/controllers-and-routing/understanding-action-filters-cs
            // Filter execution order: Authorization, Action Execution, Result Execution, Exception Handling

            var controllerDescriptor = new ReflectedControllerDescriptor(controller.GetType());
            var filters = GetFilters<IAuthorizationFilter>(controller, controllerDescriptor, actionName, httpMethod);

            var authorizationContext = new AuthorizationContext(
                controller.ControllerContext,
                controllerDescriptor.FindAction(controller.ControllerContext, actionName));

            foreach (var eachFilter in filters)
            {
                eachFilter.OnAuthorization(authorizationContext);

                if (authorizationContext.Result != null)
                    return authorizationContext.Result;
            }

            return null;
        }

        public static ActionResult RunOnActionExecuting(Controller controller, MockRepository mr)
        {
            var actionExecutingContext = mr.CreateActionExecutingContext();
            ((IActionFilter)controller).OnActionExecuting(actionExecutingContext);
            return actionExecutingContext.Result;
        }

        /// <summary>
        /// Runs the action filter's OnActionExecuting methods.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="actionName"></param>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        public static ActionResult RunOnActionExecuting(Controller controller, string actionName, string httpMethod = "GET")
        {
            var controllerDescriptor = new ReflectedControllerDescriptor(controller.GetType());
            var filters = GetFilters<IActionFilter>(controller, controllerDescriptor, actionName, httpMethod);

            var actionExecutingContext = new ActionExecutingContext(
                controller.ControllerContext,
                controllerDescriptor.FindAction(controller.ControllerContext, actionName),
                new Dictionary<string, object>());

            foreach (var eachFilter in filters)
            {
                eachFilter.OnActionExecuting(actionExecutingContext);

                if (actionExecutingContext.Result != null)
                    return actionExecutingContext.Result;
            }

            return null;
        }

        private static List<T> GetFilters<T>(Controller controller, ControllerDescriptor controllerDescriptor, string actionName, string httpMethod = "GET")
            where T : class
        {
            var allFilters = new List<object>();

            // Getting everything that is supposed to be a filter of some kind.
            var globalFilters = new GlobalFilterCollection();
            MvcApplication.RegisterGlobalFilters(globalFilters);
            allFilters.AddRange(globalFilters);

            allFilters.AddRange(controller.GetType().GetCustomAttributes());

            var controllerContextWithMethodParam = new ControllerContext(
                new MvcHelper.MockHttpContext { Request2 = new MvcHelper.MockHttpRequest { HttpMethod2 = httpMethod } },
                new RouteData(),
                controller);
            var actionDescriptor = controllerDescriptor.FindAction(controllerContextWithMethodParam, actionName);
            allFilters.AddRange(actionDescriptor.GetCustomAttributes(true));

            allFilters.Add(controller);

            // Creating the final filters list.
            // Filters inside Filter objects come first.
            // Then comes all other filters, including controller
            // itself (if it inherits from Controller).
            var mvcFilters = new List<object>();
            mvcFilters.AddRange(allFilters.OfType<Filter>().OrderBy(f => f.Order).ThenBy(f => f.Scope).Select(f => f.Instance));
            mvcFilters.AddRange(allFilters.Where(f => f != null && f.GetType() != typeof(Filter)));

            var result = mvcFilters.OfType<T>().ToList();
            return result;
        }
    }
}
