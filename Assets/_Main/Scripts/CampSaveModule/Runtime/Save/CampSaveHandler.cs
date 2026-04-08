using System;
using Zenject;
using SaveModule;
using InventoryModule;

namespace CampSaveModule
{
    public class CampSaveHandler : IInitializable, IDisposable
    {
        private readonly IGameSaveLoader _gameSaveLoader;
        private readonly CampSaveable _campSaveable;
        private readonly InventorySaveable _inventorySaveable;

        public CampSaveHandler(
            IGameSaveLoader gameSaveLoader,
            CampSaveable campSaveable,
            InventorySaveable inventorySaveable)
        {
            _gameSaveLoader = gameSaveLoader;
            _campSaveable = campSaveable;
            _inventorySaveable = inventorySaveable;
        }

        public void Initialize()
        {
            _gameSaveLoader.RegisterSaveable(_campSaveable);
            _gameSaveLoader.RegisterSaveable(_inventorySaveable);
            _gameSaveLoader.Load();
        }

        public void Dispose()
        {
            _gameSaveLoader.UnregisterSaveable(_campSaveable);
            _gameSaveLoader.UnregisterSaveable(_inventorySaveable);
        }
    }
}
