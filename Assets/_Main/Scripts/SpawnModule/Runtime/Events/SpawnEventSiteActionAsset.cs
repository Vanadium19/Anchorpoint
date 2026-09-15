using System;
using System.Collections.Generic;
using RandomEventsModule;
using UnityEngine;
using Zenject;

namespace SpawnModule
{
    /// <summary>Asset wrapper for <see cref="SpawnEventSiteAction"/>.</summary>
    /// <remarks>Holds the design-facing knobs: which prefab to place, how many guards, occupants and how much loot it carries, and the search range for a valid spot.</remarks>
    [Serializable]
    public class SpawnEventSiteActionAsset : ZenjectRandomEventActionAsset<SpawnEventSiteAction>
    {
        [Header("Site")]
        [SerializeField] private GameObject sitePrefab;

        [Header("Guards")]
        [SerializeField] private GameObject guardPrefab;
        [SerializeField] [Min(0)] private int guardCount = 3;
        [SerializeField] [Min(0f)] private float guardScatterRadius = 4f;

        [Header("Occupants")]
        [SerializeField] private GameObject occupantPrefab;
        [SerializeField] [Min(0)] private int occupantCount;
        [SerializeField] [Min(0f)] private float occupantScatterRadius = 2f;

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
            Optional(sitePrefab), Optional(guardPrefab), guardCount, guardScatterRadius,
            Optional(occupantPrefab), occupantCount, occupantScatterRadius,
            lootEntries, minimumLootDrops, maximumLootDrops,
            searchRadius, minimumPlayerDistance
        };
    }
}
