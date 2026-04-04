using System;
using ComponentsModule;
using InventoryModule;
using WeaponModule;
using Zenject;

namespace PlayerModule
{
    public class InventoryDeathHandler : IInitializable, IDisposable
    {
        private readonly IHealthComponent _health;
        private readonly IInventoryManager _inventoryManager;
        private readonly AmmoReserveService _ammoReserveService;

        public InventoryDeathHandler(
            IHealthComponent health,
            IInventoryManager inventoryManager,
            AmmoReserveService ammoReserveService)
        {
            _health = health;
            _inventoryManager = inventoryManager;
            _ammoReserveService = ammoReserveService;
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
            _ammoReserveService.Clear();
        }
    }
}