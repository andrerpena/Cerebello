using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Code.Mvc
{
    public static class ModelStateExtensions
    {
        public static void AddModelError<TModel>(this ModelStateDictionary modelState, Expression<Func<TModel, object>> expression, string errorMessage)
        {
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
