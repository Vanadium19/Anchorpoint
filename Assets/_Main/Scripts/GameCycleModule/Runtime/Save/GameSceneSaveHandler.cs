using System;
using Zenject;
using SaveModule;
using InventoryModule;
using EffectModule;

namespace GameCycleModule
{
    public class GameSceneSaveHandler : IInitializable, IDisposable
    {
        private readonly IGameSaveLoader _gameSaveLoader;
        private readonly InventorySaveable _inventorySaveable;
        private readonly BuffSaveable _buffSaveable;
        private readonly DeathLootSaveable _deathLootSaveable;
        private readonly IDeathLootSpawner _deathLootSpawner;

        public GameSceneSaveHandler(
            [Inject(Id = GameSaveLoaderIds.Game)] IGameSaveLoader gameSaveLoader,
            InventorySaveable inventorySaveable,
            BuffSaveable buffSaveable,
            DeathLootSaveable deathLootSaveable,
            IDeathLootSpawner deathLootSpawner)
        {
            _gameSaveLoader = gameSaveLoader;
            _inventorySaveable = inventorySaveable;
            _buffSaveable = buffSaveable;
            _deathLootSaveable = deathLootSaveable;
            _deathLootSpawner = deathLootSpawner;
        }

        public void Initialize()
        {
            _gameSaveLoader.RegisterSaveable(_inventorySaveable);
            _gameSaveLoader.RegisterSaveable(_buffSaveable);
            _gameSaveLoader.RegisterSaveable(_deathLootSaveable);
            _gameSaveLoader.Load();
            _deathLootSpawner.SpawnStoredPiles();
        }

        public void Dispose()
        {
            _gameSaveLoader.UnregisterSaveable(_inventorySaveable);
            _gameSaveLoader.UnregisterSaveable(_buffSaveable);
            _gameSaveLoader.UnregisterSaveable(_deathLootSaveable);
        }
    }
}
