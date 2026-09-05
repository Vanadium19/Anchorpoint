using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>The set of events one trigger can start, each with its own weight, plus the algorithm that picks between them.</summary>
    /// <remarks>Leave <c>picker</c> empty to use <see cref="WeightedRandomPicker"/>.</remarks>
    [Serializable]
    public class RandomEventPool
    {
        [SerializeReference] private IRandomEventPickerAsset picker;

        [SerializeField] private List<RandomEventPoolEntry> entries = new();

        /// <summary>Events this pool can pick from, with their weights.</summary>
        public IReadOnlyList<RandomEventPoolEntry> Entries => entries;

        /// <summary>Resolves the assigned picker, or a <see cref="WeightedRandomPicker"/> if none is assigned.</summary>
        public IRandomEventPicker CreatePicker(DiContainer container) =>
            picker != null ? picker.Create(container) : new WeightedRandomPicker();
    }
}
