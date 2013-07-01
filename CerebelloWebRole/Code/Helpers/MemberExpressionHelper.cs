using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CerebelloWebRole.Code
{
    public class MemberExpressionHelper
    {
        /// <summary>
        /// Returns the display name of a model property marked with DisplayAttribute.
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static string GetPropertyDisplayName(PropertyInfo propertyInfo)
        {
            var displayAttribute = propertyInfo
                .GetCustomAttributes(typeof(DisplayAttribute), true)
                .Cast<DisplayAttribute>()
                .FirstOrDefault();

            var propertyDisplay = displayAttribute != null ? displayAttribute.Name : propertyInfo.Name;

            return propertyDisplay;
        }

        /// <summary>
        /// Gets the PropertyInfo being returned by and expression tree.
        /// </summary>
        /// <param name="expression">Expression tree that returns a property.</param>
        /// <returns>The PropertyInfo that the expression tree returns.</returns>
        public static PropertyInfo GetPropertyInfo(LambdaExpression expression)
        {
            MemberExpression memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
            {
                UnaryExpression unaryExpression = expression.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }

            return (PropertyInfo)memberExpression.Member;
        }
    }
}
