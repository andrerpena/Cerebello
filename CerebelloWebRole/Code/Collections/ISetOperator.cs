using System.Collections.Generic;

namespace CerebelloWebRole.Code.Collections
{
    /// <summary>
    /// Interface of a set operator helper,
    /// used to change the result of an operation,
    /// and to merge the resulting items into the final set.
    /// </summary>
    /// <typeparam name="T">Type of keys in the set.</typeparam>
    public interface ISetOperator<in T>
    {
        /// <summary>
        /// Invokes the operator passing all operands.
        /// </summary>
        /// <param name="bools">Boolean operands to operate.</param>
        /// <param name="opIndex">Index of the current operation. Odds are always points, evens are always intervals.</param>
        /// <param name="result">Reference to the boolean result of the operation, to be changed, or left as is.</param>
        void Invoke(bool[] bools, int opIndex, ref bool result);

        /// <summary>
        /// Gets a value indicating whether these results should be kept in the set or not.
        /// </summary>
        /// <param name="itemsInGroup">The items in the current operation group.</param>
        /// <param name="beforeIncluded">The resulting beforeIncluded value.</param>
        /// <param name="selfIncluded">The resulting selfIncluded value.</param>
        /// <param name="nextIncluded">The resulting nextIncluded value.</param>
        /// <returns>Returns true if these results should be kept in the final resulting set.</returns>
        bool KeepInSet(IEnumerable<ContinuousSet.IItem<T>> itemsInGroup, bool beforeIncluded, bool selfIncluded, bool nextIncluded);
    }
}