using System;
using System.Linq;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests.Other
{
    /// <summary>
    /// The continuous set tests.
    /// </summary>
    [TestClass]
    public class ContinuousSetTests
    {
        /// <summary>
        /// Tests continuous sets of DateTimes, by adding intervals, then splitting, intersecting, and doing lots of operations.
        /// </summary>
        [TestMethod]
        public void SetSplitingEveryMonth()
        {
            #region snapSetIntervals = ...
            // the following snapshot was generated from a good result using the method GenerateCode("snapSetIntervals")
            var snapSetIntervals = new[]
                {
                    // [01/01/2000 00:00:00 to 10/10/2002 00:00:00]
                    new Interval<DateTime>(new DateTime(2000, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2002, 10, 10, 00, 00, 00), PointState.Included, isPositive: true),

                    // [01/01/2010 00:00:00 to 10/10/2012 00:00:00]
                    new Interval<DateTime>(new DateTime(2010, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2012, 10, 10, 00, 00, 00), PointState.Included, isPositive: true)
                };
            #endregion

            #region snapSetIntervalsPoint = ...
            // the following snapshot was generated from a good result using the method GenerateCode("snapSetIntervalsPoint")
            var snapSetIntervalsPoint = new[]
                {
                    // [01/01/2000 00:00:00 to 10/10/2002 00:00:00]
                    new Interval<DateTime>(new DateTime(2000, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2002, 10, 10, 00, 00, 00), PointState.Included, isPositive: true),

                    // [04/05/2005 12:35:20]
                    new Interval<DateTime>(new DateTime(2005, 04, 05, 12, 35, 20), PointState.Included, new DateTime(2005, 04, 05, 12, 35, 20), PointState.Included, isPositive: true),

                    // [01/01/2010 00:00:00 to 10/10/2012 00:00:00]
                    new Interval<DateTime>(new DateTime(2010, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2012, 10, 10, 00, 00, 00), PointState.Included, isPositive: true)
                };
            #endregion

            #region snapSetIntervalsSplit = ...
            // the following snapshot was generated from a good result using the method GenerateCode("snapSetIntervalsSplit")
            var snapSetIntervalsSplit = new[]
                {
                    // [01/01/2000 00:00:00 to 05/01/2000 00:00:00[
                    new Interval<DateTime>(new DateTime(2000, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2000, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2000 00:00:00 to 09/01/2000 00:00:00[
                    new Interval<DateTime>(new DateTime(2000, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2000, 09, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/01/2000 00:00:00 to 01/01/2001 00:00:00[
                    new Interval<DateTime>(new DateTime(2000, 09, 01, 00, 00, 00), PointState.Included, new DateTime(2001, 01, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [01/01/2001 00:00:00 to 05/01/2001 00:00:00[
                    new Interval<DateTime>(new DateTime(2001, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2001, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2001 00:00:00 to 09/01/2001 00:00:00[
                    new Interval<DateTime>(new DateTime(2001, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2001, 09, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/01/2001 00:00:00 to 01/01/2002 00:00:00[
                    new Interval<DateTime>(new DateTime(2001, 09, 01, 00, 00, 00), PointState.Included, new DateTime(2002, 01, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [01/01/2002 00:00:00 to 05/01/2002 00:00:00[
                    new Interval<DateTime>(new DateTime(2002, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2002, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2002 00:00:00 to 09/01/2002 00:00:00[
                    new Interval<DateTime>(new DateTime(2002, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2002, 09, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/01/2002 00:00:00 to 10/10/2002 00:00:00]
                    new Interval<DateTime>(new DateTime(2002, 09, 01, 00, 00, 00), PointState.Included, new DateTime(2002, 10, 10, 00, 00, 00), PointState.Included, isPositive: true),

                    // [01/01/2010 00:00:00 to 05/01/2010 00:00:00[
                    new Interval<DateTime>(new DateTime(2010, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2010, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2010 00:00:00 to 09/01/2010 00:00:00[
                    new Interval<DateTime>(new DateTime(2010, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2010, 09, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/01/2010 00:00:00 to 01/01/2011 00:00:00[
                    new Interval<DateTime>(new DateTime(2010, 09, 01, 00, 00, 00), PointState.Included, new DateTime(2011, 01, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [01/01/2011 00:00:00 to 05/01/2011 00:00:00[
                    new Interval<DateTime>(new DateTime(2011, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2011, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2011 00:00:00 to 09/01/2011 00:00:00[
                    new Interval<DateTime>(new DateTime(2011, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2011, 09, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/01/2011 00:00:00 to 01/01/2012 00:00:00[
                    new Interval<DateTime>(new DateTime(2011, 09, 01, 00, 00, 00), PointState.Included, new DateTime(2012, 01, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [01/01/2012 00:00:00 to 05/01/2012 00:00:00[
                    new Interval<DateTime>(new DateTime(2012, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2012, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2012 00:00:00 to 09/01/2012 00:00:00[
                    new Interval<DateTime>(new DateTime(2012, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2012, 09, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/01/2012 00:00:00 to 10/10/2012 00:00:00]
                    new Interval<DateTime>(new DateTime(2012, 09, 01, 00, 00, 00), PointState.Included, new DateTime(2012, 10, 10, 00, 00, 00), PointState.Included, isPositive: true)
                };
            #endregion

            #region snapSetIntervalsSplitPoint = ...
            // the following snapshot was generated from a good result using the method GenerateCode("snapSetIntervalsSplitPoint")
            var snapSetIntervalsSplitPoint = new[]
                {
                    // [01/01/2000 00:00:00 to 05/01/2000 00:00:00[
                    new Interval<DateTime>(new DateTime(2000, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2000, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2000 00:00:00 to 09/01/2000 00:00:00[
                    new Interval<DateTime>(new DateTime(2000, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2000, 09, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/01/2000 00:00:00 to 01/01/2001 00:00:00[
                    new Interval<DateTime>(new DateTime(2000, 09, 01, 00, 00, 00), PointState.Included, new DateTime(2001, 01, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [01/01/2001 00:00:00 to 05/01/2001 00:00:00[
                    new Interval<DateTime>(new DateTime(2001, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2001, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2001 00:00:00 to 09/01/2001 00:00:00[
                    new Interval<DateTime>(new DateTime(2001, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2001, 09, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/01/2001 00:00:00 to 01/01/2002 00:00:00[
                    new Interval<DateTime>(new DateTime(2001, 09, 01, 00, 00, 00), PointState.Included, new DateTime(2002, 01, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [01/01/2002 00:00:00 to 05/01/2002 00:00:00[
                    new Interval<DateTime>(new DateTime(2002, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2002, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2002 00:00:00 to 09/01/2002 00:00:00[
                    new Interval<DateTime>(new DateTime(2002, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2002, 09, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/01/2002 00:00:00 to 10/10/2002 00:00:00]
                    new Interval<DateTime>(new DateTime(2002, 09, 01, 00, 00, 00), PointState.Included, new DateTime(2002, 10, 10, 00, 00, 00), PointState.Included, isPositive: true),

                    // [04/05/2005 12:35:20]
                    new Interval<DateTime>(new DateTime(2005, 04, 05, 12, 35, 20), PointState.Included, new DateTime(2005, 04, 05, 12, 35, 20), PointState.Included, isPositive: true),

                    // [01/01/2010 00:00:00 to 05/01/2010 00:00:00[
                    new Interval<DateTime>(new DateTime(2010, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2010, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2010 00:00:00 to 09/01/2010 00:00:00[
                    new Interval<DateTime>(new DateTime(2010, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2010, 09, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/01/2010 00:00:00 to 01/01/2011 00:00:00[
                    new Interval<DateTime>(new DateTime(2010, 09, 01, 00, 00, 00), PointState.Included, new DateTime(2011, 01, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [01/01/2011 00:00:00 to 05/01/2011 00:00:00[
                    new Interval<DateTime>(new DateTime(2011, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2011, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2011 00:00:00 to 09/01/2011 00:00:00[
                    new Interval<DateTime>(new DateTime(2011, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2011, 09, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/01/2011 00:00:00 to 01/01/2012 00:00:00[
                    new Interval<DateTime>(new DateTime(2011, 09, 01, 00, 00, 00), PointState.Included, new DateTime(2012, 01, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [01/01/2012 00:00:00 to 05/01/2012 00:00:00[
                    new Interval<DateTime>(new DateTime(2012, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2012, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2012 00:00:00 to 09/01/2012 00:00:00[
                    new Interval<DateTime>(new DateTime(2012, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2012, 09, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/01/2012 00:00:00 to 10/10/2012 00:00:00]
                    new Interval<DateTime>(new DateTime(2012, 09, 01, 00, 00, 00), PointState.Included, new DateTime(2012, 10, 10, 00, 00, 00), PointState.Included, isPositive: true)
                };
            #endregion

            #region set2Intervals = ...
            // the following snapshot was generated from a good result using the method GenerateCode("set2Intervals")
            var set2Intervals = new[]
                {
                    // ]10/05/1999 00:00:00 to 08/05/2000 15:44:30[
                    new Interval<DateTime>(new DateTime(1999, 10, 05, 00, 00, 00), PointState.Excluded, new DateTime(2000, 08, 05, 15, 44, 30), PointState.Excluded, isPositive: true),

                    // [01/02/2001 00:00:00 to 02/03/2011 14:00:00]
                    new Interval<DateTime>(new DateTime(2001, 01, 02, 00, 00, 00), PointState.Included, new DateTime(2011, 02, 03, 14, 00, 00), PointState.Included, isPositive: true)
                };
            #endregion

            #region intervals = ...
            // the following snapshot was generated from a good result using the method GenerateCode("intervals")
            var intervals = new[]
                {
                    // [01/01/2000 00:00:00 to 05/01/2000 00:00:00[
                    new Interval<DateTime>(new DateTime(2000, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2000, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2000 00:00:00 to 08/05/2000 15:44:30[
                    new Interval<DateTime>(new DateTime(2000, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2000, 08, 05, 15, 44, 30), PointState.Excluded, isPositive: true),

                    // [01/02/2001 00:00:00 to 05/01/2001 00:00:00[
                    new Interval<DateTime>(new DateTime(2001, 01, 02, 00, 00, 00), PointState.Included, new DateTime(2001, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2001 00:00:00 to 09/01/2001 00:00:00[
                    new Interval<DateTime>(new DateTime(2001, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2001, 09, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/01/2001 00:00:00 to 01/01/2002 00:00:00[
                    new Interval<DateTime>(new DateTime(2001, 09, 01, 00, 00, 00), PointState.Included, new DateTime(2002, 01, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [01/01/2002 00:00:00 to 05/01/2002 00:00:00[
                    new Interval<DateTime>(new DateTime(2002, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2002, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2002 00:00:00 to 09/01/2002 00:00:00[
                    new Interval<DateTime>(new DateTime(2002, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2002, 09, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/01/2002 00:00:00 to 10/10/2002 00:00:00]
                    new Interval<DateTime>(new DateTime(2002, 09, 01, 00, 00, 00), PointState.Included, new DateTime(2002, 10, 10, 00, 00, 00), PointState.Included, isPositive: true),

                    // [04/05/2005 12:35:20]
                    new Interval<DateTime>(new DateTime(2005, 04, 05, 12, 35, 20), PointState.Included, new DateTime(2005, 04, 05, 12, 35, 20), PointState.Included, isPositive: true),

                    // [01/01/2010 00:00:00 to 05/01/2010 00:00:00[
                    new Interval<DateTime>(new DateTime(2010, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2010, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2010 00:00:00 to 09/01/2010 00:00:00[
                    new Interval<DateTime>(new DateTime(2010, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2010, 09, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/01/2010 00:00:00 to 01/01/2011 00:00:00[
                    new Interval<DateTime>(new DateTime(2010, 09, 01, 00, 00, 00), PointState.Included, new DateTime(2011, 01, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [01/01/2011 00:00:00 to 02/03/2011 14:00:00]
                    new Interval<DateTime>(new DateTime(2011, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2011, 02, 03, 14, 00, 00), PointState.Included, isPositive: true)
                };
            #endregion

            #region snapSetNot = ...
            // the following snapshot was generated from a good result using the method GenerateCode("snapSetNot")
            var snapSetNot = new[]
                {
                    // -∞ to 01/01/2000 00:00:00[
                    new Interval<DateTime>(default(DateTime), PointState.Open, new DateTime(2000, 01, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [08/05/2000 15:44:30 to 01/02/2001 00:00:00[
                    new Interval<DateTime>(new DateTime(2000, 08, 05, 15, 44, 30), PointState.Included, new DateTime(2001, 01, 02, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // ]10/10/2002 00:00:00 to 04/05/2005 12:35:20[
                    new Interval<DateTime>(new DateTime(2002, 10, 10, 00, 00, 00), PointState.Excluded, new DateTime(2005, 04, 05, 12, 35, 20), PointState.Excluded, isPositive: true),

                    // ]04/05/2005 12:35:20 to 01/01/2010 00:00:00[
                    new Interval<DateTime>(new DateTime(2005, 04, 05, 12, 35, 20), PointState.Excluded, new DateTime(2010, 01, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // ]02/03/2011 14:00:00 to +∞
                    new Interval<DateTime>(new DateTime(2011, 02, 03, 14, 00, 00), PointState.Excluded, default(DateTime), PointState.Open, isPositive: true)
                };
            #endregion

            // commented code: useful to recreate the snapshots at the top of this method
            // string code = null;
            var set = new ContinuousSet<DateTime>();
            set.AddInterval(new DateTime(2000, 1, 1), true, new DateTime(2002, 10, 10), true);
            set.AddInterval(new DateTime(2010, 1, 1), true, new DateTime(2012, 10, 10), true);

            var setClone = set.Clone();

            // commented code: useful to recreate the snapshots at the top of this method
            // code += GenerateCode(setClone.PositiveIntervals.ToArray(), "snapSetIntervals");
            CollectionAssert.AreEqual(snapSetIntervals, setClone.PositiveIntervals.ToArray());

            setClone.AddPoint(new DateTime(2005, 04, 05, 12, 35, 20));

            // commented code: useful to recreate the snapshots at the top of this method
            // code += GenerateCode(setClone.PositiveIntervals.ToArray(), "snapSetIntervalsPoint");
            CollectionAssert.AreEqual(snapSetIntervalsPoint, setClone.PositiveIntervals.ToArray());

            setClone.SplitAt(DateTimeHelper.Range(new DateTime(1990, 1, 1), new DateTime(2020, 1, 1), d => d.AddMonths(4)));
            set.SplitAt(DateTimeHelper.Range(new DateTime(1990, 1, 1), new DateTime(2020, 1, 1), d => d.AddMonths(4)));

            CollectionAssert.AreEqual(snapSetIntervalsSplitPoint, setClone.PositiveIntervals.ToArray());

            // commented code: useful to recreate the snapshots at the top of this method
            // code += GenerateCode(set.PositiveIntervals.ToArray(), "snapSetIntervalsSplit");
            CollectionAssert.AreEqual(snapSetIntervalsSplit, set.PositiveIntervals.ToArray());

            set.AddPoint(new DateTime(2005, 04, 05, 12, 35, 20));
            set.Flatten(mergeRedundantTrues: false);

            // commented code: useful to recreate the snapshots at the top of this method
            // code += GenerateCode(set.PositiveIntervals.ToArray(), "snapSetIntervalsSplitPoint");
            CollectionAssert.AreEqual(snapSetIntervalsSplitPoint, set.PositiveIntervals.ToArray());

            CollectionAssert.AreEqual(set.PositiveIntervals.ToArray(), setClone.PositiveIntervals.ToArray());

            var set2 = new ContinuousSet<DateTime>();
            set2.AddInterval(new DateTime(1999, 10, 5), false, new DateTime(2000, 08, 05, 15, 44, 30), false);
            set2.AddInterval(new DateTime(2001, 01, 2), true, new DateTime(2011, 02, 03, 14, 00, 00), true);
            set.Flatten();

            // commented code: useful to recreate the snapshots at the top of this method
            // code += GenerateCode(set2.PositiveIntervals.ToArray(), "set2Intervals");
            CollectionAssert.AreEqual(set2Intervals, set2.PositiveIntervals.ToArray());

            set.Operator = ContinuousSet.OpIntersection;
            set.AddContinuousSet(set2);
            set.Flatten(mergeRedundantTrues: false);

            var allIntervals = set.PositiveIntervals.ToArray();

            // commented code: useful to recreate the snapshots at the top of this method
            // code += GenerateCode(allIntervals, "intervals");
            CollectionAssert.AreEqual(intervals, allIntervals);

            var set3 = new ContinuousSet<DateTime>();
            set3.AddIntervalRange(intervals);
            set3.Flatten(mergeRedundantTrues: false);
            CollectionAssert.AreEqual(intervals, set3.PositiveIntervals.ToArray());

            var setNot = !set;

            // commented code: useful to recreate the snapshots at the top of this method
            // code += GenerateCode(setNot.PositiveIntervals.ToArray(), "snapSetNot");
            CollectionAssert.AreEqual(snapSetNot, setNot.PositiveIntervals.ToArray());

            #region snapSetFull = ...
            // the following snapshot was generated from a good result using the method GenerateCode("snapSetFull")
            var snapSetFull = new[]
                {
                    // -∞ to 01/01/2000 00:00:00]
                    new Interval<DateTime>(default(DateTime), PointState.Open, new DateTime(2000, 01, 01, 00, 00, 00), PointState.Included, isPositive: true),

                    // [01/01/2000 00:00:00 to 05/01/2000 00:00:00[
                    new Interval<DateTime>(new DateTime(2000, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2000, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2000 00:00:00 to 08/05/2000 15:44:30[
                    new Interval<DateTime>(new DateTime(2000, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2000, 08, 05, 15, 44, 30), PointState.Excluded, isPositive: true),

                    // [08/05/2000 15:44:30 to 01/02/2001 00:00:00[
                    new Interval<DateTime>(new DateTime(2000, 08, 05, 15, 44, 30), PointState.Included, new DateTime(2001, 01, 02, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [01/02/2001 00:00:00 to 05/01/2001 00:00:00[
                    new Interval<DateTime>(new DateTime(2001, 01, 02, 00, 00, 00), PointState.Included, new DateTime(2001, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2001 00:00:00 to 09/01/2001 00:00:00[
                    new Interval<DateTime>(new DateTime(2001, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2001, 09, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/01/2001 00:00:00 to 01/01/2002 00:00:00[
                    new Interval<DateTime>(new DateTime(2001, 09, 01, 00, 00, 00), PointState.Included, new DateTime(2002, 01, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [01/01/2002 00:00:00 to 05/01/2002 00:00:00[
                    new Interval<DateTime>(new DateTime(2002, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2002, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2002 00:00:00 to 09/01/2002 00:00:00[
                    new Interval<DateTime>(new DateTime(2002, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2002, 09, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/01/2002 00:00:00 to 10/10/2002 00:00:00[
                    new Interval<DateTime>(new DateTime(2002, 09, 01, 00, 00, 00), PointState.Included, new DateTime(2002, 10, 10, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [10/10/2002 00:00:00 to 04/05/2005 12:35:20[
                    new Interval<DateTime>(new DateTime(2002, 10, 10, 00, 00, 00), PointState.Included, new DateTime(2005, 04, 05, 12, 35, 20), PointState.Excluded, isPositive: true),

                    // [04/05/2005 12:35:20 to 01/01/2010 00:00:00[
                    new Interval<DateTime>(new DateTime(2005, 04, 05, 12, 35, 20), PointState.Included, new DateTime(2010, 01, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [01/01/2010 00:00:00 to 05/01/2010 00:00:00[
                    new Interval<DateTime>(new DateTime(2010, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2010, 05, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/01/2010 00:00:00 to 09/01/2010 00:00:00[
                    new Interval<DateTime>(new DateTime(2010, 05, 01, 00, 00, 00), PointState.Included, new DateTime(2010, 09, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/01/2010 00:00:00 to 01/01/2011 00:00:00[
                    new Interval<DateTime>(new DateTime(2010, 09, 01, 00, 00, 00), PointState.Included, new DateTime(2011, 01, 01, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [01/01/2011 00:00:00 to 02/03/2011 14:00:00[
                    new Interval<DateTime>(new DateTime(2011, 01, 01, 00, 00, 00), PointState.Included, new DateTime(2011, 02, 03, 14, 00, 00), PointState.Excluded, isPositive: true),

                    // [02/03/2011 14:00:00 to +∞
                    new Interval<DateTime>(new DateTime(2011, 02, 03, 14, 00, 00), PointState.Included, default(DateTime), PointState.Open, isPositive: true)
                };
            #endregion

            #region snapSetFullMerged = ...
            // the following snapshot was generated from a good result using the method GenerateCode("snapSetFullMerged")
            var snapSetFullMerged = new[]
                {
                    // -∞ to +∞
                    new Interval<DateTime>(default(DateTime), PointState.Open, default(DateTime), PointState.Open, isPositive: true)
                };
            #endregion

            var setFull = set | !set;

            // code += GenerateCode(setFull.PositiveIntervals.ToArray(), "snapSetFull");
            CollectionAssert.AreEqual(snapSetFull, setFull.PositiveIntervals.ToArray());

            var setEmpty = !setFull;
            CollectionAssert.AreEqual(new DateTime[0], setEmpty.PositiveIntervals.ToArray());
            setFull.MergeRedundant();

            // code += GenerateCode(setFull.PositiveIntervals.ToArray(), "snapSetFullMerged");
            CollectionAssert.AreEqual(snapSetFullMerged, setFull.PositiveIntervals.ToArray());

            setEmpty.MergeRedundant();
            CollectionAssert.AreEqual(new DateTime[0], setEmpty.PositiveIntervals.ToArray());

            #region snapTransformed = ...
            // the following snapshot was generated from a good result using the method GenerateCode("snapTransformed")
            var snapTransformed = new[]
                {
                    // [02/20/2000 00:00:00 to 06/20/2000 00:00:00[
                    new Interval<DateTime>(new DateTime(2000, 02, 20, 00, 00, 00), PointState.Included, new DateTime(2000, 06, 20, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [06/20/2000 00:00:00 to 09/24/2000 15:44:30[
                    new Interval<DateTime>(new DateTime(2000, 06, 20, 00, 00, 00), PointState.Included, new DateTime(2000, 09, 24, 15, 44, 30), PointState.Excluded, isPositive: true),

                    // [02/21/2001 00:00:00 to 06/20/2001 00:00:00[
                    new Interval<DateTime>(new DateTime(2001, 02, 21, 00, 00, 00), PointState.Included, new DateTime(2001, 06, 20, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [06/20/2001 00:00:00 to 10/21/2001 00:00:00[
                    new Interval<DateTime>(new DateTime(2001, 06, 20, 00, 00, 00), PointState.Included, new DateTime(2001, 10, 21, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [10/21/2001 00:00:00 to 02/20/2002 00:00:00[
                    new Interval<DateTime>(new DateTime(2001, 10, 21, 00, 00, 00), PointState.Included, new DateTime(2002, 02, 20, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [02/20/2002 00:00:00 to 06/20/2002 00:00:00[
                    new Interval<DateTime>(new DateTime(2002, 02, 20, 00, 00, 00), PointState.Included, new DateTime(2002, 06, 20, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [06/20/2002 00:00:00 to 10/21/2002 00:00:00[
                    new Interval<DateTime>(new DateTime(2002, 06, 20, 00, 00, 00), PointState.Included, new DateTime(2002, 10, 21, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [10/21/2002 00:00:00 to 11/29/2002 00:00:00]
                    new Interval<DateTime>(new DateTime(2002, 10, 21, 00, 00, 00), PointState.Included, new DateTime(2002, 11, 29, 00, 00, 00), PointState.Included, isPositive: true),

                    // [05/25/2005 12:35:20]
                    new Interval<DateTime>(new DateTime(2005, 05, 25, 12, 35, 20), PointState.Included, new DateTime(2005, 05, 25, 12, 35, 20), PointState.Included, isPositive: true),

                    // [02/20/2010 00:00:00 to 06/20/2010 00:00:00[
                    new Interval<DateTime>(new DateTime(2010, 02, 20, 00, 00, 00), PointState.Included, new DateTime(2010, 06, 20, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [06/20/2010 00:00:00 to 10/21/2010 00:00:00[
                    new Interval<DateTime>(new DateTime(2010, 06, 20, 00, 00, 00), PointState.Included, new DateTime(2010, 10, 21, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [10/21/2010 00:00:00 to 02/20/2011 00:00:00[
                    new Interval<DateTime>(new DateTime(2010, 10, 21, 00, 00, 00), PointState.Included, new DateTime(2011, 02, 20, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [02/20/2011 00:00:00 to 03/25/2011 14:00:00]
                    new Interval<DateTime>(new DateTime(2011, 02, 20, 00, 00, 00), PointState.Included, new DateTime(2011, 03, 25, 14, 00, 00), PointState.Included, isPositive: true)
                };
            #endregion

            var setTransform = set.Clone();
            setTransform.Transform(k => k + TimeSpan.FromDays(50));

            // code += GenerateCode(setFull.PositiveIntervals.ToArray(), "snapSetFullMerged");
            // var code = GenerateCode(setTransform.PositiveIntervals.ToArray(), "snapTransformed");
            CollectionAssert.AreEqual(snapTransformed, setTransform.PositiveIntervals.ToArray());

            #region snapTransformedInvMul = ...
            // the following snapshot was generated from a good result using the method GenerateCode("snapTransformedInvMul")
            var snapTransformedInvMul = new[]
                {
                    // [07/16/1977 20:00:00 to 09/22/1977 00:00:00[
                    new Interval<DateTime>(new DateTime(1977, 07, 16, 20, 00, 00), PointState.Included, new DateTime(1977, 09, 22, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/22/1977 00:00:00 to 05/24/1978 00:00:00[
                    new Interval<DateTime>(new DateTime(1977, 09, 22, 00, 00, 00), PointState.Included, new DateTime(1978, 05, 24, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/24/1978 00:00:00 to 01/25/1979 00:00:00[
                    new Interval<DateTime>(new DateTime(1978, 05, 24, 00, 00, 00), PointState.Included, new DateTime(1979, 01, 25, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [01/25/1979 00:00:00 to 09/22/1979 00:00:00]
                    new Interval<DateTime>(new DateTime(1979, 01, 25, 00, 00, 00), PointState.Included, new DateTime(1979, 09, 22, 00, 00, 00), PointState.Included, isPositive: true),

                    // [03/15/1989 22:49:20]
                    new Interval<DateTime>(new DateTime(1989, 03, 15, 22, 49, 20), PointState.Included, new DateTime(1989, 03, 15, 22, 49, 20), PointState.Included, isPositive: true),

                    // [03/07/1994 00:00:00 to 05/24/1994 00:00:00[
                    new Interval<DateTime>(new DateTime(1994, 03, 07, 00, 00, 00), PointState.Included, new DateTime(1994, 05, 24, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/24/1994 00:00:00 to 01/25/1995 00:00:00[
                    new Interval<DateTime>(new DateTime(1994, 05, 24, 00, 00, 00), PointState.Included, new DateTime(1995, 01, 25, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [01/25/1995 00:00:00 to 09/22/1995 00:00:00[
                    new Interval<DateTime>(new DateTime(1995, 01, 25, 00, 00, 00), PointState.Included, new DateTime(1995, 09, 22, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [09/22/1995 00:00:00 to 05/23/1996 00:00:00[
                    new Interval<DateTime>(new DateTime(1995, 09, 22, 00, 00, 00), PointState.Included, new DateTime(1996, 05, 23, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [05/23/1996 00:00:00 to 01/24/1997 00:00:00[
                    new Interval<DateTime>(new DateTime(1996, 05, 23, 00, 00, 00), PointState.Included, new DateTime(1997, 01, 24, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [01/24/1997 00:00:00 to 09/19/1997 00:00:00]
                    new Interval<DateTime>(new DateTime(1997, 01, 24, 00, 00, 00), PointState.Included, new DateTime(1997, 09, 19, 00, 00, 00), PointState.Included, isPositive: true),

                    // ]07/14/1998 16:31:00 to 01/24/1999 00:00:00[
                    new Interval<DateTime>(new DateTime(1998, 07, 14, 16, 31, 00), PointState.Excluded, new DateTime(1999, 01, 24, 00, 00, 00), PointState.Excluded, isPositive: true),

                    // [01/24/1999 00:00:00 to 09/23/1999 00:00:00]
                    new Interval<DateTime>(new DateTime(1999, 01, 24, 00, 00, 00), PointState.Included, new DateTime(1999, 09, 23, 00, 00, 00), PointState.Included, isPositive: true)
                };
            #endregion

            setTransform.Transform(k => new DateTime(2000, 1, 1) - (k - new DateTime(2000, 1, 1)) - (k - new DateTime(2000, 1, 1)));

            // code += GenerateCode(setFull.PositiveIntervals.ToArray(), "snapSetFullMerged");
            // var code2 = GenerateCode(setTransform.PositiveIntervals.ToArray(), "snapTransformedInvMul");
            CollectionAssert.AreEqual(snapTransformedInvMul, setTransform.PositiveIntervals.ToArray());
        }

        /// <summary>
        /// Generates a code containing all intervals in a list of intervals of DateTimes.
        /// </summary>
        /// <param name="allIntervals"> The intervals to generate code for. </param>
        /// <param name="varName"> The variable name to use. </param>
        /// <returns> Returns the generated code for the data. </returns>
        public static string GenerateCode(Interval<DateTime>[] allIntervals, string varName)
        {
            var dateToStr = (Func<DateTime, string>)(d => d == default(DateTime) ? "default(DateTime)" : d.ToString("'new DateTime('yyyy', 'MM', 'dd', 'HH', 'mm', 'ss')'"));

            var str = string.Format(
                @"
            #region {0} = ...
            // the following snapshot was generated from a good result using the method GenerateCode(""{0}"")
            var {0} = new []
                {{",
                     varName) + string.Join(
                @",
",
                allIntervals.Select(
                    i => string.Format(
                        @"
                    // {5}
                    new Interval<DateTime>({0}, PointState.{1}, {2}, PointState.{3}, isPositive: {4})",
                        dateToStr(i.Start),
                        i.StartState,
                        dateToStr(i.End),
                        i.EndState,
                        i.IsPositive ? "true" : "false",
                        i))) + @"
                };
            #endregion
";

            return str;
        }
    }
}
