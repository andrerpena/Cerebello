using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cerebello.Firestarter
{
    /// <summary>
    /// Extension methods for the Type type.
    /// </summary>
    public static class TypeExtensions
    {
        private static readonly Dictionary<Type, List<Type>> dict = new Dictionary<Type, List<Type>>() {
            { typeof(decimal), new List<Type> { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(char) } },
            { typeof(double), new List<Type> { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(char), typeof(float) } },
            { typeof(float), new List<Type> { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(char), typeof(float) } },
            { typeof(ulong), new List<Type> { typeof(byte), typeof(ushort), typeof(uint), typeof(char) } },
            { typeof(long), new List<Type> { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(char) } },
            { typeof(uint), new List<Type> { typeof(byte), typeof(ushort), typeof(char) } },
            { typeof(int), new List<Type> { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(char) } },
            { typeof(ushort), new List<Type> { typeof(byte), typeof(char) } },
            { typeof(short), new List<Type> { typeof(byte) } }
        };

        /// <summary>
        /// Gets information about the given type cast.
        /// </summary>
        /// <param name="from">The source type of the cast.</param>
        /// <param name="to">The destination type of the cast.</param>
        /// <returns>Returns the type cast kind.</returns>
        public static TypeCastIs IsCastableTo(this Type from, Type to)
        {
            if (to == from)
                return TypeCastIs.NotNeeded;
            if (to.IsAssignableFrom(from))
                return TypeCastIs.Covariant;
            if (from.IsAssignableFrom(to))
                return TypeCastIs.Contravariant;
            if (dict.ContainsKey(to) && dict[to].Contains(from))
                return TypeCastIs.BuiltInImplicit;
            if (dict.ContainsKey(from) && dict[from].Contains(to))
                return TypeCastIs.BuiltInExplicit;
            var fromMethods = from.GetMethods(BindingFlags.Public | BindingFlags.Static);
            var toMethods = to.GetMethods(BindingFlags.Public | BindingFlags.Static);
            bool implicitCast = fromMethods.Any(m => m.ReturnType == to && m.Name == "op_Implicit")
                || toMethods.Any(m => m.ReturnType == from && m.Name == "op_Implicit");
            bool explicitCast = fromMethods.Any(m => m.ReturnType == to && m.Name == "op_Explicit")
                || toMethods.Any(m => m.ReturnType == from && m.Name == "op_Explicit");
            if (implicitCast) return TypeCastIs.CustomImplicit;
            if (explicitCast) return TypeCastIs.CustomExplicit;
            return TypeCastIs.Impossible;
        }
    }
}
