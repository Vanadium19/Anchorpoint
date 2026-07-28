using System;
using UnityEngine.SceneManagement;
using Zenject;

namespace SpawnModule
{
    public sealed class WorldSessionController : IInitializable, IDisposable
    {
        private readonly EnemySpawnController _enemySpawnController;
        private readonly LootSpawnController _lootSpawnController;
        private readonly WorldCleanupService _worldCleanupService;

        private Scene _gameplayScene;

        public WorldSessionController(
            EnemySpawnController enemySpawnController,
            LootSpawnController lootSpawnController,
            WorldCleanupService worldCleanupService)
        {
            _enemySpawnController = enemySpawnController;
            _lootSpawnController = lootSpawnController;
            _worldCleanupService = worldCleanupService;
        }

        public void Initialize()
        {
            _gameplayScene = SceneManager.GetActiveScene();
            _enemySpawnController.StartSession();
            _lootSpawnController.SpawnInitialLoot();
        }

        public void Dispose()
        {
            _enemySpawnController.StopSession();
            _worldCleanupService.RemoveTransientObjects(_gameplayScene);
        }
    }
}
