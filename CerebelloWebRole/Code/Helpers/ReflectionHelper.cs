using System;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code
{
    public static class ReflectionHelper
    {
        /// <summary>
        /// Returns either the type itself of the underlying type, in case it's a nullable type
        /// </summary>
        public static Type GetTypeOrUnderlyingType([NotNull] Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            var underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType ?? type;
        }
    }
}