using System;
using Zenject;
using SaveModule;
using InventoryModule;
using EffectModule;

namespace CampSaveModule
{
    public class CampSaveHandler : IInitializable, IDisposable
    {
        private readonly IGameSaveLoader _gameSaveLoader;
        private readonly CampSaveable _campSaveable;
        private readonly InventorySaveable _inventorySaveable;
        private readonly BuffSaveable _buffSaveable;

        public CampSaveHandler(
            IGameSaveLoader gameSaveLoader,
            CampSaveable campSaveable,
            InventorySaveable inventorySaveable,
            BuffSaveable buffSaveable)
        {
            _gameSaveLoader = gameSaveLoader;
            _campSaveable = campSaveable;
            _inventorySaveable = inventorySaveable;
            _buffSaveable = buffSaveable;
        }

        public void Initialize()
        {
            _gameSaveLoader.RegisterSaveable(_campSaveable);
            _gameSaveLoader.RegisterSaveable(_inventorySaveable);
            _gameSaveLoader.RegisterSaveable(_buffSaveable);
            _gameSaveLoader.Load();
        }

        public void Dispose()
        {
            _gameSaveLoader.UnregisterSaveable(_campSaveable);
            _gameSaveLoader.UnregisterSaveable(_inventorySaveable);
            _gameSaveLoader.UnregisterSaveable(_buffSaveable);
        }
    }
}
