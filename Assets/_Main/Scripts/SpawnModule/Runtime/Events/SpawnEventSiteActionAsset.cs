using System;
using System.Collections.Generic;
using RandomEventsModule;
using UnityEngine;
using Zenject;

namespace SpawnModule
{
    /// <summary>Asset wrapper for <see cref="SpawnEventSiteAction"/>.</summary>
    /// <remarks>Holds the design-facing knobs: which prefab to place, its units and how much loot it carries, and the search range for a valid spot.</remarks>
    [Serializable]
    public class SpawnEventSiteActionAsset : ZenjectRandomEventActionAsset<SpawnEventSiteAction>
    {
        [Header("Site")]
        [SerializeField] private GameObject sitePrefab;

        [Header("Units")]
        [SerializeField] private List<UnitEntry> unitEntries = new();
        [SerializeField] [Min(0)] private int unitCount = 3;
        [SerializeField] [Min(0f)] private float unitScatterRadius = 4f;

        [Header("Loot")]
        [SerializeField] private List<LootEntry> lootEntries = new();
        [SerializeField] [Min(0)] private int minimumLootDrops = 2;
        [SerializeField] [Min(0)] private int maximumLootDrops = 4;

        [Header("Placement")]
        [SerializeField] [Min(0f)] private float searchRadius = 25f;
        [SerializeField] [Min(0f)] private float minimumPlayerDistance = 20f;

        /// <inheritdoc/>
        protected override object[] GetArguments(DiContainer container) => new object[]
        {
            Optional(sitePrefab), unitEntries, unitCount, unitScatterRadius,
            lootEntries, minimumLootDrops, maximumLootDrops,
            searchRadius, minimumPlayerDistance
        };
    }
}
