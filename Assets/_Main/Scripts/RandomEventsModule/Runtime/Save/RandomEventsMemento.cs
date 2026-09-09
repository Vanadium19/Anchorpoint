using System;
using System.Collections.Generic;

namespace RandomEventsModule
{
    /// <summary>Serializable snapshot of <see cref="IRandomEventStateStore"/>, keyed by the raw string key.</summary>
    [Serializable]
    public class RandomEventsMemento
    {
        /// <summary>Saved integer values.</summary>
        public Dictionary<string, int> Ints = new();

        /// <summary>Saved float values.</summary>
        public Dictionary<string, float> Floats = new();

        /// <summary>Saved boolean values.</summary>
        public Dictionary<string, bool> Flags = new();

        /// <summary>Unix timestamp (ms) of when this snapshot was taken.</summary>
        public long SavedAtUnixMs;
    }
}
