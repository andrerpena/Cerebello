using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Web.Mvc.Html;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.Code;

namespace CommonLib.Mvc
{
    public static class HtmlExtensions
    {
        public static MvcHtmlString EnumDropdownFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var memberName = (expression.Body as MemberExpression).Member.Name;
            var propertyInfo = typeof(TModel).GetProperty(memberName);

            Type enumType = null;

            if (propertyInfo.PropertyType.IsEnum)
            {
                enumType = propertyInfo.PropertyType;
            }
            else
            {
                var attributes = propertyInfo.GetCustomAttributes(typeof(EnumDataTypeAttribute), true);
                if (attributes == null || attributes.Length == 0)
                    throw new Exception("cannot resolve enum type");

                enumType = (attributes[0] as EnumDataTypeAttribute).EnumType;
                if (enumType == null)
                    throw new Exception("cannot resolve enum type");
            }

            var enumValues = Enum.GetValues(enumType);
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var value in enumValues)
                items.Add(new SelectListItem() { Value = ((int)value).ToString(), Text = EnumHelper.GetText(value) });

            return SelectExtensions.DropDownList(html, memberName, items, "");
        }

        public static MvcHtmlString EnumDisplayFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var propertyInfo = MemberExpressionHelper.GetPropertyInfo(expression);

            Type enumType = null;

            if (propertyInfo.PropertyType.IsEnum)
            {
                enumType = propertyInfo.PropertyType;
            }
            else
            {
                var attributes = propertyInfo.GetCustomAttributes(typeof(EnumDataTypeAttribute), true);
                if (attributes == null || attributes.Length == 0)
                    throw new Exception("cannot resolve enum type");

                enumType = (attributes[0] as EnumDataTypeAttribute).EnumType;
                if (enumType == null)
                    throw new Exception("cannot resolve enum type");
            }

            var model = (html.ViewContext.View as WebViewPage).Model;
            var modelValue = propertyInfo.GetValue(model, null);

            if(modelValue == null)
                return new MvcHtmlString("");

            return new MvcHtmlString(EnumHelper.GetText((int)modelValue, enumType));
        }
    }
}