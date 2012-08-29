using System;
using System.Linq;

namespace Cerebello.Model
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class TimeZoneDataAttribute : Attribute
    {
        public string Id { get; set; }

        public static TimeZoneDataAttribute GetAttributeFromEnumValue(object enumValue)
        {
            return (TimeZoneDataAttribute)GetCustomAttribute(
                    enumValue.GetType().GetField(enumValue.ToString()),
                    typeof(TimeZoneDataAttribute));
        }
    }
}
