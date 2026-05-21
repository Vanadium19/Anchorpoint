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

        public GameSceneSaveHandler(
            [Inject(Id = GameSaveLoaderIds.Game)] IGameSaveLoader gameSaveLoader,
            InventorySaveable inventorySaveable,
            BuffSaveable buffSaveable)
        {
            _gameSaveLoader = gameSaveLoader;
            _inventorySaveable = inventorySaveable;
            _buffSaveable = buffSaveable;
        }

        public void Initialize()
        {
            _gameSaveLoader.RegisterSaveable(_inventorySaveable);
            _gameSaveLoader.RegisterSaveable(_buffSaveable);
            _gameSaveLoader.Load();
        }

        public void Dispose()
        {
            _gameSaveLoader.UnregisterSaveable(_inventorySaveable);
            _gameSaveLoader.UnregisterSaveable(_buffSaveable);
        }
    }
}
