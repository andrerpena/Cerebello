using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Linq.Expressions;
using JetBrains.Annotations;

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
            if (@enum == null) throw new ArgumentNullException("enum");
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
            if (((Object)aEnumType) == null) throw new ArgumentNullException("aEnumType");
            if (!aEnumType.IsEnum)
                throw new Exception("Type must be an Enum");

            Dictionary<int, String> vResult = new Dictionary<int, string>();

            foreach (var vEnumValue in Enum.GetValues(aEnumType))
                vResult.Add((int)vEnumValue, EnumHelper.GetText(vEnumValue));

            return vResult;
        }

        /// <summary>
        /// Returns an List<SelectListItem> with enum values out of an enum type
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Se enumType for nulo</exception>
        public static List<SelectListItem> GetSelectListItems([NotNull] Type enumType)
        {
            if (((Object)enumType) == null) throw new ArgumentNullException("enumType");
            if (!enumType.IsEnum)
                throw new Exception("Type must be an Enum");

            var result = new List<SelectListItem>();

            foreach (var vEnumValue in Enum.GetValues(enumType))
                result.Add(new SelectListItem() { Value = ((int)vEnumValue).ToString(), Text = EnumHelper.GetText(vEnumValue) });

            return result;
        }

        /// <summary>
        /// Returns an SelectList with enum values out of an enum type
        /// </summary>
        public static SelectList GetSelectList([NotNull] Type enumType, object selectedValue)
        {
            if (enumType == null) throw new ArgumentNullException("enumType");
            if (!enumType.IsEnum)
                throw new Exception("Type must be an Enum");

            var selectListItems =
                (Enum.GetValues(enumType)
                     .Cast<object>()
                     .Select(ev => new SelectListItem()
                         {
                             Value = ((int)ev).ToString(),
                             Text = EnumHelper.GetText(ev),
                             // To set the "Selected" property here won't take any effect
                         })).ToList();
            return new SelectList(selectListItems, "Value", "Text", selectedValue != null ? selectedValue.ToString() : null);
        }

        /// <summary>
        /// Returns the enum items based on a member expression. This expression has to be either an Int16, Int32 or Int64
        /// with an EnumDataTypeAttribute
        /// </summary>
        public static Type GetEnumDataTypeFromExpression<TModel, TProperty>([NotNull] Expression<Func<TModel, TProperty>> expression)
        {
            if (expression == null) throw new ArgumentNullException("expression");

            if (expression.Body.NodeType != ExpressionType.MemberAccess)
                throw new Exception("Expression must represent an object member");

            Type propertyType = null;
            var propertyInfo = ExpressionHelper.GetPropertyInfoFromMemberExpression(expression);
            if (propertyInfo.PropertyType.IsGenericType)
            {
                var genericType = propertyInfo.PropertyType.GetGenericTypeDefinition();
                if (genericType == typeof(Nullable<>))
                    propertyType = propertyInfo.PropertyType.GetGenericArguments()[0];
            }
            else
            {
                propertyType = propertyInfo.PropertyType;
            }

            var supportedTypes = new HashSet<Type> {
                    typeof(Int32),
                    typeof(Int16),
                    typeof(Int64),
                };

            if (!supportedTypes.Contains(propertyType))
                throw new Exception("Expression member must be of an integer type");

            var vEnumDataTypeAttributes = propertyInfo.GetCustomAttributes(typeof(EnumDataTypeAttribute), false);
            if (vEnumDataTypeAttributes.Length == 0)
                throw new Exception("Expression member must have the EnumDataTypeAttribute when the type is Int32");

            return ((EnumDataTypeAttribute)vEnumDataTypeAttributes[0]).EnumType;
        }
    }
}