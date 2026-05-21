using System;
using BaseModule;
using SaveModule;
using Zenject;

namespace MenuModule
{
    public class VictorySaveDeleteController : IInitializable, IDisposable
    {
        private readonly IBaseLevelService _baseLevelService;
        private readonly IGameSaveLoader _gameSaveLoader;

        public VictorySaveDeleteController(IBaseLevelService baseLevelService,
            IGameSaveLoader gameSaveLoader)
        {
            _baseLevelService = baseLevelService;
            _gameSaveLoader = gameSaveLoader;
        }

        public void Initialize()
        {
            DeleteSave();

            _baseLevelService.LevelChanged += OnLevelChanged;
        }

        public void Dispose()
        {
            _baseLevelService.LevelChanged -= OnLevelChanged;
        }

        private void OnLevelChanged(int oldLevel, int newLevel) => DeleteSave();

        private void DeleteSave()
        {
            if (!_baseLevelService.IsTargetLevelReached)
                return;

            _gameSaveLoader.DeleteSave();
        }
    }
}