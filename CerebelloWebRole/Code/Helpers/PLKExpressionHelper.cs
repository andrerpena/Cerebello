using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Code
{
    public static class PLKExpressionHelper
    {
        /// <summary>
        /// Retorna o nome de exibição dado uma expression
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Se expression for nulo</exception>
        public static string GetDisplayName<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression)
        {
			if (((Object) expression) == null)throw new ArgumentNullException("expression");

            var propertyInfo = GetPropertyInfoFromMemberExpression(expression);
            var displayAttribute = propertyInfo.GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().FirstOrDefault();

            return displayAttribute != null ? displayAttribute.Name : propertyInfo.Name;
        }

        /// <summary>
        /// Retorna um PropertyInfo dado uma expression
        /// </summary>
        public static PropertyInfo GetPropertyInfoFromMemberExpression<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression)
        {
            Expression memberBodyExpression = null;

            if (expression.Body.NodeType == ExpressionType.Convert)
                memberBodyExpression = ((System.Linq.Expressions.UnaryExpression)expression.Body).Operand;
            else
                memberBodyExpression = (MemberExpression)expression.Body;

            return (PropertyInfo)((MemberExpression)memberBodyExpression).Member;
        }
    }
}
