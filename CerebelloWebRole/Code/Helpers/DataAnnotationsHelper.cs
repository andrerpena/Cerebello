using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace CerebelloWebRole.Code
{
    public class DataAnnotationsHelper
    {
        public static string GetRegularExpressionPattern<TModel>(Expression<Func<TModel, Object>> expression)
        {
            var propertyInfo = MemberExpressionHelper.GetPropertyInfo(expression);
            var regularExpressionAttribute = propertyInfo.GetCustomAttributes(typeof(RegularExpressionAttribute), true).Cast<RegularExpressionAttribute>().FirstOrDefault();
            if (regularExpressionAttribute == null)
                throw new Exception("Expression member does not have a RegularExpressionAttribute");
            return regularExpressionAttribute.Pattern;
        }

        public static string GetDisplayName<TModel>(Expression<Func<TModel, Object>> expression)
        {
            var propertyInfo = MemberExpressionHelper.GetPropertyInfo(expression);
            var displayAttribute = propertyInfo.GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().FirstOrDefault();
            if (displayAttribute == null)
                return propertyInfo.Name;
            return displayAttribute.Name;
        }
    }
}
