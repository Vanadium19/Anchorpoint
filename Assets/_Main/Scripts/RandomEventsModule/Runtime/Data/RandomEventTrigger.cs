using System;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>
    /// One complete rule for starting events: when to check, what must hold, how likely it is,
    /// and which pool to pick from.
    /// </summary>
    /// <remarks>
    /// A scope lists as many triggers as it needs and each one runs on its own clock and its own
    /// odds — a camp can check base events every 30 seconds, raid every 120, and alchemy events
    /// only after the table has been used, all from the same asset. Conditions and the chance roll
    /// happen before any event is picked, so a trigger that does not fire costs no cooldown.
    /// </remarks>
    [Serializable]
    public class RandomEventTrigger
    {
        [SerializeReference] private IRandomEventTriggerSourceAsset source;

        [SerializeField] private RandomEventConditionSet conditions = new();

        [SerializeReference] private IRandomEventChanceAsset chance;

        [SerializeField] private RandomEventPool pool = new();

        /// <summary>Whether this trigger is fully configured and can be run.</summary>
        public bool IsValid => source != null && pool != null;

        /// <summary>Resolves this trigger into its runtime form.</summary>
        public RandomEventTriggerInstance Create(DiContainer container) =>
            new(source.Create(container), conditions.Create(container), chance?.Create(container), pool, pool.CreatePicker(container));
    }
}
