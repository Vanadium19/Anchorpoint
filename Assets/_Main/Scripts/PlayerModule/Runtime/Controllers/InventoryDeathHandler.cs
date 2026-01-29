using System;
using ComponentsModule;
using InventoryModule;
using Zenject;

namespace PlayerModule
{
    public class InventoryDeathHandler : IInitializable, IDisposable
    {
        private readonly PlayerProvider _playerProvider;
        private readonly InventoryModel _inventory;

        public InventoryDeathHandler(PlayerProvider playerProvider, InventoryModel inventory)
        {
            _playerProvider = playerProvider;
            _inventory = inventory;
        }

        public void Initialize()
        {
            if (_playerProvider.TryGet<IHealthComponent>(out var health))
                health.Died += OnDied;
        }

        public void Dispose()
        {
            if (_playerProvider.TryGet<IHealthComponent>(out var health))
                health.Died -= OnDied;
        }

        private void OnDied() => _inventory.Clear();
    }
}