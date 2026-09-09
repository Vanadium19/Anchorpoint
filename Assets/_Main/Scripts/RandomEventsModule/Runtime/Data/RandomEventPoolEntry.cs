using System;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>One event inside a pool, with the weight that pool gives it.</summary>
    /// <remarks>Weight lives here rather than on the definition, so the same event can be common in one pool and rare in another.</remarks>
    [Serializable]
    public class RandomEventPoolEntry
    {
        [SerializeField] private RandomEventDefinition definition;
        [SerializeField] [Min(0f)] private float weight = 1f;

        /// <summary>The event this entry points at.</summary>
        public RandomEventDefinition Definition => definition;

        /// <summary>Selection weight inside the owning pool.</summary>
        public float Weight => weight;
    }
}
