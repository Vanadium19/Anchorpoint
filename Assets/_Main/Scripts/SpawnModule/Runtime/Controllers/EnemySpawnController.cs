using System;
using System.Linq;
using Zenject;

namespace SpawnModule
{
    public class EnemySpawnController : IInitializable
    {
        private readonly LevelSpawnPointsView _view;
        private readonly IEnemyFactory _factory;
        private readonly SpawnConfig _config;
        // Надо подумать, где сохранять врагов
        
        public EnemySpawnController(
            LevelSpawnPointsView view,
            IEnemyFactory factory,
            SpawnConfig config)
        {
            _view = view;
            _factory = factory;
            _config = config;
        }
        
        public void Initialize()
        {
            Spawn();
        }
        
        public void Spawn()
        {
            var points = _view.SpawnPoints;

            if (_config.EnemyCount > points.Count)
                throw new Exception("EnemyCount > SpawnPoints");

            var shuffled = points
                .OrderBy(_ => UnityEngine.Random.value)
                .ToList();

            for (int i = 0; i < _config.EnemyCount; i++)
            {
                var point = shuffled[i];
                _factory.Create(point.Position);
            }
        }
    }
}