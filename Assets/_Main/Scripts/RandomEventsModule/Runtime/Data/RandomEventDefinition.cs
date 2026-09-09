using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>
    /// A single event, fully described by data. The asset itself is the event's identity —
    /// the service keys active instances and cooldowns by it, so no string id is needed.
    /// </summary>
    /// <remarks>
    /// A definition knows nothing about when it runs or how likely it is: triggers, chance and
    /// weight belong to whichever <see cref="RandomEventPool"/> lists it, inside whichever
    /// <see cref="RandomEventScope"/>. The same definition can sit in several pools at once.
    /// </remarks>
    [CreateAssetMenu(fileName = "RandomEventDefinition", menuName = "Game/Configs/RandomEvents/RandomEventDefinition")]
    public class RandomEventDefinition : ScriptableObject
    {
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private bool isExclusive = true;
        [SerializeField] [Min(0f)] private float cooldownSeconds;

        [SerializeField] private RandomEventActionSequence sequence = new();

        [SerializeField] private string displayNameKey;

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

        /// <summary>Resolves this event's action sequence into a runnable plan.</summary>
        public RandomEventActionPlan CreatePlan(DiContainer container) => sequence.Create(container);
    }
}
