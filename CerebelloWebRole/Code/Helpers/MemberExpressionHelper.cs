using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace CerebelloWebRole.Code
{
    public class MemberExpressionHelper
    {
#warning This method is strange... see if this is really needed.
        public static PropertyInfo GetPropertyInfo<TModel, TValue>(Expression<Func<TModel, TValue>> expression)
        {
            MemberExpression memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
            {
                UnaryExpression unaryExpression = expression.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }

#warning This is the strange part, why not just type-cast 'memberExpression.Member' to PropertyInfo? TModel would be dismissed that way.
            var memberName = memberExpression.Member.Name;
            return typeof(TModel).GetProperty(memberName);
        }

        /// <summary>
        /// Gets the PropertyInfo being returned by and expression tree.
        /// </summary>
        /// <param name="expression">Expression tree that returns a property.</param>
        /// <returns>The PropertyInfo that the expression tree returns.</returns>
        public static PropertyInfo GetPropertyInfo(Expression<Func<object>> expression)
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
