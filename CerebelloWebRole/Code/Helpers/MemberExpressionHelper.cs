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
        public static PropertyInfo GetPropertyInfo<TModel, TValue>(Expression<Func<TModel, TValue>> expression)
        {
            MemberExpression memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
            {
                UnaryExpression unaryExpression = expression.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }

            var memberName = memberExpression.Member.Name;
            return typeof(TModel).GetProperty(memberName);
        }
    }
}
