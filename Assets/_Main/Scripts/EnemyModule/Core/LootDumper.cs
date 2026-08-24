using System;
using ComponentsModule;
using InventoryModule;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace EnemyModule
{
    public class LootDumper : IInitializable, IDisposable
    {
        private readonly EnemyConfig _config;
        private readonly IHealthComponent _health;
        private readonly LootViewFactory _lootViewFactory;

        private readonly Transform _transform;

        public LootDumper(EnemyConfig config, IHealthComponent health, Transform transform, LootViewFactory lootViewFactory)
        {
            _config = config;
            _health = health;
            _transform = transform;
            _lootViewFactory = lootViewFactory;
        }

        public void Initialize() => _health.Died += OnDied;

        public void Dispose() => _health.Died -= OnDied;

        private void OnDied() => DropLoot();

        private void DropLoot()
        {
            var count = _config.GetRandomDropCount();

            for (int i = 0; i < count; i++)
            {
                if (!_config.TryGetRandomLootItem(out var item))
                    break;

                SpawnLoot(item);
            }
        }

        private void SpawnLoot(ItemDataSo item)
        {
            if (!item || !item.WorldPrefab || !item.IsDropable)
                return;

            var spawnPosition = _transform.position + Vector3.up * _config.LootSpawnOffsetY;
            var randomOffset = Random.insideUnitCircle * _config.LootScatterRadius;
            spawnPosition += new Vector3(randomOffset.x, 0f, randomOffset.y);

            var lootInstance = _lootViewFactory.Create(item.WorldPrefab, spawnPosition);
            lootInstance.SetItemTable(new(item));
        }
    }
}