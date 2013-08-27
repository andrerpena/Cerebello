using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CerebelloWebRole.Code
{
    public static class ExpressionHelper
    {
        /// <summary>
        /// Retorna o nome de exibição dado uma expression
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Se expression for nulo</exception>
        public static string GetDisplayName<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression)
        {
            if (((Object)expression) == null) throw new ArgumentNullException("expression");

            var propertyInfo = GetPropertyInfoFromMemberExpression(expression);
            var displayAttribute = propertyInfo.GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().FirstOrDefault();

            return displayAttribute != null ? displayAttribute.Name : propertyInfo.Name;
        }

        public static Expression<Func<T, object>> CleanUpExpression<T>(Expression<Func<T, object>> expression)
        {
            return (Expression<Func<T, object>>)new FlattenExpressionVisitor<T>().Visit(expression);
        }

        /// <summary>
        /// Retorna um PropertyInfo dado uma expression
        /// </summary>
        public static PropertyInfo GetPropertyInfoFromMemberExpression<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression)
        {
            var memberBodyExpression = expression.Body.NodeType == ExpressionType.Convert ? ((UnaryExpression)expression.Body).Operand : expression.Body;
            return (PropertyInfo)((MemberExpression)memberBodyExpression).Member;
        }

        public static string GetExpressionText(LambdaExpression expression)
        {
            return System.Web.Mvc.ExpressionHelper.GetExpressionText(expression);
        }
    }
}
