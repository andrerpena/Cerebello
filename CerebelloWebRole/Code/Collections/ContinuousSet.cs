using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code.Collections
{
    /// <summary>
    /// Represents a set of elements in a continuous space, in contrast with a set in a discrete space such as the HashSet.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The type of elements in a continuous set must be comparable to each other,
    /// such that it exists a continuity between elements.
    /// You can pass an IComparer of the type to the constructor if a default one does not exist.
    /// </para>
    /// <para>
    /// When calculating the hash of the set, hashes of elements are calculated using the default EqualityComparer.
    /// Comparing two sets using the Equals method does not use EqualityComparer, instead it uses an IComparer.
    /// </para>
    /// <para>
    /// When using the Transform method, the elements must be comparable to the default(T) value,
    /// and also, the transformation delegate must be abled to transform the value of default(T).
    /// </para>
    /// </remarks>
    /// <typeparam name="T">Type of elements in the set.</typeparam>
    public class ContinuousSet<T>
    {
        private static readonly ItemComparer itemComparer = new ItemComparer(Comparer<T>.Default);

        private readonly List<Item> inclusionList;
        private readonly List<bool> lessInfinityOperands;
        private readonly ItemComparer comparer;
        private int lastOperand;
        private Func<bool[], bool> flattener;

        /// <summary>
        /// Represents a point in the continuous set. It has a key indicating the position of the point in the set,
        /// and 3 boolean values, indicating whether the interval before is included, whether itself is included,
        /// and whether the interval after it is included in the set.
        /// </summary>
        [DebuggerDisplay("Key={Key}; {VisualSequence}")]
        public struct Item : ContinuousSet.IItem<T>
        {
            private readonly byte hasValue;
            private readonly byte beforeIncluded;
            private readonly byte selfIncluded;
            private readonly byte afterIncluded;
            private readonly int operandId;
            private readonly T key;

            /// <summary>
            /// Initializes a new instance of the <see cref="Item"/> struct. This represents a new point to place in the set.
            /// </summary>
            /// <param name="key">The key of this item, representing it's position. </param>
            /// <param name="operandId">The operand Id, in case this item is an operand to be operated by an operator.</param>
            /// <param name="beforeIncluded">Whether the preceding interval, before this point is included in the set. </param>
            /// <param name="selfIncluded">Whether this point itself is included in the set.</param>
            /// <param name="afterIncluded">Whether the proceeding interval, after this point is included in the set.</param>
            public Item(T key, int operandId, bool beforeIncluded, bool selfIncluded, bool afterIncluded)
            {
                this.beforeIncluded = (byte)(beforeIncluded ? 1 : 0);
                this.selfIncluded = (byte)(selfIncluded ? 1 : 0);
                this.afterIncluded = (byte)(afterIncluded ? 1 : 0);
                this.operandId = operandId;
                this.key = key;
                this.hasValue = 1;
            }

            /// <summary>
            /// Gets a value indicating whether the preceding interval, before this point is included in the set.
            /// </summary>
            public bool BeforeIncluded
            {
                get { return this.beforeIncluded != 0; }
            }

            /// <summary>
            /// Gets a value indicating whether this point itself is included in the set.
            /// </summary>
            public bool SelfIncluded
            {
                get { return this.selfIncluded != 0; }
            }

            /// <summary>
            /// Gets a value indicating whether the proceeding interval, after this point is included in the set.
            /// </summary>
            public bool AfterIncluded
            {
                get { return this.afterIncluded != 0; }
            }

            /// <summary>
            /// Gets the Id of the operand in a multilayer set, awaiting for the list of operands to be flattened by an operator.
            /// </summary>
            public int OperandId
            {
                get { return this.operandId; }
            }

            /// <summary>
            /// Gets the key represented by this point.
            /// This is the position of this point inside the set.
            /// </summary>
            public T Key
            {
                get { return this.key; }
            }

            /// <summary>
            /// Gets a value indicating whether this point
            /// </summary>
            public bool IsEmpty
            {
                get { return this.hasValue == 0; }
            }

            /// <summary>
            /// Gets a value indicating whether this item is redundant or not.
            /// A redundant item has got all of it's components set to the same value,
            /// that is the values of BeforeIncluded, SelfIncluded and AfterIncluded are equal.
            /// </summary>
            public bool IsRedundant
            {
                get { return this.beforeIncluded == this.selfIncluded && this.selfIncluded == this.afterIncluded; }
            }

            /// <summary>
            /// Indicates whether this item should be removed after the flattening of a set.
            /// </summary>
            /// <param name="item">Item to be tested.</param>
            /// <returns>Returns true to remove the item; otherwise false.</returns>
            public static bool CleanAfterFlattening(Item item)
            {
                return item.operandId != 0 || item.IsEmpty;
            }

            /// <summary>
            /// Indicates whether this item should be removed after the flattening of a set.
            /// This method indicates that redundant items should be removed.
            /// </summary>
            /// <param name="item">Item to be tested.</param>
            /// <returns>Returns true to remove the item; otherwise false.</returns>
            public static bool CleanMergeAfterFlattening(Item item)
            {
                return item.operandId != 0 || item.IsEmpty || item.IsRedundant;
            }

            /// <summary>
            /// Indicates whether this item should be removed after the flattening of a set.
            /// This method indicates that redundant negative items should be removed.
            /// </summary>
            /// <param name="item">Item to be tested.</param>
            /// <returns>Returns true to remove the item; otherwise false.</returns>
            public static bool CleanMergeFalsesAfterFlattening(Item item)
            {
                return item.operandId != 0 || item.IsEmpty || item.IsRedundant && item.selfIncluded == 0;
            }

            /// <summary>
            /// Indicates whether this item should be removed after the flattening of a set.
            /// This method indicates that redundant positive items should be removed.
            /// </summary>
            /// <param name="item">Item to be tested.</param>
            /// <returns>Returns true to remove the item; otherwise false.</returns>
            public static bool CleanMergeTruesAfterFlattening(Item item)
            {
                return item.operandId != 0 || item.IsEmpty || item.IsRedundant && item.selfIncluded == 1;
            }

            /// <summary>
            /// Gets a value representing a void item, that does not influence the set that contains it.
            /// </summary>
            // ReSharper disable StaticFieldInGenericType
            public static readonly Item Empty = new Item();
            // ReSharper restore StaticFieldInGenericType

            /// <summary>
            /// Gets a visual representation of the interval for debugging purposes.
            /// </summary>
            [UsedImplicitly]
            internal string VisualSequence
            {
                get { return (this.BeforeIncluded ? "█" : "─") + (this.SelfIncluded ? "█" : "─") + (this.AfterIncluded ? "█" : "─"); }
            }
        }

        /// <summary>
        /// Comparer used to sort the items in the set before calculating the results, when flattening the set.
        /// </summary>
        public class ItemComparer : IComparer<Item>
        {
            private readonly IComparer<T> keyComparer;

            /// <summary>
            /// Constructs a new ItemComparer given the internal key comparer to use.
            /// </summary>
            /// <param name="keyComparer">Comparer used to compare the keys of the items.</param>
            public ItemComparer(IComparer<T> keyComparer)
            {
                this.keyComparer = keyComparer;
            }

            /// <summary>
            /// Gets the comparer used to compare the keys of the items.
            /// </summary>
            public IComparer<T> KeyComparer
            {
                get { return this.keyComparer; }
            }

            /// <summary>
            /// Compares two items, indicating whether the relative order between them.
            /// </summary>
            /// <param name="x">First item in the comparison.</param>
            /// <param name="y">Second item in the comparison.</param>
            /// <returns>Returns 0 if both items are equivalent; 1 if second is greater than first; -1 if the first is greater than second.</returns>
            public int Compare(Item x, Item y)
            {
                var cmp1 = this.KeyComparer.Compare(x.Key, y.Key);
                if (cmp1 != 0) return cmp1;
                var cmp2 = Comparer<int>.Default.Compare(x.OperandId, y.OperandId);
                if (cmp2 != 0) return cmp2;
                return 0;
            }
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinuousSet{T}"/> class.
        /// The new set will be empty, that is without any elements.
        /// </summary>
        public ContinuousSet()
        {
            this.inclusionList = new List<Item>();
            this.comparer = itemComparer;
            this.flattener = ContinuousSet.OpUnion;
            this.lastOperand = 0;
            this.lessInfinityOperands = new List<bool> { false };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinuousSet{T}"/> class.
        /// The new set will be empty, or completely filled of elements depending on 'outerSpaceIncluded' parameter.
        /// </summary>
        /// <param name="outerSpaceIncluded"> Indicates whether the set is completely empty or filled of elements. </param>
        public ContinuousSet(bool outerSpaceIncluded)
        {
            this.inclusionList = new List<Item>();
            this.comparer = itemComparer;
            this.flattener = ContinuousSet.OpUnion;
            this.lastOperand = 0;
            this.lessInfinityOperands = new List<bool> { outerSpaceIncluded };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinuousSet{T}"/> class, with the internal given capacity for new points.
        /// The new set will be empty, or completely filled of elements depending on 'outerSpaceIncluded' parameter.
        /// </summary>
        /// <param name="capacity">Internal memory capacity of this set. This is useful in optimizing memory allocations by pre-allocating the given capacity.</param>
        /// <param name="outerSpaceIncluded"> Indicates whether the set is completely empty or filled of elements. </param>
        public ContinuousSet(int capacity, bool outerSpaceIncluded = false)
        {
            this.inclusionList = new List<Item>(capacity);
            this.comparer = itemComparer;
            this.flattener = ContinuousSet.OpUnion;
            this.lastOperand = 0;
            this.lessInfinityOperands = new List<bool> { outerSpaceIncluded };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinuousSet{T}"/> class, with the given comparer.
        /// The new set will be empty, or completely filled of elements depending on 'outerSpaceIncluded' parameter.
        /// </summary>
        /// <param name="comparer"> The comparer that defines the continuous space of this set. </param>
        /// <param name="outerSpaceIncluded"> Indicates whether the set is completely empty or filled of elements. </param>
        public ContinuousSet(IComparer<T> comparer, bool outerSpaceIncluded = false)
        {
            this.inclusionList = new List<Item>();
            this.comparer = new ItemComparer(comparer ?? Comparer<T>.Default);
            this.flattener = ContinuousSet.OpUnion;
            this.lastOperand = 0;
            this.lessInfinityOperands = new List<bool> { outerSpaceIncluded };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinuousSet{T}"/> class by cloning another existing one.
        /// </summary>
        /// <param name="toClone"> The other set to clone. </param>
        /// <param name="additionalCapacity"> The additional capacity to add to this set, in case you need to add more elements. </param>
        public ContinuousSet(ContinuousSet<T> toClone, int additionalCapacity = 0)
        {
            this.inclusionList = new List<Item>(toClone.inclusionList.Count + additionalCapacity);
            this.inclusionList.AddRange(toClone.inclusionList);
            this.comparer = toClone.comparer;
            this.flattener = toClone.flattener;
            this.lastOperand = toClone.lastOperand;
            this.lessInfinityOperands = new List<bool>(toClone.lessInfinityOperands);
        }

        #endregion

        /// <summary>
        /// Gets or sets an operator used to flatten the set, after adding multiple operands, by using the Add methods.
        /// </summary>
        public Func<bool[], bool> Operator
        {
            get { return this.flattener; }
            set { this.flattener = value; }
        }

        /// <summary>
        /// Gets or sets the internal memory capacity of elements in this set.
        /// </summary>
        public int Capacity
        {
            get { return this.inclusionList.Capacity; }
            set { this.inclusionList.Capacity = value; }
        }

        /// <summary>
        /// Adds a single point to this set.
        /// Note that after adding, you will need to flatten the set to apply the merging operator.
        /// </summary>
        /// <param name="position">Position of the item, that is used as key in this set.</param>
        /// <param name="included">Whether the item is considered to be in the set, or out of the set.</param>
        public void AddPoint(T position, bool included = true)
        {
            this.lastOperand++;
            this.inclusionList.Add(new Item(position, this.lastOperand, !included, included, !included));
            this.lessInfinityOperands.Add(!included);
        }

        /// <summary>
        /// Adds a single point to this set.
        /// Note that after adding, you will need to flatten the set to apply the merging operator.
        /// </summary>
        /// <param name="position">Position of the item, that is used as key in this set.</param>
        /// <param name="beforeIncluded">Whether the elements before position are considered to be in the set, or out of the set.</param>
        /// <param name="selfIncluded">Whether the element at position is considered to be in the set, or out of the set.</param>
        /// <param name="afterIncluded">Whether the elements after position are considered to be in the set, or out of the set.</param>
        public void AddPoint(T position, bool beforeIncluded, bool selfIncluded, bool afterIncluded)
        {
            this.lastOperand++;
            this.inclusionList.Add(new Item(position, this.lastOperand, beforeIncluded, selfIncluded, afterIncluded));
            this.lessInfinityOperands.Add(beforeIncluded);
        }

        /// <summary>
        /// Adds an interval to this set.
        /// Note that after adding, you will need to flatten the set to apply the merging operator.
        /// </summary>
        /// <param name="start">Starting position of the interval.</param>
        /// <param name="startIncluded">Whether start element is included or not in the set.</param>
        /// <param name="end">End position of the interval.</param>
        /// <param name="endIncluded">Whether the end element is included or not in the set.</param>
        public void AddInterval(T start, bool startIncluded, T end, bool endIncluded)
        {
            var compStartEnd = this.comparer.KeyComparer.Compare(start, end);
            if (compStartEnd >= 0)
                throw new ArgumentException("'start' must come before 'end'");

            this.lastOperand++;

            this.inclusionList.Add(new Item(start, this.lastOperand, false, startIncluded, true));
            this.inclusionList.Add(new Item(end, this.lastOperand, true, endIncluded, false));
            this.lessInfinityOperands.Add(false);
        }

        /// <summary>
        /// Adds an interval to this set.
        /// Note that after adding, you will need to flatten the set to apply the merging operator.
        /// </summary>
        /// <param name="interval">Interval to be added to this set.</param>
        public void AddInterval(Interval<T> interval)
        {
            var compStartEnd = this.comparer.KeyComparer.Compare(interval.Start, interval.End);
            if (compStartEnd > 0)
                throw new ArgumentException("'Start' must come before 'End', the interval is invalid.");

            this.lastOperand++;

            if (interval.IsSinglePoint)
            {
                this.inclusionList.Add(new Item(interval.Start, this.lastOperand, interval.IsLessInfinityIncluded, interval.SinglePointState == PointState.Included, interval.IsPlusInfinityIncluded));
            }
            else
            {
                if (interval.StartState != PointState.Open)
                    this.inclusionList.Add(new Item(interval.Start, this.lastOperand, interval.IsLessInfinityIncluded, interval.StartState == PointState.Included, interval.IsPositive));
                if (interval.EndState != PointState.Open)
                    this.inclusionList.Add(new Item(interval.End, this.lastOperand, interval.IsPositive, interval.EndState == PointState.Included, interval.IsPlusInfinityIncluded));
            }

            this.lessInfinityOperands.Add(interval.IsLessInfinityIncluded);
        }

        /// <summary>
        /// Adds multiple intervals to this set.
        /// Note that after adding, you will need to flatten the set to apply the merging operator.
        /// </summary>
        /// <param name="intervals">Intervals to be added to this set.</param>
        public void AddIntervalRange(IEnumerable<Interval<T>> intervals)
        {
            foreach (var interval in intervals)
                this.AddInterval(interval);
        }

        /// <summary>
        /// Adds a negative interval to this set.
        /// Note that after adding, you will need to flatten the set to apply the merging operator.
        /// </summary>
        /// <param name="start">Starting position of the negative interval.</param>
        /// <param name="startExcluded">Whether start element is excluded or not from the set.</param>
        /// <param name="end">End position of the interval.</param>
        /// <param name="endExcluded">Whether the end element is excluded or not from the set.</param>
        public void AddNegativeInterval(T start, bool startExcluded, T end, bool endExcluded)
        {
            var compStartEnd = this.comparer.KeyComparer.Compare(start, end);
            if (compStartEnd >= 0)
                throw new ArgumentException("'start' must come before 'end'");

            this.lastOperand++;

            this.inclusionList.Add(new Item(start, this.lastOperand, true, !startExcluded, false));
            this.inclusionList.Add(new Item(end, this.lastOperand, false, !endExcluded, true));
            this.lessInfinityOperands.Add(true);
        }

        /// <summary>
        /// Adds another continuous set to this set.
        /// Note that after adding, you will need to flatten the set to apply the merging operator.
        /// </summary>
        /// <param name="other">Other set to be add in this set..</param>
        public void AddContinuousSet(ContinuousSet<T> other)
        {
            if (other.flattener != null)
            {
                other = new ContinuousSet<T>(other);
                other.Flatten(mergeRedundantTrues: false, mergeRedundantFalses: false);
            }

            this.lastOperand++;

            foreach (var item in other.inclusionList)
                this.inclusionList.Add(
                    new Item(item.Key, this.lastOperand, item.BeforeIncluded, item.SelfIncluded, item.AfterIncluded));

            this.lessInfinityOperands.Add(other.lessInfinityOperands[0]);
        }

        /// <summary>
        /// Splits the set in the given positions by creating redundant items,
        /// if there is no specific item at the indicated position.
        /// There is no need to flatten the set after doing this.
        /// </summary>
        /// <param name="positions">Positions enumerator where the set will be split.</param>
        /// <param name="redundancyType">Redundancy types that can be created by the splitting method.</param>
        /// <returns>Returns the number of split positions by adding a redundant item; otherwise false.</returns>
        public int SplitAt(IEnumerable<T> positions, RedundancyType redundancyType = RedundancyType.Positive)
        {
            if (!this.EnsureIsFlat("The set must be flattened before splitting it."))
                return 0;

            if (redundancyType == RedundancyType.None)
                return 0;

            var count = this.inclusionList.Count;
            foreach (var eachPosition in positions)
            {
                var index = this.inclusionList.BinarySearch(0, count, new Item(eachPosition, 0, false, true, false), this.comparer);
                if (index < 0)
                {
                    bool included = ~index < count ?
                        this.inclusionList[~index].BeforeIncluded :
                        this.lessInfinityOperands[0];

                    if (redundancyType == RedundancyType.All
                        || redundancyType == RedundancyType.Negative && !included
                        || redundancyType == RedundancyType.Positive && included)
                    {
                        this.inclusionList.Add(new Item(eachPosition, 0, included, included, included));
                    }
                }
            }

            this.inclusionList.Sort(this.comparer);
            return this.inclusionList.Count - count;
        }

        /// <summary>
        /// Splits the set in the given positions by creating redundant items,
        /// if there is no specific item at the indicated position.
        /// There is no need to flatten the set after doing this.
        /// </summary>
        /// <param name="positions">Positions enumerator where the set will be split.</param>
        /// <returns>Returns the number of split positions by adding a redundant item; otherwise false.</returns>
        public int SplitAt(params T[] positions)
        {
            return this.SplitAt((IEnumerable<T>)positions);
        }

        /// <summary>
        /// Flattens the set after adding multiple operands, by applying the operator specified in the Operator property.
        /// </summary>
        /// <param name="setOperator">Object capable of changing how the operation is done, and how elements are merged in the final resulting set.</param>
        /// <param name="mergeRedundantTrues">Whether to delete resulting positive redundant items.</param>
        /// <param name="mergeRedundantFalses">Whether to delete resulting negative redundant items.</param>
        public void Flatten(ISetOperator<T> setOperator = null, bool mergeRedundantTrues = true, bool mergeRedundantFalses = true)
        {
            if (this.flattener == null)
                throw new Exception("'Operator' must be set in order to flatten the set.");

            if (this.lastOperand == 0 || this.inclusionList.Count == 0)
                return;

            this.inclusionList.Sort(this.comparer);

            int opIndex = -1; // opIndex -1 represents the interval from less infinity, to the current point
            int index = 0;
            int indexSelf = 0;
            int indexAfter = 0;

            // operating less infinities
            bool[] bools = this.lessInfinityOperands.ToArray();
            bool beforeResult = this.flattener(bools);
            if (setOperator != null)
                setOperator.Invoke(bools, opIndex++, ref beforeResult);

            this.lessInfinityOperands.Clear();
            this.lessInfinityOperands.Add(beforeResult);

            while (index < this.inclusionList.Count)
            {
                // Counting elements in current group of operands.
                // A group is a sequence of items that have the same key.
                int startOfNextGroup = index;
                while (!IsLastOfGroup(this.inclusionList, startOfNextGroup++, this.comparer.KeyComparer)) { }

                // operating selfs
                while (indexSelf < startOfNextGroup)
                {
                    var item = this.inclusionList[indexSelf++];
                    bools[item.OperandId] = item.SelfIncluded;
                }

                bool selfResult = this.flattener(bools);
                if (setOperator != null)
                    setOperator.Invoke(bools, opIndex++, ref selfResult);

                // operating afters
                while (indexAfter < startOfNextGroup)
                {
                    var item = this.inclusionList[indexAfter++];
                    bools[item.OperandId] = item.AfterIncluded;
                }

                bool afterResult = this.flattener(bools);
                if (setOperator != null)
                    setOperator.Invoke(bools, opIndex++, ref afterResult);

                // creating new item
                var keepInSet = setOperator == null
                                || setOperator.KeepInSet(
                                    ListSkip(this.inclusionList, index).Take(startOfNextGroup - index).Cast<ContinuousSet.IItem<T>>(),
                                    beforeResult,
                                    selfResult,
                                    afterResult);

                var item0 = this.inclusionList[index];
                item0 = keepInSet ? new Item(item0.Key, 0, beforeResult, selfResult, afterResult) : Item.Empty;
                this.inclusionList[index] = item0;

                // preparing for more items
                index = indexSelf;
                beforeResult = afterResult;
            }

            // cleaning garbage items, and redundant sequences of trues/falses
            if (mergeRedundantTrues && mergeRedundantFalses)
                this.inclusionList.RemoveAll(Item.CleanMergeAfterFlattening);
            else if (mergeRedundantFalses)
                this.inclusionList.RemoveAll(Item.CleanMergeFalsesAfterFlattening);
            else if (mergeRedundantTrues)
                this.inclusionList.RemoveAll(Item.CleanMergeTruesAfterFlattening);
            else
                this.inclusionList.RemoveAll(Item.CleanAfterFlattening);

            this.lastOperand = 0;
        }

        /// <summary>
        /// Fast skip implementation optimized for lists that don't mutate while enumerating.
        /// </summary>
        /// <param name="list">The list to be enumerated.</param>
        /// <param name="index">Index to skip to.</param>
        /// <returns>Returns an enumerable object that enumerates items of the list starting at the given index.</returns>
        private static IEnumerable<Item> ListSkip(List<Item> list, int index)
        {
            while (index < list.Count)
                yield return list[index++];
        }

        /// <summary>
        /// Indicates whether the item of list at index is the last element of a group of operands.
        /// </summary>
        /// <param name="list">List containing all elements to be operated.</param>
        /// <param name="index">Item index that is going to be tested to see if it is the last one of the operands group.</param>
        /// <param name="keyComparer">Comparer used to know whether keys are equal or not.</param>
        /// <returns>Returns true if the item is the last one of the group of operands.</returns>
        private static bool IsLastOfGroup(List<Item> list, int index, IComparer<T> keyComparer)
        {
            if (index + 1 >= list.Count)
                return true;

            var item1 = list[index];
            var item2 = list[index + 1];
            return item1.OperandId >= item2.OperandId || keyComparer.Compare(item1.Key, item2.Key) != 0;
        }

        /// <summary>
        /// Forces the merging of redundant items, by eliminating them.
        /// </summary>
        /// <returns>Returns the number of merged (eliminated) items.</returns>
        public int MergeRedundant()
        {
            if (this.lastOperand != 0)
                throw new Exception("Must flatten before merging redundant items.");

            return this.inclusionList.RemoveAll(Item.CleanMergeAfterFlattening);
        }

        /// <summary>
        /// Forces the merging of redundant positive items, by eliminating them.
        /// </summary>
        /// <returns>Returns the number of merged (eliminated) items.</returns>
        public int MergeRedundantTrues()
        {
            if (this.lastOperand != 0)
                throw new Exception("Must flatten before merging redundant items.");

            return this.inclusionList.RemoveAll(Item.CleanMergeTruesAfterFlattening);
        }

        /// <summary>
        /// Forces the merging of redundant negative items, by eliminating them.
        /// </summary>
        /// <returns>Returns the number of merged (eliminated) items.</returns>
        public int MergeRedundantFalses()
        {
            if (this.lastOperand != 0)
                throw new Exception("Must flatten before merging redundant items.");

            return this.inclusionList.RemoveAll(Item.CleanMergeFalsesAfterFlattening);
        }

        /// <summary>
        /// Negates the current set, that is, every element that was in the set are turned out of set,
        /// and every element that was out of the set are turned into the set.
        /// </summary>
        public void Negate()
        {
            if (!this.EnsureIsFlat("Must flatten before negating the set."))
                return;

            this.lessInfinityOperands[0] ^= true; // negating less infinity
            for (int index = 0; index < this.inclusionList.Count; index++)
            {
                var item = this.inclusionList[index];
                this.inclusionList[index] = new Item(item.Key, 0, !item.BeforeIncluded, !item.SelfIncluded, !item.AfterIncluded);
            }
        }

        /// <summary>
        /// Creates a new set that is the negative of the given set.
        /// </summary>
        /// <param name="value">The set to create the negated set for.</param>
        /// <returns>Returns a new negated set of the passes set.</returns>
        public static ContinuousSet<T> operator !(ContinuousSet<T> value)
        {
            value = new ContinuousSet<T>(value);
            value.Flatten(mergeRedundantTrues: false, mergeRedundantFalses: false);
            value.Negate();
            return value;
        }

        /// <summary>
        /// Creates a new set that is intersection of the given sets.
        /// </summary>
        /// <param name="a">The first set to intersect.</param>
        /// <param name="b">The second set to intersect.</param>
        /// <returns>Returns a new intersected set from the passed sets.</returns>
        public static ContinuousSet<T> operator &(ContinuousSet<T> a, ContinuousSet<T> b)
        {
            var result = new ContinuousSet<T>(a, b.inclusionList.Count);
            result.Flatten(mergeRedundantTrues: false, mergeRedundantFalses: false);
            result.Operator = ContinuousSet.OpIntersection;
            result.AddContinuousSet(b);
            result.Flatten(mergeRedundantTrues: false, mergeRedundantFalses: false);
            return result;
        }

        /// <summary>
        /// Creates a new set that is union of the given sets.
        /// </summary>
        /// <param name="a">The first set to unite.</param>
        /// <param name="b">The second set to unite.</param>
        /// <returns>Returns a new united set from the passed sets.</returns>
        public static ContinuousSet<T> operator |(ContinuousSet<T> a, ContinuousSet<T> b)
        {
            var result = new ContinuousSet<T>(a, b.inclusionList.Count);
            result.Flatten(mergeRedundantTrues: false, mergeRedundantFalses: false);
            result.Operator = ContinuousSet.OpUnion;
            result.AddContinuousSet(b);
            result.Flatten(mergeRedundantTrues: false, mergeRedundantFalses: false);
            return result;
        }

        /// <summary>
        /// Creates a new set that contains only elements that are exclusively contained in the first set.
        /// </summary>
        /// <param name="a">The first set of the operation.</param>
        /// <param name="b">The second set of the operation.</param>
        /// <returns>Returns a new set containing all elements of the first set that are not in the second set.</returns>
        public static ContinuousSet<T> operator -(ContinuousSet<T> a, ContinuousSet<T> b)
        {
            var result = new ContinuousSet<T>(a, b.inclusionList.Count);
            result.Flatten(mergeRedundantTrues: false, mergeRedundantFalses: false);
            result.Operator = ContinuousSet.OpSubtraction;
            result.AddContinuousSet(b);
            result.Flatten(mergeRedundantTrues: false, mergeRedundantFalses: false);
            return result;
        }

        /// <summary>
        /// Compares two sets, to determine if they represent the same set.
        /// </summary>
        /// <param name="obj">Object to be compared to.</param>
        /// <returns>Returns true if the current set equals passed parameter.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as ContinuousSet<T>;
            return Equals(this, other, false, false);
        }

        /// <summary>
        /// Gets a hash code for this set, considering all the included elements.
        /// </summary>
        /// <returns>Returns an integer hash code for this set.</returns>
        public override int GetHashCode()
        {
            return GetHashCode(EqualityComparer<T>.Default, this, false, false);
        }

        /// <summary>
        /// Compares two sets, and determines whether they are equal or not,
        /// based on the equality relaxations indicated in the parameters.
        /// </summary>
        /// <param name="x">First element to be compared for equality.</param>
        /// <param name="y">Second element to be compared for equality.</param>
        /// <param name="ignoreRedundantFalses">Whether to ignore redundant negative items in the comparison.</param>
        /// <param name="ignoreRedundantTrues">Whether to ignore redundant positive items in the comparison.</param>
        /// <returns>Returns true if x and y sets are equivalent; otherwise false.</returns>
        public static bool Equals(ContinuousSet<T> x, ContinuousSet<T> y, bool ignoreRedundantFalses, bool ignoreRedundantTrues)
        {
            // both are nulls
            if (x == null && y == null)
                return true;

            // only one is null
            if (x == null || y == null)
                return false;

            // Two sets with inequivalent comparers cannot be equal.
            if (!x.comparer.KeyComparer.Equals(y.comparer.KeyComparer))
                return false;

            if (!x.EnsureIsFlat("'x' set must be flattened before comparing.") || !y.EnsureIsFlat("'y' set must be flattened before comparing."))
                return false;

            var keyComparer = x.comparer.KeyComparer;

            bool xInSet = x.lessInfinityOperands[0];
            bool yInSet = y.lessInfinityOperands[0];

            // comparing less infinity values
            if (xInSet != yInSet)
                return false;

            int xCount = x.inclusionList.Count;
            int yCount = y.inclusionList.Count;

            if (xCount > 0 || yCount > 0)
            {
                int xIndex = 0, yIndex = 0;
                Item xItem, yItem;

                do xItem = xIndex < xCount ? x.inclusionList[xIndex++] : Item.Empty;
                while (!xItem.IsEmpty && (ignoreRedundantTrues && xItem.SelfIncluded && xItem.IsRedundant
                    || ignoreRedundantFalses && !xItem.SelfIncluded && xItem.IsRedundant));

                do yItem = yIndex < yCount ? y.inclusionList[yIndex++] : Item.Empty;
                while (!yItem.IsEmpty && (ignoreRedundantTrues && yItem.SelfIncluded && yItem.IsRedundant
                    || ignoreRedundantFalses && !yItem.SelfIncluded && yItem.IsRedundant));

                int cmpXy = 0;
                while (!xItem.IsEmpty || !yItem.IsEmpty)
                {
                    if (!xItem.IsEmpty && !yItem.IsEmpty)
                        cmpXy = keyComparer.Compare(xItem.Key, yItem.Key);

                    bool xBeforeInSet, xSelfInSet, xAfterInSet;
                    if ((cmpXy <= 0 || yItem.IsEmpty) && !xItem.IsEmpty)
                    {
                        xBeforeInSet = xItem.BeforeIncluded;
                        xSelfInSet = xItem.SelfIncluded;
                        xInSet = xAfterInSet = xItem.AfterIncluded;

                        do xItem = xIndex < xCount ? x.inclusionList[xIndex++] : Item.Empty;
                        while (!xItem.IsEmpty && (ignoreRedundantTrues && xItem.SelfIncluded && xItem.IsRedundant
                            || ignoreRedundantFalses && !xItem.SelfIncluded && xItem.IsRedundant));
                    }
                    else
                    {
                        xBeforeInSet = xSelfInSet = xAfterInSet = xInSet;
                    }

                    bool yBeforeInSet, ySelfInSet, yAfterInSet;
                    if ((cmpXy >= 0 || xItem.IsEmpty) && !yItem.IsEmpty)
                    {
                        yBeforeInSet = yItem.BeforeIncluded;
                        ySelfInSet = yItem.SelfIncluded;
                        yInSet = yAfterInSet = yItem.AfterIncluded;

                        do yItem = yIndex < yCount ? y.inclusionList[yIndex++] : Item.Empty;
                        while (!yItem.IsEmpty && (ignoreRedundantTrues && yItem.SelfIncluded && yItem.IsRedundant
                            || ignoreRedundantFalses && !yItem.SelfIncluded && yItem.IsRedundant));
                    }
                    else
                    {
                        yBeforeInSet = ySelfInSet = yAfterInSet = yInSet;
                    }

                    if (xBeforeInSet != yBeforeInSet || xSelfInSet != ySelfInSet || xAfterInSet != yAfterInSet)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets a hash code for the give object, considering the given relaxations.
        /// </summary>
        /// <param name="eqT">Equality comparer used to get the hash code for keys in the set.</param>
        /// <param name="obj">The set to get the hash code for.</param>
        /// <param name="ignoreRedundantFalses">Whether to ignore redundant negative items in the hash.</param>
        /// <param name="ignoreRedundantTrues">Whether to ignore redundant positive items in the hash.</param>
        /// <returns>Returns an integer hash code for this set.</returns>
        public static int GetHashCode(IEqualityComparer<T> eqT, ContinuousSet<T> obj, bool ignoreRedundantFalses, bool ignoreRedundantTrues)
        {
            int result = 0x64b2017c;
            if (obj != null && obj.EnsureIsFlat("'obj' set must be flattened before hashing."))
            {
                result ^= obj.lessInfinityOperands[0] ? 0x34b1a67 : 0xf12d798;
                int count = obj.inclusionList.Count;
                int index = 0;
                while (index < count)
                {
                    Item item;
                    do item = index < count ? obj.inclusionList[index++] : Item.Empty;
                    while (!item.IsEmpty && (ignoreRedundantTrues && item.SelfIncluded && item.IsRedundant || ignoreRedundantFalses && !item.SelfIncluded && item.IsRedundant));

                    if (item.IsEmpty)
                        break;

                    var itHash = EqualityComparer<int>.Default.GetHashCode(index);
                    var include = (item.SelfIncluded ? 2 : 0) | (item.AfterIncluded ? 1 : 0);
                    result ^= eqT.GetHashCode(item.Key) ^ (itHash * include);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns whether this set contains a value or not.
        /// </summary>
        /// <param name="value">Value to be tested.</param>
        /// <returns>Returns true if the value is in the set; otherwise false.</returns>
        public bool Contains(T value)
        {
            if (!this.EnsureIsFlat("The set must be flattened before seeking for items."))
                return false;

            var index = this.inclusionList.BinarySearch(new Item(value, 0, false, true, false), this.comparer);

            bool included =
                index >= 0 ? this.inclusionList[index].SelfIncluded :
                ~index < this.inclusionList.Count ? this.inclusionList[~index].BeforeIncluded :
                this.lessInfinityOperands[0];

            return included;
        }

        /// <summary>
        /// Ensures that this set is flat before doing operations that requires a flattened set.
        /// </summary>
        /// <param name="message"> The message associated with the error of not being flat already. </param>
        /// <returns> Returns true, if the set is flat; otherwise false. </returns>
        protected virtual bool EnsureIsFlat(string message)
        {
            if (this.lastOperand != 0)
                this.Flatten(mergeRedundantTrues: false, mergeRedundantFalses: false);

            return true;
        }

        /// <summary>
        /// Gets an enumerable object containing a sequence of positive intervals contained in this set.
        /// </summary>
        public IEnumerable<Interval<T>> PositiveIntervals
        {
            get
            {
                if (!this.EnsureIsFlat("The set must be flattened before iterating it."))
                    yield break;

                Debug.Assert(this.lessInfinityOperands != null, "this.lessInfinityOperands should not be null");
                Debug.Assert(this.lessInfinityOperands.Count > 0, "this.lessInfinityOperands.Count should be greater than 0");

                if (this.inclusionList.Count != 0)
                {
                    var itemStart = this.inclusionList[0];
                    var startState = itemStart.SelfIncluded ? PointState.Included : PointState.Excluded;
                    if (itemStart.BeforeIncluded)
                        yield return new Interval<T>(default(T), PointState.Open, itemStart.Key, startState);

                    Debug.Assert(itemStart.BeforeIncluded == this.lessInfinityOperands[0], "itemStart.BeforeIncluded should equal this.lessInfinityOperands[0]");

                    for (int index = 1; index < this.inclusionList.Count; index++)
                    {
                        var itemEnd = this.inclusionList[index];

                        var endState = itemEnd.SelfIncluded ? PointState.Included : PointState.Excluded;

                        Debug.Assert(itemStart.AfterIncluded == itemEnd.BeforeIncluded, "itemStart.AfterIncluded should equal itemEnd.BeforeIncluded");

                        if (itemEnd.BeforeIncluded)
                        {
                            if (itemEnd.IsRedundant)
                            {
                                // redundant end, will consider it as being included in the next interval, not in the current interval
                                yield return new Interval<T>(itemStart.Key, startState, itemEnd.Key, PointState.Excluded);
                            }
                            else
                            {
                                // found interval ending in non redundant item
                                yield return new Interval<T>(itemStart.Key, startState, itemEnd.Key, endState);
                            }
                        }
                        else if (itemEnd.SelfIncluded && !itemEnd.AfterIncluded)
                        {
                            // single lost positive point found
                            yield return new Interval<T>(itemEnd.Key, PointState.Included, itemEnd.Key, PointState.Included);
                        }

                        startState = endState;
                        itemStart = itemEnd;
                    }

                    if (itemStart.AfterIncluded)
                        yield return new Interval<T>(itemStart.Key, startState, default(T), PointState.Open);
                }
                else if (this.lessInfinityOperands[0])
                {
                    yield return new Interval<T>(default(T), PointState.Open, default(T), PointState.Open, true);
                }
            }
        }

        /// <summary>
        /// Clones the continuous set.
        /// </summary>
        /// <returns>Returns the clone of the continuous set.</returns>
        public ContinuousSet<T> Clone()
        {
            return new ContinuousSet<T>(this);
        }

        /// <summary>
        /// Transforms all keys in the set by using the passed transformation delegate.
        /// </summary>
        /// <remarks>
        /// When using the Transform method, the elements must be comparable to the default(T) value,
        /// and also, the transformation delegate must be abled to transform the value of default(T).
        /// </remarks>
        /// <param name="linearTransformationFunc">The linear transformation function to apply. If it is not linear, you may compromise the integrity of the set.</param>
        public void Transform(Func<T, T> linearTransformationFunc)
        {
            if (this.inclusionList.Count > 0)
            {
                var keyComparer = this.comparer.KeyComparer;
                var item0 = this.inclusionList[0];
                var cmp0 = keyComparer.Compare(default(T), item0.Key);
                var cmp1 = keyComparer.Compare(linearTransformationFunc(default(T)), linearTransformationFunc(item0.Key));
                bool orderReverted = cmp0 != cmp1;

                for (int index = 0; index < this.inclusionList.Count; index++)
                {
                    var item = this.inclusionList[index];
                    item = new Item(
                        linearTransformationFunc(item.Key),
                        item.OperandId,
                        orderReverted ? item.AfterIncluded : item.BeforeIncluded,
                        item.SelfIncluded,
                        orderReverted ? item.BeforeIncluded : item.AfterIncluded);

                    this.inclusionList[index] = item;
                }

                if (orderReverted && this.lastOperand == 0)
                    this.inclusionList.Reverse();
            }
        }
    }

    /// <summary>
    /// Contains operations to be used to flatten a continuous set after adding operands to it.
    /// </summary>
    public static class ContinuousSet
    {
        /// <summary>
        /// Represents an item in a continuous set collection.
        /// </summary>
        /// <typeparam name="T">Type of elements in the continuous set collection.</typeparam>
        public interface IItem<out T>
        {
            /// <summary>
            /// Gets a value indicating whether the preceding interval, before this point is included in the set.
            /// </summary>
            bool BeforeIncluded { get; }

            /// <summary>
            /// Gets a value indicating whether this point itself is included in the set.
            /// </summary>
            bool SelfIncluded { get; }

            /// <summary>
            /// Gets a value indicating whether the proceeding interval, after this point is included in the set.
            /// </summary>
            bool AfterIncluded { get; }

            /// <summary>
            /// Gets the Id of the operand in a multilayer set, awaiting for the list of operands to be flattened by an operator.
            /// </summary>
            int OperandId { get; }

            /// <summary>
            /// Gets the key represented by this point.
            /// This is the position of this point inside the set.
            /// </summary>
            T Key { get; }

            /// <summary>
            /// Gets a value indicating whether this point
            /// </summary>
            bool IsEmpty { get; }

            /// <summary>
            /// Gets a value indicating whether this item is redundant or not.
            /// A redundant item has got all of it's components set to the same value,
            /// that is the values of BeforeIncluded, SelfIncluded and AfterIncluded are equal.
            /// </summary>
            bool IsRedundant { get; }
        }

        /// <summary>
        /// Gets the union of all boolean values. That is equivalent to an OR between all of the booleans.
        /// </summary>
        /// <param name="bools">Boolean operands to be united.</param>
        /// <returns>Returns the union of the boolean operands.</returns>
        public static bool OpUnion(bool[] bools)
        {
            return bools.Any(IsTrue);
        }

        /// <summary>
        /// Gets the intersection of all boolean values. That is equivalent to an AND between all of the booleans.
        /// </summary>
        /// <param name="bools">Boolean operands to be intersected.</param>
        /// <returns>Returns the intersection of the boolean operands.</returns>
        public static bool OpIntersection(bool[] bools)
        {
            return bools.All(IsTrue);
        }

        /// <summary>
        /// Gets the first exclusive operation result for the passed boolean operands.
        /// The order does matter for this operation.
        /// </summary>
        /// <param name="bools">Boolean operands to operated.</param>
        /// <returns>Returns the boolean exclusiveness of the first operand.</returns>
        public static bool OpSubtraction(bool[] bools)
        {
            return bools[0] && !bools.Skip(1).Any(IsTrue);
        }

        /// <summary>
        /// Gets whether there is an odd number of trues in the operands list.
        /// This is equivalent to a XOR of all elements.
        /// </summary>
        /// <param name="bools">The operands list.</param>
        /// <returns>Returns true if there is an odd number of true elements in the list; otherwise false.</returns>
        public static bool OpOdd(bool[] bools)
        {
            return bools.Aggregate(AggOdd);
        }

        /// <summary>
        /// Gets whether there is an even number of trues in the operands list.
        /// This is equivalent to a XOR of all elements, being negated with a NOT.
        /// </summary>
        /// <param name="bools">The operands list.</param>
        /// <returns>Returns true if there is an even number of true elements in the list; otherwise false.</returns>
        public static bool OpEven(bool[] bools)
        {
            return !bools.Aggregate(AggOdd);
        }

        /// <summary>
        /// Gets whether only one of the arguments is true.
        /// </summary>
        /// <param name="b1">First value to test.</param>
        /// <param name="b2">Second value to test.</param>
        /// <returns>Returns true if the passed parameters are different; otherwise false.</returns>
        private static bool AggOdd(bool b1, bool b2)
        {
            return b1 != b2;
        }

        /// <summary>
        /// Returns whether the parameter is true.
        /// </summary>
        /// <param name="b">Parameter to return.</param>
        /// <returns>Returns the parameter that is passed.</returns>
        private static bool IsTrue(bool b)
        {
            return b;
        }
    }
}
