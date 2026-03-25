using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using EnemyModule;
using Zenject;

namespace SpawnModule
{
    public class EnemySpawnController : IInitializable, IDisposable
    {
        private readonly LevelSpawnPointsView _view;
        private readonly IEnemyFactory _factory;
        private readonly SpawnConfig _config;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _isSpawning;
        
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
            StartSpawning();
        }

        public void StartSpawning()
        {
            if (_isSpawning) return;
        
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            _isSpawning = true;
        
            SpawnLoopAsync(_cts.Token).Forget();
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

        private async UniTaskVoid SpawnLoopAsync(CancellationToken token)
        {
            await UniTask.WaitForSeconds(_config.WaitTime, cancellationToken: token);

            while (!token.IsCancellationRequested && _isSpawning)
            {
                Spawn();
                await UniTask.WaitForSeconds(_config.SpawnInterval, cancellationToken: token);
            }
        }
        
        public void StopSpawning()
        {
            _isSpawning = false;
            _cts?.Cancel();
        }
        
        public void Dispose()
        {
            StopSpawning();
            _cts?.Dispose();
        }
    }
}