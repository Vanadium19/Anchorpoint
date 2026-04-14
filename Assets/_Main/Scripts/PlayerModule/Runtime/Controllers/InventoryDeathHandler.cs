using System;
using ComponentsModule;
using InventoryModule;
using Zenject;

namespace PlayerModule
{
    public class InventoryDeathHandler : IInitializable, IDisposable
    {
        private readonly IHealthComponent _health;
        private readonly IInventoryManager _inventoryManager;

        public InventoryDeathHandler(IHealthComponent health, IInventoryManager inventoryManager)
        {
            _health = health;
            _inventoryManager = inventoryManager;
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
            _inventoryManager.ClearInventory();
        }
    }
}