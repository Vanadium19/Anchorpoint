using System;
using Zenject;
using SaveModule;
using InventoryModule;

namespace GameSceneSaveModule
{
    public class GameSceneSaveHandler : IInitializable, IDisposable
    {
        private readonly IGameSaveLoader _gameSaveLoader;
        private readonly InventorySaveable _inventorySaveable;

        public GameSceneSaveHandler(IGameSaveLoader gameSaveLoader, InventorySaveable inventorySaveable)
        {
            _gameSaveLoader = gameSaveLoader;
            _inventorySaveable = inventorySaveable;
        }

        public void Initialize()
        {
            _gameSaveLoader.RegisterSaveable(_inventorySaveable);
            _gameSaveLoader.Load();
        }

        public void Dispose()
        {
            _gameSaveLoader.UnregisterSaveable(_inventorySaveable);
        }
    }
}
