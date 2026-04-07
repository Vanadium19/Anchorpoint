using System;
using Zenject;
using SaveModule;

namespace InventoryModule
{
    public class InventorySaveRegistration : IInitializable, IDisposable
    {
        private readonly IGameSaveLoader _gameSaveLoader;
        private readonly InventorySaveable _inventorySaveable;

        public InventorySaveRegistration(IGameSaveLoader gameSaveLoader, InventorySaveable inventorySaveable)
        {
            _gameSaveLoader = gameSaveLoader;
            _inventorySaveable = inventorySaveable;
        }

        public void Initialize()
        {
            _gameSaveLoader.RegisterSaveable(_inventorySaveable);
        }

        public void Dispose()
        {
            _gameSaveLoader.UnregisterSaveable(_inventorySaveable);
        }
    }
}
