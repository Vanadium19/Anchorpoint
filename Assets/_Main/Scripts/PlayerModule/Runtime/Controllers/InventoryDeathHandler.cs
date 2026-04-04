using System;
using ComponentsModule;
using InventoryModule;
using UnityEngine;
using Zenject;

namespace PlayerModule
{
    public class InventoryDeathHandler : IInitializable, IDisposable
    {
        private readonly IHealthComponent _health;
        private readonly IInventoryManager _inventoryManager;
        private readonly IDeathLootService _deathLootService;
        private readonly Transform _playerTransform;

        public InventoryDeathHandler(
            IHealthComponent health,
            IInventoryManager inventoryManager,
            IDeathLootService deathLootService,
            Transform playerTransform)
        {
            _health = health;
            _inventoryManager = inventoryManager;
            _deathLootService = deathLootService;
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
            _deathLootService.CreatePile(items, _playerTransform.position);
        }
    }
}