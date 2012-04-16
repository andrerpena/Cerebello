using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections;

namespace CerebelloWebRole.Code
{
    public static class EnumHelper
    {
        public class EnumSelectListItem
        {
            public string Value { get; set; }
            public string Text { get; set; }
        }

        //public static SelectList GetSelectList(Type enumType, object selectedValue, bool includeEmptyItem = true)
        //{
        //    if (selectedValue != null)
        //    {
        //        if (selectedValue.GetType().IsEnum)
        //            selectedValue = ((int)selectedValue).ToString();
        //    }

        //    ArrayList list = new ArrayList();
        //    if (includeEmptyItem)
        //        list.Add(new EnumSelectListItem()
        //        {
        //            Value = null,
        //            Text = ""
        //        });

        //    foreach (var value in Enum.GetValues(enumType))
        //    {
        //        list.Add(new EnumSelectListItem
        //        {
        //            Value = ((int)value).ToString(),
        //            Text = EnumHelper.GetText(value)
        //        });
        //    }



        //    return new SelectList(list, "Value", "Text", selectedValue);
        //}

        /// <summary>
        /// Retorna o texto para um objeto enum
        /// </summary>
        ///<exception cref="System.ArgumentNullException">If @enum is null</exception>
        public static String GetText(Object @enum)
        {
            if (@enum == null) throw new ArgumentNullException("@enum");
            var enumType = @enum.GetType();
            if (!enumType.IsEnum)
                throw new ArgumentException("passed object must be an enum");
            var enumValueString = @enum.ToString();
            var enumField = enumType.GetField(enumValueString);

            // pego os atributes do FieldInfo do enum
            var customAttributes = enumField.GetCustomAttributes(typeof(DisplayAttribute), true);
            if (customAttributes == null || customAttributes.Length == 0)
                return enumValueString;
            return (customAttributes[0] as DisplayAttribute).Name;
        }

        ///<exception cref="System.ArgumentNullException">If enumType is null</exception>
        public static String GetText(int? enumValue, Type enumType)
        {
            if (enumValue == null)
                return "";

            if (enumType == null) throw new ArgumentNullException("enumType");
            if (!enumType.IsEnum)
                throw new ArgumentException("passed object must be an enum");

            return EnumHelper.GetText(Enum.ToObject(enumType, enumValue));
        }
    }
}