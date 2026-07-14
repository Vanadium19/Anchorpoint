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
        private readonly WorkbenchSaveable _workbenchSaveable;

        public CampSaveHandler(
            [Inject(Id = GameSaveLoaderIds.Game)] IGameSaveLoader gameSaveLoader,
            CampSaveable campSaveable,
            InventorySaveable inventorySaveable,
            BuffSaveable buffSaveable,
            WorkbenchSaveable workbenchSaveable)
        {
            _gameSaveLoader = gameSaveLoader;
            _campSaveable = campSaveable;
            _inventorySaveable = inventorySaveable;
            _buffSaveable = buffSaveable;
            _workbenchSaveable = workbenchSaveable;
        }

        public void Initialize()
        {
            _gameSaveLoader.RegisterSaveable(_campSaveable);
            _gameSaveLoader.RegisterSaveable(_inventorySaveable);
            _gameSaveLoader.RegisterSaveable(_buffSaveable);
            _gameSaveLoader.RegisterSaveable(_workbenchSaveable);
            _gameSaveLoader.Load();
        }

        public void Dispose()
        {
            _gameSaveLoader.UnregisterSaveable(_campSaveable);
            _gameSaveLoader.UnregisterSaveable(_inventorySaveable);
            _gameSaveLoader.UnregisterSaveable(_buffSaveable);
            _gameSaveLoader.UnregisterSaveable(_workbenchSaveable);
        }
    }
}
