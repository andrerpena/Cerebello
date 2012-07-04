using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Code.Mvc
{
    public static class ModelStateExtensions
    {
        public static void AddModelError<TModel>(this ModelStateDictionary modelState, Expression<Func<TModel, object>> expression, string errorMessage)
        {
            // todo: this method should accept a resource name, instead of an error message.

            var propertyInfo = MemberExpressionHelper.GetPropertyInfo(expression);

            if (errorMessage.Contains("{0}"))
            {
                var displayAttribute = propertyInfo.GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().FirstOrDefault();
                var propertyDisplay = displayAttribute != null ? displayAttribute.Name : propertyInfo.Name;
                errorMessage = string.Format(errorMessage, propertyDisplay);
            }

            modelState.AddModelError(propertyInfo.Name, errorMessage);
        }

        /// <summary>
        /// Add a new validation message to the collection of messages and exceptions associated with the given model property.
        /// </summary>
        /// <param name="modelState">ModelState objeto to add the validation message to.</param>
        /// <param name="expression">Expression tree that goes to the property that is not valid.</param>
        /// <param name="errorMessage">Validation message to associate with the property.</param>
        public static void AddModelError(this ModelStateDictionary modelState, Expression<Func<object>> expression, string errorMessage)
        {
            // todo: this method should accept a resource name, instead of an error message.

            var propertyInfo = MemberExpressionHelper.GetPropertyInfo(expression);

            if (errorMessage.Contains("{0}"))
            {
                var displayAttribute = propertyInfo.GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().FirstOrDefault();
                var propertyDisplay = displayAttribute != null ? displayAttribute.Name : propertyInfo.Name;
                errorMessage = string.Format(errorMessage, propertyDisplay);
            }

            modelState.AddModelError(propertyInfo.Name, errorMessage);
        }

        /// <summary>
        /// Returns the ModelState object associated with a model property.
        /// </summary>
        /// <param name="modelState">ModelStateDictionary that contains validation messages of the model.</param>
        /// <param name="expression">Expression tree that goes to the property of the model, for which to return the associated ModelState.</param>
        /// <returns>Returns the ModelState associated with the property represented by the expression tree.</returns>
        public static ModelState GetPropertyErrors(this ModelStateDictionary modelState, Expression<Func<object>> expression)
        {
            var propertyInfo = MemberExpressionHelper.GetPropertyInfo(expression);
            var result = modelState[propertyInfo.Name];
            return result;
        }

        /// <summary>
        /// Returns the ModelState object associated with a model property.
        /// </summary>
        /// <param name="modelState">ModelStateDictionary that contains validation messages of the model.</param>
        /// <param name="expression">Expression tree that goes to the property of the model, for which to return the associated ModelState.</param>
        /// <returns>Returns the ModelState associated with the property represented by the expression tree.</returns>
        public static ModelState GetPropertyErrors<TModel>(this ModelStateDictionary modelState, Expression<Func<TModel, object>> expression)
        {
            var propertyInfo = MemberExpressionHelper.GetPropertyInfo(expression);
            var result = modelState[propertyInfo.Name];
            return result;
        }

        /// <summary>
        /// Remove the validation messages and exceptions associated with a model property,
        /// making the value in that property valid.
        /// </summary>
        /// <param name="modelState">ModelState object from which the model property will be removed, and thus be considered as valid.</param>
        /// <param name="expression">Expression tree that goes to the property that should be made valid.</param>
        public static void Remove(this ModelStateDictionary modelState, Expression<Func<object>> expression)
        {
            var propertyInfo = MemberExpressionHelper.GetPropertyInfo(expression);
            modelState.Remove(propertyInfo.Name);
        }
    }
}
