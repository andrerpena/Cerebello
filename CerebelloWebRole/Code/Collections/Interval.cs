using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Represents an interval of values between a starting point and an ending point.
    /// These points can be open, extending the interval to less or positive infinity.
    /// </summary>
    /// <typeparam name="T">The type of the elements of this interval.</typeparam>
    public struct Interval<T>
    {
        private readonly T start;
        private readonly T end;
        private readonly PointState startState;
        private readonly PointState endState;
        private readonly bool isPositive;

        /// <summary>
        /// Initializes a new instance of the <see cref="Interval{T}"/> struct.
        /// </summary>
        /// <param name="start"> The start point of this interval. </param>
        /// <param name="startState"> The state of the start point, that can be open, positive or negative. </param>
        /// <param name="end"> The end point of this interval. </param>
        /// <param name="endState"> The state of the end point, that can be open, positive or negative. </param>
        /// <param name="isPositive">
        /// Whether the inner space of this interval is included or excluded.
        /// The outer space is the negative of the inner space.
        /// </param>
        public Interval(T start, PointState startState, T end, PointState endState, bool isPositive = true)
        {
            this.start = start;
            this.startState = startState;
            this.end = end;
            this.endState = endState;
            this.isPositive = isPositive;
        }

        /// <summary>
        /// Gets the start of interval.
        /// </summary>
        public T Start
        {
            get { return this.start; }
        }

        /// <summary>
        /// Gets the end of interval.
        /// </summary>
        public T End
        {
            get { return this.end; }
        }

        /// <summary>
        /// Gets whether start of interval is inclusive or exclusive, or if it is open (non existent).
        /// </summary>
        public PointState StartState
        {
            get { return this.startState; }
        }

        /// <summary>
        /// Gets whether end of interval is inclusive or exclusive, or if it is open (non existent).
        /// </summary>
        public PointState EndState
        {
            get { return this.endState; }
        }

        /// <summary>
        /// Gets a value indicating whether this interval represents a positive set, or a negative set.
        /// When positive, values in between start and end, are in the set, and values outside, are not in the set.
        /// When negative, it is the opposite.
        /// </summary>
        public bool IsPositive
        {
            get { return this.isPositive; }
        }

        /// <summary>
        /// Gets a value indicating whether less infinity is included.
        /// </summary>
        public bool IsLessInfinityIncluded
        {
            get { return this.startState == PointState.Open && this.isPositive || this.startState != PointState.Open && !this.isPositive; }
        }

        /// <summary>
        /// Gets a value indicating whether plus infinity is included.
        /// </summary>
        public bool IsPlusInfinityIncluded
        {
            get { return this.endState == PointState.Open && this.isPositive || this.endState != PointState.Open && !this.isPositive; }
        }

        /// <summary>
        /// Gets a value indicating whether this interval is in fact a single point.
        /// </summary>
        public bool IsSinglePoint
        {
            get { return this.startState != PointState.Open && this.endState == PointState.Open && EqualityComparer<T>.Default.Equals(this.start, this.end); }
        }

        /// <summary>
        /// Gets whether the single point represented by this interval is included or excluded.
        /// If this interval is not a single point, this throws an exception.
        /// This will always be Included or Excluded.
        /// </summary>
        public PointState SinglePointState
        {
            get
            {
                if (!this.IsSinglePoint)
                    throw new Exception("The SinglePointState property is only valid when IsSinglePoint is true.");

                if (this.isPositive)
                    return (this.startState == PointState.Included && this.endState == PointState.Included) ? PointState.Included : PointState.Excluded;
                else
                    return (this.startState == PointState.Included || this.endState == PointState.Included) ? PointState.Included : PointState.Excluded;
            }
        }

        /// <summary>
        /// Gets a visual representation of the interval for debugging purposes.
        /// </summary>
        internal string VisualSequence
        {
            get
            {
                var stringBuilder = new StringBuilder(20);
                stringBuilder.Append("…");
                if (this.startState == PointState.Open) stringBuilder.Append(this.isPositive ? "████ … █" : "──── … ─");
                if (this.startState == PointState.Included) stringBuilder.Append(this.isPositive ? "──── … █" : "████ … █");
                if (this.startState == PointState.Excluded) stringBuilder.Append(this.isPositive ? "──── … ─" : "████ … ─");

                if (!EqualityComparer<T>.Default.Equals(this.start, this.end))
                    stringBuilder.Append(this.isPositive ? "████" : "────");

                if (this.endState == PointState.Open) stringBuilder.Append(this.isPositive ? "█ … ████" : "─ … ────");
                if (this.endState == PointState.Included) stringBuilder.Append(this.isPositive ? "█ … ────" : "█ … ████");
                if (this.endState == PointState.Excluded) stringBuilder.Append(this.isPositive ? "─ … ────" : "─ … ████");
                stringBuilder.Append("…");
                return stringBuilder.ToString();
            }
        }

        /// <summary>
        /// Converts this interval to a string.
        /// </summary>
        /// <returns>Returns a string representing the interval.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder(100);

            var invariant = CultureInfo.InvariantCulture;

            if (this.startState == PointState.Open)
            {
                if (this.endState == PointState.Open)
                    return this.isPositive ? "-∞ to +∞" : "Ø";
                if (this.endState == PointState.Excluded)
                    return this.isPositive
                               ? string.Format(invariant, "-∞ to {0}[", this.end)
                               : string.Format(invariant, "(?) Ø + ]{0} to +∞", this.end);
                if (this.endState == PointState.Included)
                    return this.isPositive
                               ? string.Format(invariant, "-∞ to {0}]", this.end)
                               : string.Format(invariant, "(?) Ø + [{0} to +∞", this.end);
            }
            else if (this.startState == PointState.Excluded)
            {
                if (this.endState == PointState.Open)
                    return this.isPositive
                               ? string.Format(invariant, "]{0} to +∞", this.start)
                               : string.Format(invariant, "(?) -∞ to {0}[ + Ø", this.start);

                var isSinglePoint = EqualityComparer<T>.Default.Equals(this.start, this.end);
                if (isSinglePoint)
                {
                    if (this.endState == PointState.Excluded)
                        return this.isPositive
                                   ? string.Format(invariant, "(=Ø) ]{0}[", this.start)
                                   : string.Format(invariant, "-∞ to ]{0}[ to +∞", this.start);
                    if (this.endState == PointState.Included)
                        return this.isPositive
                                   ? string.Format(invariant, "(=Ø) ]{0}]", this.start)
                                   : string.Format(invariant, "(?=-∞ to +∞) -∞ to [{0}[ to +∞", this.start);
                }
                else
                {
                    if (this.endState == PointState.Excluded)
                        return this.isPositive
                                   ? string.Format(invariant, "]{0} to {1}[", this.start, this.end)
                                   : string.Format(invariant, "-∞ to {0}[ + ]{1} to +∞", this.start, this.end);
                    if (this.endState == PointState.Included)
                        return this.isPositive
                                   ? string.Format(invariant, "]{0} to {1}]", this.start, this.end)
                                   : string.Format(invariant, "-∞ to {0}[ + [{1} to +∞", this.start, this.end);
                }
            }
            else if (this.startState == PointState.Included)
            {
                if (this.endState == PointState.Open)
                    return this.isPositive
                               ? string.Format(invariant, "[{0} to +∞", this.start)
                               : string.Format(invariant, "(?) -∞ to {0}] + Ø", this.start);

                var isSinglePoint = EqualityComparer<T>.Default.Equals(this.start, this.end);
                if (isSinglePoint)
                {
                    if (this.endState == PointState.Excluded)
                        return this.isPositive
                                   ? string.Format(invariant, "(=Ø) [{0}[", this.start)
                                   : string.Format(invariant, "(?=-∞ to +∞) -∞ to ]{0}] to +∞", this.start);
                    if (this.endState == PointState.Included)
                        return this.isPositive
                                   ? string.Format(invariant, "[{0}]", this.start)
                                   : string.Format(invariant, "(?=-∞ to +∞) -∞ to [{0}] to +∞", this.start);
                }
                else
                {
                    if (this.endState == PointState.Excluded)
                        return this.isPositive
                                   ? string.Format(invariant, "[{0} to {1}[", this.start, this.end)
                                   : string.Format(invariant, "-∞ to {0}] + ]{1} to +∞", this.start, this.end);
                    if (this.endState == PointState.Included)
                        return this.isPositive
                                   ? string.Format(invariant, "[{0} to {1}]", this.start, this.end)
                                   : string.Format(invariant, "-∞ to {0}] + [{1} to +∞", this.start, this.end);
                }
            }

            return stringBuilder.ToString();
        }
    }
}