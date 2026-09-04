using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>
    /// A single event, fully described by data. The asset itself is the event's identity —
    /// the service keys active instances and cooldowns by it, so no string id is needed.
    /// </summary>
    /// <remarks>
    /// A definition does not know which pool or scope it belongs to — that membership is decided
    /// by whichever <see cref="RandomEventPool"/> lists it, inside whichever <see cref="RandomEventScope"/>.
    /// </remarks>
    [CreateAssetMenu(fileName = "RandomEventDefinition", menuName = "Game/Configs/RandomEvents/RandomEventDefinition")]
    public class RandomEventDefinition : ScriptableObject
    {
        [SerializeField] [Min(0f)] private float weight = 1f;
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private bool isExclusive = true;
        [SerializeField] [Min(0f)] private float cooldownSeconds = 0f;

        [Header("Actions")]
        [SerializeField] private List<RandomEventActionStep> actionSteps = new();

        [Header("Localization")]
        [SerializeField] private string displayNameKey;

        /// <summary>Weight used by the default weighted picker.</summary>
        public float Weight => weight;

        /// <summary>Master switch for this event.</summary>
        public bool IsEnabled => isEnabled;

        /// <summary>Whether another event can start while this one is running.</summary>
        public bool IsExclusive => isExclusive;

        /// <summary>Pause before this event can run again after it finishes.</summary>
        public float CooldownSeconds => cooldownSeconds;

        /// <summary>Localization key for this event's display name.</summary>
        public string DisplayNameKey => displayNameKey;

        /// <summary>Creates an immutable snapshot for a running instance of this event.</summary>
        public RandomEventInfo CreateInfo() => new(this, displayNameKey, isExclusive);

        /// <summary>Resolves the flat step list into groups of actions to run in sequence.</summary>
        public IReadOnlyList<IReadOnlyList<IRandomEventAction>> CreateActionGroups(DiContainer container)
        {
            var groups = new List<List<IRandomEventAction>>();

            foreach (var step in actionSteps)
            {
                var action = step?.Create(container);

                if (action == null)
                    continue;

                if (step.RunInParallel && groups.Count > 0)
                    groups[^1].Add(action);
                else
                    groups.Add(new List<IRandomEventAction> { action });
            }

            return groups;
        }
    }
}
