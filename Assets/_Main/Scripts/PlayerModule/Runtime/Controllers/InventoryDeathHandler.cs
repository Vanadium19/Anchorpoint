using System;
using ComponentsModule;
using InventoryModule;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace PlayerModule
{
    public class InventoryDeathHandler : IInitializable, IDisposable
    {
        private readonly IHealthComponent _health;
        private readonly IInventoryManager _inventoryManager;
        private readonly IDeathLootStorage _deathLootStorage;
        private readonly IDeathLootSpawner _deathLootSpawner;
        private readonly Transform _playerTransform;

        public InventoryDeathHandler(
            IHealthComponent health,
            IInventoryManager inventoryManager,
            IDeathLootStorage deathLootStorage,
            IDeathLootSpawner deathLootSpawner,
            Transform playerTransform)
        {
            _health = health;
            _inventoryManager = inventoryManager;
            _deathLootStorage = deathLootStorage;
            _deathLootSpawner = deathLootSpawner;
            _playerTransform = playerTransform;
        }

        public void Initialize()
        {
            _health.Died += OnDied;
        }

        public void Dispose()
        {
            _health.Died -= OnDied;
        }

        private void OnDied()
        {
            var items = _inventoryManager.ExtractAllRootItems();
            var sceneName = SceneManager.GetActiveScene().name;

            var pile = _deathLootStorage.CreatePile(sceneName, _playerTransform.position, items);

            if (pile != null)
                _deathLootSpawner.SpawnPile(pile);
        }
    }
}