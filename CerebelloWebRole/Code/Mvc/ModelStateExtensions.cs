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
    }
}
