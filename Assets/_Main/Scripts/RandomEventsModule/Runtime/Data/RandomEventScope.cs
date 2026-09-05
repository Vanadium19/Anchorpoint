using System.Collections.Generic;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>Everything one scene's random events need: the triggers that can start events and the pacing shared by all of them.</summary>
    /// <remarks>A scene can hold several scopes, each installed on its own context; each scope drives its triggers independently.</remarks>
    [CreateAssetMenu(fileName = "RandomEventScope", menuName = "Game/Configs/RandomEvents/RandomEventScope")]
    public class RandomEventScope : ScriptableObject
    {
        [SerializeField] [Min(0f)] private float minIntervalBetweenEventsSeconds;

        [SerializeField] private List<RandomEventTrigger> triggers = new();

        /// <summary>Minimum pause between one event finishing and the next one starting, across every trigger of this scope.</summary>
        public float MinIntervalBetweenEventsSeconds => minIntervalBetweenEventsSeconds;

        /// <summary>The rules that can start events in this scope.</summary>
        public IReadOnlyList<RandomEventTrigger> Triggers => triggers;
    }
}
