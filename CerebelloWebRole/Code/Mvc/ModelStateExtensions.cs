using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code
{
    public static class ModelStateExtensions
    {
        [StringFormatMethod("errorMessage")]
        public static void AddModelError(
            this ModelStateDictionary modelState,
            string key,
            [Localizable(true)] string errorMessage,
            params object[] formatValues)
        {
            // todo: this method should accept a resource name, instead of an error message.
            errorMessage = string.Format(errorMessage, formatValues);
            modelState.AddModelError(key, errorMessage);
        }

        [StringFormatMethod("errorMessage")]
        public static void AddModelError(
            this ModelStateDictionary modelState,
            Expression<Func<object>> expression,
            [Localizable(true)] string errorMessage,
            params object[] formatValues)
        {
            // todo: this method should accept a resource name, instead of an error message.
            errorMessage = string.Format(errorMessage, formatValues);
            modelState.AddModelError(expression, errorMessage);
        }

        public static void AddModelError<TModel>(
            this ModelStateDictionary modelState,
            Expression<Func<TModel, object>> expression,
            string errorMessage)
        {
            // todo: this method should accept a resource name, instead of an error message.

            var propertyInfo = MemberExpressionHelper.GetPropertyInfo(expression);

            var propertyDisplay = MemberExpressionHelper.GetPropertyDisplayName(propertyInfo);
            errorMessage = string.Format(errorMessage, propertyDisplay);

            modelState.AddModelError(propertyInfo.Name, errorMessage);
        }

        /// <summary>
        /// Add a new validation message to the collection of messages and exceptions associated with the given model property.
        /// </summary>
        /// <param name="modelState">ModelState objeto to add the validation message to.</param>
        /// <param name="expression">Expression tree that goes to the property that is not valid.</param>
        /// <param name="errorMessage">Validation message to associate with the property.</param>
        public static void AddModelError(this ModelStateDictionary modelState, Expression<Func<object>> expression, [Localizable(true)] string errorMessage)
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
        /// <returns>
        /// Returns the ModelState associated with the property represented by the expression tree.
        /// If there is no ModelState associated, then returns null.
        /// </returns>
        public static ModelErrorCollection GetPropertyErrors(this ModelStateDictionary modelState, Expression<Func<object>> expression)
        {
            var propertyInfo = MemberExpressionHelper.GetPropertyInfo(expression);
            var result = modelState[propertyInfo.Name];

            if (result == null)
                return null;

            return result.Errors;
        }

        public static bool HasPropertyErrors(this ModelStateDictionary modelState, Expression<Func<object>> expression)
        {
            return GetPropertyErrors(modelState, expression).Any();
        }

        /// <summary>
        /// Returns the ModelState object associated with a model property.
        /// </summary>
        /// <param name="modelState">ModelStateDictionary that contains validation messages of the model.</param>
        /// <param name="expression">Expression tree that goes to the property of the model, for which to return the associated ModelState.</param>
        /// <returns>Returns the ModelState associated with the property represented by the expression tree.</returns>
        public static ModelErrorCollection GetPropertyErrors<TModel>(this ModelStateDictionary modelState, Expression<Func<TModel, object>> expression)
        {
            var propertyInfo = MemberExpressionHelper.GetPropertyInfo(expression);
            var result = modelState[propertyInfo.Name];

            if (result == null)
                return null;

            return result.Errors;
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

        /// <summary>
        /// Remove the validation messages indicated by a predicate.
        /// </summary>
        /// <param name="modelState">ModelState object from which the model property will be removed, and thus be considered as valid.</param>
        /// <param name="predicate">ModelError predicate that indicates which errors to remove.</param>
        /// <returns>True if any items were removed. Otherwise False.</returns>
        public static bool Remove(this ModelStateDictionary modelState, Predicate<ModelError> predicate)
        {
            bool anyRemoved = false;
            foreach (var modelStateItem in modelState.ToArray())
            {
                foreach (var eachModelError in modelStateItem.Value.Errors.ToArray())
                    if (predicate(eachModelError))
                    {
                        modelStateItem.Value.Errors.Remove(eachModelError);
                        anyRemoved = true;
                    }

                if (!modelStateItem.Value.Errors.Any())
                    modelState.Remove(modelStateItem);
            }
            return anyRemoved;
        }

        /// <summary>
        /// Clears the validation messages and exceptions associated with a model property,
        /// making the value in that property valid.
        /// </summary>
        /// <param name="modelState">ModelState object from which the model property will be removed, and thus be considered as valid.</param>
        /// <param name="expression">Expression tree that goes to the property that should be made valid.</param>
        public static void ClearPropertyErrors(this ModelStateDictionary modelState, Expression<Func<object>> expression)
        {
            var propertyInfo = MemberExpressionHelper.GetPropertyInfo(expression);
            if (modelState.ContainsKey(propertyInfo.Name))
                modelState[propertyInfo.Name].Errors.Clear();
        }

        /// <summary>
        /// Removes duplicated messages from the modelState.
        /// Same messages in different properties are considered duplicates.
        /// This should be last thing to do on a ModelState object.
        /// </summary>
        /// <param name="modelState">The model state object to remove duplicates from.</param>
        /// <param name="ignoreCase">Whether case is considered when looking for duplicates.</param>
        /// <returns>Returns the number of removed items.</returns>
        public static int RemoveDuplicates(this ModelStateDictionary modelState, bool ignoreCase = false)
        {
            var foundMessages = new HashSet<string>(ignoreCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture);

            int countRemoved = 0;
            foreach (var modelStateItem in modelState.ToArray())
            {
                foreach (var eachModelError in modelStateItem.Value.Errors.ToArray())
                {
                    if (foundMessages.Contains(eachModelError.ErrorMessage))
                    {
                        modelStateItem.Value.Errors.Remove(eachModelError);
                        countRemoved++;
                    }

                    foundMessages.Add(eachModelError.ErrorMessage);
                }
            }

            return countRemoved;
        }


        /// <summary>
        /// Flatten all model errors in a single list of tuples containing the property name and the ModelError object.
        /// </summary>
        /// <param name="modelState">ModelStateDictionary to flatten.</param>
        /// <returns>A single flattened list of all model errors.</returns>
        public static List<Tuple<string, ModelError>> GetAllErrors(this ModelStateDictionary modelState)
        {
            var result = new List<Tuple<string, ModelError>>();

            foreach (var eachModelState in modelState)
                foreach (var eachModelError in eachModelState.Value.Errors)
                    result.Add(new Tuple<string, ModelError>(eachModelState.Key, eachModelError));

            return result;
        }

        /// <summary>
        /// Creates a string containing the description of all errors in the list.
        /// </summary>
        /// <param name="flatErrorList">List of model errors to convert to text.</param>
        /// <returns>A text containing all errors.</returns>
        public static string TextMessage(this List<Tuple<string, ModelError>> flatErrorList)
        {
            return string.Join("\n", flatErrorList.Select(x => string.Format("{0}: {1}", x.Item1, x.Item2)));
        }
    }
}
