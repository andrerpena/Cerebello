namespace Cerebello.SolutionSetup
{
    /// <summary>
    /// Enumerates the kinds of type casts.
    /// </summary>
    public enum TypeCastIs
    {
        /// <summary>
        /// The type cast cannot be made.
        /// </summary>
        Impossible = 0,

        /// <summary>
        /// The type cast can be made implicitly or explicitly.
        /// </summary>
        BuiltInImplicit = 1,

        /// <summary>
        /// The type cast can be made explicitly only, probably losing data and/or precision.
        /// </summary>
        BuiltInExplicit = 2,

        /// <summary>
        /// The type cast can be made by covariance implicitly or explicitly.
        /// </summary>
        Covariant = 4,

        /// <summary>
        /// The type cast can be made by contravariance explicitly only, and may throw an exception if the cast fails.
        /// </summary>
        Contravariant = 8,

        /// <summary>
        /// The type cast can be made using a custom cast-operator implicitly or explicitly.
        /// </summary>
        CustomImplicit = 16,

        /// <summary>
        /// The type cast can be made using a custom cast-operator explicitly only.
        /// </summary>
        CustomExplicit = 32,

        /// <summary>
        /// Both types are the same.
        /// </summary>
        NotNeeded = 64,
    }
}
