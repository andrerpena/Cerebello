namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Possible redundancy types for items in a continuous set.
    /// </summary>
    public enum RedundancyType
    {
        /// <summary>
        /// No redundancies.
        /// </summary>
        None,

        /// <summary>
        /// Only redundant negatives.
        /// </summary>
        Negative,

        /// <summary>
        /// Only redundant positives.
        /// </summary>
        Positive,

        /// <summary>
        /// All redundancies.
        /// </summary>
        All,
    }
}