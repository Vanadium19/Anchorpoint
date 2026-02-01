using System;
using ComponentsModule;
using InventoryModule;
using Zenject;

namespace PlayerModule
{
    public class InventoryDeathHandler : IInitializable, IDisposable
    {
        private readonly IHealthComponent _health;
        private readonly IInventoryService _inventoryService;
        public InventoryDeathHandler(IHealthComponent health, IInventoryService inventoryService)
        {
            _health = health;
            _inventoryService = inventoryService;
        }

        public void Initialize()
        {
            _health.Died += OnDied;
        }

        public void Dispose()
        {
            _health.Died -= OnDied;
        }

        private void OnDied() => _inventoryService.ClearInventory();
    }
}