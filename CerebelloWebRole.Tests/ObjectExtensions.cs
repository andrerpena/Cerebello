using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace CerebelloWebRole.Tests
{
    static class ObjectExtensions
    {
        public static string ConvertToString(this object obj, string format)
        {
            StringBuilder result = new StringBuilder();
            var type = obj.GetType();
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (var eachProp in props)
            {
                var propName = eachProp.Name;
                var propValue = eachProp.GetValue(obj, null);
                result.AppendFormat(format, propName, propValue);
            }
            return result.ToString();
        }
    }
}
