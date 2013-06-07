using System;
using System.Collections.Generic;

namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Equality comparer capable of comparing continuous sets.
    /// </summary>
    /// <typeparam name="T">Type of the elements in the set.</typeparam>
    public class ContinuousSetEqualityComparer<T> : IEqualityComparer<ContinuousSet<T>>
    {
        private readonly bool ignoreRedundantFalses;
        private readonly bool ignoreRedundantTrues;
        private readonly IEqualityComparer<T> eqT;

        private static ContinuousSetEqualityComparer<T> cmpIgnoreRedundantFalses;

        /// <summary>
        /// Gets an equality comparer that ignores redundant negative items in the set.
        /// </summary>
        public static ContinuousSetEqualityComparer<T> IgnoreRedundantFalses
        {
            get
            {
                if (cmpIgnoreRedundantFalses != null)
                    return cmpIgnoreRedundantFalses;

                cmpIgnoreRedundantFalses = new ContinuousSetEqualityComparer<T>(null, true, false);
                return cmpIgnoreRedundantFalses;
            }
        }

        private static ContinuousSetEqualityComparer<T> cmpIgnoreRedundantTrues;

        /// <summary>
        /// Gets an equality comparer that ignores redundant positive items in the set.
        /// </summary>
        public static ContinuousSetEqualityComparer<T> IgnoreRedundantTrues
        {
            get
            {
                if (cmpIgnoreRedundantTrues != null)
                    return cmpIgnoreRedundantTrues;

                cmpIgnoreRedundantTrues = new ContinuousSetEqualityComparer<T>(null, false, true);
                return cmpIgnoreRedundantTrues;
            }
        }

        private static ContinuousSetEqualityComparer<T> cmpIgnoreRedundant;

        /// <summary>
        /// Gets an equality comparer that ignores redundant items in the set.
        /// </summary>
        public static ContinuousSetEqualityComparer<T> IgnoreRedundant
        {
            get
            {
                if (cmpIgnoreRedundant != null)
                    return cmpIgnoreRedundant;

                cmpIgnoreRedundant = new ContinuousSetEqualityComparer<T>(null, true, true);
                return cmpIgnoreRedundant;
            }
        }

        private static ContinuousSetEqualityComparer<T> cmpDefault;

        /// <summary>
        /// Gets an equality comparer that compares all elements in the set, including the redundant ones.
        /// </summary>
        public static ContinuousSetEqualityComparer<T> Default
        {
            get
            {
                if (cmpDefault != null)
                    return cmpDefault;

                cmpDefault = new ContinuousSetEqualityComparer<T>(null, false, false);
                return cmpDefault;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinuousSetEqualityComparer{T}"/> class.
        /// </summary>
        public ContinuousSetEqualityComparer()
        {
            this.ignoreRedundantFalses = false;
            this.ignoreRedundantTrues = false;
            this.eqT = EqualityComparer<T>.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinuousSetEqualityComparer{T}"/> class.
        /// </summary>
        /// <param name="equalityComparer">
        /// The equality comparer used to determine whether items in the sets are equal or not, and to get hashes of these elements.
        /// </param>
        /// <param name="ignoreRedundantFalses">
        /// Whether to ignore redundant negative items in the sets being compared or not.
        /// </param>
        /// <param name="ignoreRedundantTrues">
        /// Whether to ignore redundant positive items in the sets being compared or not.
        /// </param>
        public ContinuousSetEqualityComparer(IEqualityComparer<T> equalityComparer, bool ignoreRedundantFalses = false, bool ignoreRedundantTrues = false)
        {
            this.ignoreRedundantFalses = ignoreRedundantFalses;
            this.ignoreRedundantTrues = ignoreRedundantTrues;
            this.eqT = equalityComparer ?? EqualityComparer<T>.Default;
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        /// <param name="x">The first object of type <paramref name="T"/> to compare.</param><param name="y">The second object of type <paramref name="T"/> to compare.</param>
        public bool Equals(ContinuousSet<T> x, ContinuousSet<T> y)
        {
            return ContinuousSet<T>.Equals(x, y, this.ignoreRedundantFalses, this.ignoreRedundantTrues);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <param name="obj">The object for which a hash code is to be returned.</param>
        /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        public int GetHashCode(ContinuousSet<T> obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            return ContinuousSet<T>.GetHashCode(this.eqT, obj, this.ignoreRedundantFalses, this.ignoreRedundantTrues);
        }
    }
}