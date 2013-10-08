using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace CerebelloWebRole.Code
{
    public class CardViewField<TModel, TValue> : CardViewFieldBase
    {
        public Expression<Func<TModel, TValue>> Expression { get; set; }

        public CardViewField(Expression<Func<TModel, TValue>> exp, Func<TModel, object> format = null, string header = null, bool foreverAlone = false)
        {
            Func<dynamic, object> dynFormat = null;
            if (format != null)
                dynFormat = d => format((TModel)d);

            this.Format = dynFormat;
            this.Expression = exp;
            this.Header = header;
            this.WholeRow = foreverAlone;
        }

        public override MvcHtmlString GetLabelText(HtmlHelper htmlHelper)
        {
            return this.Header != null
                ? new MvcHtmlString(this.Header)
                : ((HtmlHelper<TModel>)htmlHelper).LabelFor(this.Expression);
        }

        public override MvcHtmlString GetDisplayText(HtmlHelper htmlHelper)
        {
            // if there's a format, retrieve the format
            if (this.Format != null)
                return new MvcHtmlString(this.Format(htmlHelper.ViewData.Model).ToString());

            // if there's no format but it's a special case, return the special case
            string cellValueInnerHtml = null;
            if (this.Expression != null && htmlHelper.ViewData.Model != null)
            {
                var propertyInfo = (PropertyInfo)((MemberExpression)this.Expression.Body).Member;
                var propertyType = ReflectionHelper.GetTypeOrUnderlyingType(propertyInfo.PropertyType);
                var propertyValue = propertyInfo.GetValue(htmlHelper.ViewData.Model, null);

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
            }

            return ((HtmlHelper<TModel>)htmlHelper).DisplayFor(this.Expression);
        }
    }
}
