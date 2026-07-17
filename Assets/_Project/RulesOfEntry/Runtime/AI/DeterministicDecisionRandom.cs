namespace RulesOfEntry.AI
{
    /// <summary>
    /// Small deterministic generator used only for replayable AI decisions.
    /// </summary>
    public sealed class DeterministicDecisionRandom
    {
        private uint state;

        public DeterministicDecisionRandom(int seed)
        {
            state = unchecked((uint)seed);
            if (state == 0)
            {
                state = 0x6D2B79F5u;
            }
        }

        public float Next01()
        {
            uint value = NextUInt();
            return (value & 0x00FFFFFFu) / 16777216f;
        }

        public float Range(float minimum, float maximum)
        {
            return minimum + (maximum - minimum) * Next01();
        }

        private uint NextUInt()
        {
            uint value = state;
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            state = value;
            return value;
        }
    }
}
