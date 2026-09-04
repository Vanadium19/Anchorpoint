using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>
    /// The default picker: rolls one candidate proportionally to <see cref="RandomEventDefinition.Weight"/>.
    /// This is the built-in behavior when a <see cref="RandomEventPool"/> has no picker asset assigned.
    /// </summary>
    public class WeightedRandomPicker : IRandomEventPicker
    {
        /// <inheritdoc/>
        public RandomEventDefinition Pick(IReadOnlyList<RandomEventDefinition> candidates)
        {
            if (candidates == null || candidates.Count == 0)
                return null;

            var totalWeight = candidates.Sum(d => d.Weight);
            var roll = Random.value * totalWeight;
            var accumulated = 0f;

            foreach (var candidate in candidates)
            {
                accumulated += candidate.Weight;

                if (roll < accumulated)
                    return candidate;
            }

            return candidates[^1];
        }
    }
}
