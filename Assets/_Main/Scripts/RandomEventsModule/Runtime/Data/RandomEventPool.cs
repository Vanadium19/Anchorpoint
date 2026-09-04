using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>A named subset of a scope's events with its own selection algorithm: a picker and the events it picks from.</summary>
    /// <remarks>
    /// A scope can list several pools at once, each rolling independently on every evaluation —
    /// e.g. a "common" pool with the default weighted pick and a "rare" pool that never repeats
    /// the same event twice in a row — without adding another scope, scene, or runner.
    /// </remarks>
    [Serializable]
    public class RandomEventPool
    {
        [SerializeReference] private IRandomEventPickerAsset picker;

        [ListDrawerSettings(ShowFoldout = true, DraggableItems = true)]
        [SerializeField] private List<RandomEventDefinition> definitions = new();

        /// <summary>Events this pool can pick from.</summary>
        public IReadOnlyList<RandomEventDefinition> Definitions => definitions;

        /// <summary>Resolves the assigned picker, or a <see cref="WeightedRandomPicker"/> if none is assigned.</summary>
        public IRandomEventPicker CreatePicker(DiContainer container) =>
            picker != null ? picker.Create(container) : new WeightedRandomPicker();
    }
}
