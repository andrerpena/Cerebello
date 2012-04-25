using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections;
using System.Linq.Expressions;

namespace CerebelloWebRole.Code
{
    public static class EnumHelper
    {
        public class EnumSelectListItem
        {
            public string Value { get; set; }
            public string Text { get; set; }
        }

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

        public static Dictionary<int, String> GetValueDisplayDictionary(Type aEnumType)
        {
            /// <exception cref="System.ArgumentNullException">Se aEnumType for nulo</exception>
            if (((Object)aEnumType) == null) throw new ArgumentNullException("aEnumType");
            if (!aEnumType.IsEnum)
                throw new Exception("Type must be an Enum");

            Dictionary<int, String> vResult = new Dictionary<int, string>();

            foreach (var vEnumValue in Enum.GetValues(aEnumType))
                vResult.Add((int)vEnumValue, EnumHelper.GetText(vEnumValue));

            return vResult;
        }

        public static List<SelectListItem> GetSelectListItems(Type aEnumType)
        {
            /// <exception cref="System.ArgumentNullException">Se aEnumType for nulo</exception>
            if (((Object)aEnumType) == null) throw new ArgumentNullException("aEnumType");
            if (!aEnumType.IsEnum)
                throw new Exception("Type must be an Enum");

            var result = new List<SelectListItem>();

            foreach (var vEnumValue in Enum.GetValues(aEnumType))
                result.Add(new SelectListItem() { Value = ((int)vEnumValue).ToString(), Text = EnumHelper.GetText(vEnumValue) });

            return result;
        }

        public static Type GetEnumDataTypeFromExpression<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression)
        {
            if (expression.Body.NodeType != ExpressionType.MemberAccess)
                throw new Exception("Expression must represent an object member");

            var vPropertyInfo = PLKExpressionHelper.GetPropertyInfoFromMemberExpression(expression);

            if (vPropertyInfo.PropertyType.IsEnum)
                return vPropertyInfo.PropertyType;
            else
            {
                if (vPropertyInfo.PropertyType != typeof(Int32) && vPropertyInfo.PropertyType != typeof(Nullable<Int32>))
                    throw new Exception("Expression member must be either an enum type or a Int32");

                var vEnumDataTypeAttributes = vPropertyInfo.GetCustomAttributes(typeof(EnumDataTypeAttribute), false);
                if (vEnumDataTypeAttributes.Length == 0)
                    throw new Exception("Expression member must have the EnumDataTypeAttribute when the type is Int32");

                return ((EnumDataTypeAttribute)vEnumDataTypeAttributes[0]).EnumType;
            }
        }
    }
}