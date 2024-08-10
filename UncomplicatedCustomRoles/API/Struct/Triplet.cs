using Newtonsoft.Json;

namespace UncomplicatedCustomRoles.API.Struct
{
    public readonly struct Triplet<TFirst, TSecond, TThird>
    {
        /// <summary>
        /// Gets the first value
        /// </summary>
        public TFirst First { get; }

        /// <summary>
        /// Gets the second value
        /// </summary>
        public TSecond Second { get; }

        /// <summary>
        /// Gets the third value
        /// </summary>
        public TThird Third { get; }

        [JsonConstructor]
        public Triplet(TFirst first, TSecond second, TThird third)
        {
            First = first;
            Second = second;
            Third = third;
        }

        public Triplet(Triplet<TFirst, TSecond, TThird> clone)
        {
            First = clone.First;
            Second = clone.Second;
            Third = clone.Third;
        }

        public override string ToString() => $"({First}, {Second}, {Third})";
    }
}
