using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using InventoryModule;
using UnityEngine;
using Zenject;

namespace SpawnModule
{
    public class LootSpawnController : IInitializable
    {
        private readonly LevelLootSpawnPointsView _spawnPointsView;
        private readonly ILootFactory _factory;

        public LootSpawnController(
            LevelLootSpawnPointsView spawnPointsView,
            ILootFactory factory)
        {
            _spawnPointsView = spawnPointsView;
            _factory = factory;
        }

        public void Initialize()
        {
            SpawnLoot();
        }

        private void SpawnLoot()
        {
            if (_spawnPointsView == null || _spawnPointsView.SpawnPoints == null)
                return;

            var spawnPoints = _spawnPointsView.SpawnPoints;

            foreach (var spawnPoint in spawnPoints)
            {
                SpawnAtPoint(spawnPoint);
            }
        }

        private void SpawnAtPoint(LootSpawnPointView spawnPoint)
        {
            var entries = spawnPoint.GetLootEntries();

            if (entries == null || entries.Count == 0)
                return;

            int totalWeight = entries.Sum(e => e.Weight);

            if (totalWeight == 0)
                return;

            int itemsToSpawn = GetItemsToSpawn(spawnPoint);

            for (int i = 0; i < itemsToSpawn; i++)
            {
                var selectedEntry = SelectByWeight(entries, totalWeight);
                
                if (selectedEntry == null)
                    continue;

                if (selectedEntry.Item == null)
                    continue;

                int count = UnityEngine.Random.Range(selectedEntry.MinCount, selectedEntry.MaxCount + 1);
                Vector3 position = GetSpawnPosition(spawnPoint);

                _factory.Create(position, selectedEntry.Item, count, selectedEntry.IsStacked);
            }
        }

        private int GetItemsToSpawn(LootSpawnPointView spawnPoint)
        {
            int min = spawnPoint.GetMinItemsToSpawn();
            int max = spawnPoint.GetMaxItemsToSpawn();
            return min == max ? min : UnityEngine.Random.Range(min, max + 1);
        }

        private LootEntry SelectByWeight(List<LootEntry> entries, int totalWeight)
        {
            int random = UnityEngine.Random.Range(1, totalWeight + 1);

            for (int i = 0; i < entries.Count; i++)
            {
                if (random <= entries[i].Weight)
                    return entries[i];

                random -= entries[i].Weight;
            }

            return entries[entries.Count - 1];
        }

        private Vector3 GetSpawnPosition(LootSpawnPointView spawnPoint)
        {
            float radius = spawnPoint.GetSpawnRadius();

            if (radius <= 0)
                return spawnPoint.transform.position;

            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * radius;
            return new Vector3(
                spawnPoint.transform.position.x + randomCircle.x,
                spawnPoint.transform.position.y,
                spawnPoint.transform.position.z + randomCircle.y
            );
        }
    }
}
