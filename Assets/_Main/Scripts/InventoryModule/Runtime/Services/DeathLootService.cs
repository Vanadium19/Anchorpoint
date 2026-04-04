using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using Random = UnityEngine.Random;

namespace InventoryModule
{
    public class DeathLootService : IDeathLootService, IInitializable, IDisposable
    {
        private class DeathLootPile
        {
            public string SceneName;
            public Vector3 Position;
            public readonly List<ItemTable> Items = new();
        }

        private static readonly List<DeathLootPile> Piles = new();

        private readonly IDropService _dropService;
        private readonly float _scatterRadius;
        private readonly float _spawnOffsetY;

        public DeathLootService(
            IDropService dropService,
            float scatterRadius = 1.25f,
            float spawnOffsetY = 0.35f)
        {
            _dropService = dropService;
            _scatterRadius = scatterRadius;
            _spawnOffsetY = spawnOffsetY;
        }

        public void Initialize()
        {
            RestoreForScene(SceneManager.GetActiveScene().name);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void CreatePile(IReadOnlyList<ItemTable> items, Vector3 position)
        {
            if (items == null || items.Count == 0)
                return;

            var pile = new DeathLootPile
            {
                SceneName = SceneManager.GetActiveScene().name,
                Position = position
            };

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];

                if (item == null || item.ItemDataSo == null || !item.ItemDataSo.IsDropable)
                    continue;

                pile.Items.Add(item);
            }

            if (pile.Items.Count == 0)
                return;

            Piles.Add(pile);
            SpawnPile(pile);
        }

        public void RemoveCollectedItem(ItemTable item)
        {
            if (item == null)
                return;

            for (var pileIndex = Piles.Count - 1; pileIndex >= 0; pileIndex--)
            {
                var pile = Piles[pileIndex];

                for (var itemIndex = pile.Items.Count - 1; itemIndex >= 0; itemIndex--)
                {
                    if (pile.Items[itemIndex] != item)
                        continue;

                    pile.Items.RemoveAt(itemIndex);
                    break;
                }

                if (pile.Items.Count == 0)
                    Piles.RemoveAt(pileIndex);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RestoreForScene(scene.name);
        }

        private void RestoreForScene(string sceneName)
        {
            for (var i = 0; i < Piles.Count; i++)
            {
                var pile = Piles[i];

                if (pile.SceneName != sceneName)
                    continue;

                SpawnPile(pile);
            }
        }

        private void SpawnPile(DeathLootPile pile)
        {
            for (var i = 0; i < pile.Items.Count; i++)
            {
                var item = pile.Items[i];

                if (item == null || item.ItemDataSo == null)
                    continue;

                var spawnPosition = GetSpawnPosition(pile.Position);
                _dropService.TryDropItem(item, spawnPosition);
            }
        }

        private Vector3 GetSpawnPosition(Vector3 centerPosition)
        {
            var randomOffset = Random.insideUnitCircle * _scatterRadius;

            return new Vector3(
                centerPosition.x + randomOffset.x,
                centerPosition.y + _spawnOffsetY,
                centerPosition.z + randomOffset.y);
        }
    }
}