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
        private readonly DeathLootSaveable _deathLootSaveable;
        private readonly IDeathLootSpawner _deathLootSpawner;

        public CampSaveHandler(
            [Inject(Id = GameSaveLoaderIds.Game)] IGameSaveLoader gameSaveLoader,
            CampSaveable campSaveable,
            InventorySaveable inventorySaveable,
            BuffSaveable buffSaveable,
            WorkbenchSaveable workbenchSaveable)
            DeathLootSaveable deathLootSaveable,
            IDeathLootSpawner deathLootSpawner)
        {
            _gameSaveLoader = gameSaveLoader;
            _campSaveable = campSaveable;
            _inventorySaveable = inventorySaveable;
            _buffSaveable = buffSaveable;
            _workbenchSaveable = workbenchSaveable;
            _deathLootSaveable = deathLootSaveable;
            _deathLootSpawner = deathLootSpawner;
        }

        public void Initialize()
        {
            _gameSaveLoader.RegisterSaveable(_campSaveable);
            _gameSaveLoader.RegisterSaveable(_inventorySaveable);
            _gameSaveLoader.RegisterSaveable(_buffSaveable);
            _gameSaveLoader.RegisterSaveable(_workbenchSaveable);
            _gameSaveLoader.RegisterSaveable(_deathLootSaveable);
            _gameSaveLoader.Load();
            _deathLootSpawner.SpawnStoredPiles();
        }

        public void Dispose()
        {
            _gameSaveLoader.UnregisterSaveable(_campSaveable);
            _gameSaveLoader.UnregisterSaveable(_inventorySaveable);
            _gameSaveLoader.UnregisterSaveable(_buffSaveable);

            _gameSaveLoader.UnregisterSaveable(_workbenchSaveable);
            _gameSaveLoader.UnregisterSaveable(_deathLootSaveable);
        }
    }
}
