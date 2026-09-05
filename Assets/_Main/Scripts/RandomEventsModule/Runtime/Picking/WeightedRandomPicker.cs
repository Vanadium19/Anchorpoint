using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>
    /// The default picker: rolls one candidate proportionally to the weight its pool gave it.
    /// This is the built-in behavior when a <see cref="RandomEventPool"/> has no picker asset assigned.
    /// </summary>
    public class WeightedRandomPicker : IRandomEventPicker
    {
        /// <inheritdoc/>
        public RandomEventDefinition Pick(IReadOnlyList<RandomEventCandidate> candidates)
        {
            if (candidates == null || candidates.Count == 0)
                return null;

            var totalWeight = candidates.Sum(candidate => candidate.Weight);
            var roll = Random.value * totalWeight;
            var accumulatedWeight = 0f;

            foreach (var candidate in candidates)
            {
                accumulatedWeight += candidate.Weight;

                if (roll < accumulatedWeight)
                    return candidate.Definition;
            }

            return candidates[^1].Definition;
        }
    }
}
