using System;

namespace Cerebello.Firestarter
{
    /// <summary>
    /// Allows predictable random numbers to be generated,
    /// while still allowing the Firestarter code being changed,
    /// as long as new random branches are always added
    /// to the end of the changed branch.
    /// </summary>
    public class RandomContext : IDisposable
    {
        [ThreadStatic]
        private static RandomContext current;

        private readonly RandomContext parent;

        public Random Random { get; private set; }

        public static int? DefaultSeed { get; set; }

        public static RandomContext Create(int? seed = null)
        {
            return new RandomContext(seed);
        }

        public RandomContext(int? seed)
        {
            this.Random =
                current != null
                    ? new Random(current.Random.Next())
                    : seed != null
                          ? new Random(seed.Value)
                          : DefaultSeed != null
                                ? new Random(DefaultSeed.Value)
                                : new Random();

            this.parent = current;
            current = this;
        }

        public void Dispose()
        {
            current = current.parent;
        }
    }
}