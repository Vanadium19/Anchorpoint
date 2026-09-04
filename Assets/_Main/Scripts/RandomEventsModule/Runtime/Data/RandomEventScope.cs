using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>A scope identifier plus the pools of events it can draw from.</summary>
    /// <remarks>
    /// Split one scope's events into several <see cref="RandomEventPool"/>s when more than one
    /// selection algorithm is needed — a single pool is enough for the common case. All of a
    /// scope's pools roll independently on every evaluation, not against each other.
    /// </remarks>
    [CreateAssetMenu(fileName = "RandomEventScope", menuName = "Game/Configs/RandomEvents/RandomEventScope")]
    public class RandomEventScope : ScriptableObject
    {
        [ListDrawerSettings(ShowFoldout = true, DraggableItems = true)]
        [SerializeField] private List<RandomEventPool> pools = new();

        /// <summary>This scope's independently-rolling pools.</summary>
        public IReadOnlyList<RandomEventPool> Pools => pools;
    }
}
