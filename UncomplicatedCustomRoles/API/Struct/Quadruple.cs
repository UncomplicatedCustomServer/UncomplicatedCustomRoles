namespace UncomplicatedCustomRoles.API.Struct
{
    public readonly struct Quadruple<TFirst, TSecond, TThird, TFourth>
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

        /// <summary>
        /// Gets the fourth value
        /// </summary>
        public TFourth Fourth { get; }

        public Quadruple(TFirst first, TSecond second, TThird third, TFourth fourth)
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
        }

        public override string ToString() => $"({First}, {Second}, {Third}, {Fourth})";
    }
}
