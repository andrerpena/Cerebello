using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Helpers;
using System.Web.Mvc;

namespace CerebelloWebRole.Code
{
    public class GridField<TModel, TValue> : GridFieldBase
    {
        public Expression<Func<TModel, TValue>> Expression { get; set; }

        /// <summary>
        /// Gets the header for a column
        /// </summary>
        /// <returns></returns>
        public override string GetColumnHeader()
        {
            // if the header is specified, return it.
            if (this.Header != null)
                return this.Header;

            if (this.Expression == null)
                return string.Empty;

            // if the header
            var propertyInfo = (PropertyInfo)((MemberExpression)this.Expression.Body).Member;

            string columnHeader = null;

            var displayAttribute = propertyInfo.GetCustomAttributes(true)
                .OfType<DisplayAttribute>()
                .FirstOrDefault();

            if (displayAttribute != null)
                columnHeader = displayAttribute.Name;

            if (string.IsNullOrEmpty(columnHeader))
                columnHeader = propertyInfo.Name;

            return columnHeader;
        }

        public override Func<dynamic, object> GetDisplayTextFunction(HtmlHelper htmlHelper)
        {
            // if there's a format, retrieve the format
            if (this.Format != null)
                return this.Format;

            var propertyInfo = (PropertyInfo)((MemberExpression)this.Expression.Body).Member;
            var propertyType = ReflectionHelper.GetTypeOrUnderlyingType(propertyInfo.PropertyType);

            Func<dynamic, object> format = webGridRow =>
                {
                    var propertyValue = propertyInfo.GetValue(((WebGridRow)webGridRow).Value);
                    if (propertyValue != null)
                    {
                        // each types have particularities
                        // numbers, when they have an EnumDataTypeAttribute, will display a text
                        if (propertyType == typeof(Int16) || propertyType == typeof(Int32) || propertyType == typeof(Int64))
                        {
                            // if it's an integer, we have to see whether it has 
                            var enumAttribute = propertyInfo.GetCustomAttribute<EnumDataTypeAttribute>();
                            if (enumAttribute != null)
                                return new MvcHtmlString(Enum.ToObject(enumAttribute.EnumType, propertyValue).ToString());
                        }
                                            // date-times will appear as short date string
                    if (propertyType == typeof(DateTime))
                        return new MvcHtmlString(((DateTime)propertyValue).ToShortDateString());
                    }
                    return propertyValue;
                };
            return format;
        }
    }
}
