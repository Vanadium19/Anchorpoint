using System.Collections.Generic;
using UnityEngine;

namespace SpawnModule
{
    /// <summary>
    /// Weighted set of enemy prefabs a scene picks from when it spawns an enemy without naming a type.
    /// </summary>
    public sealed class EnemyVariants
    {
        private readonly List<UnitEntry> _entries = new();

        /// <summary>
        /// Keeps only the entries that have a prefab and a positive weight.
        /// </summary>
        public EnemyVariants(IEnumerable<UnitEntry> entries)
        {
            if (entries == null)
                return;

            foreach (var entry in entries)
            {
                if (entry != null && entry.Prefab != null && entry.Weight > 0)
                    _entries.Add(entry);
            }
        }

        /// <summary>
        /// Picks a prefab by weight. Returns false when the set is empty.
        /// </summary>
        public bool TryPick(out GameObject prefab)
        {
            prefab = null;

            var totalWeight = 0;

            foreach (var entry in _entries)
                totalWeight += entry.Weight;

            if (totalWeight <= 0)
                return false;

            var roll = Random.Range(0, totalWeight);

            foreach (var entry in _entries)
            {
                roll -= entry.Weight;

                if (roll >= 0)
                    continue;

                prefab = entry.Prefab;
                return true;
            }

            return false;
        }
    }
}
