using System;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>The pacing rules for one scene's random events runner: how it decides *when* to check for an event.</summary>
    /// <remarks>
    /// One per scene installer — the camp can tick on a timer while the shooter checks only after
    /// each extraction. The selection algorithm is not here — it belongs to the pool, see
    /// <see cref="RandomEventPool"/>.
    /// </remarks>
    [Serializable]
    public class RandomEventsRules
    {
        [SerializeField] [Min(0f)] private float minIntervalBetweenEventsSeconds = 0f;
        [SerializeField] [Min(0f)] private float evaluationIntervalSeconds = 5f;

        /// <summary>Minimum pause between one event finishing and the next one starting.</summary>
        public float MinIntervalBetweenEventsSeconds => minIntervalBetweenEventsSeconds;

        /// <summary>How often the periodic loop evaluates; zero disables it.</summary>
        public float EvaluationIntervalSeconds => evaluationIntervalSeconds;
    }
}
