using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BaseModule;
using Cysharp.Threading.Tasks;
using EnemyModule;
using UnityEngine;
using Zenject;

namespace SpawnModule
{
    public class EnemySpawnController : IInitializable, IDisposable, IPausable
    {
        private readonly LevelSpawnPointsView _view;
        private readonly IEnemyFactory _factory;
        private readonly SpawnConfig _config;
        private readonly IPauseManager _pauseManager;

        private CancellationTokenSource _cts = new();
        private bool _isSpawning;
        private bool _isPaused;
        
        public EnemySpawnController(
            LevelSpawnPointsView view,
            IEnemyFactory factory,
            SpawnConfig config,
            IPauseManager pauseManager)
        {
            _view = view;
            _factory = factory;
            _config = config;
            _pauseManager = pauseManager;
        }
        
        public void Initialize()
        {
            _pauseManager.Register(this);
            StartSpawning();
        }

        public void StartSpawning()
        {
            if (_isSpawning)
                return;
        
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

            for (var i = 0; i < _config.EnemyCount; i++)
            {
                var point = shuffled[i];
                _factory.Create(point.Position);
            }
        }

        public void StopSpawning()
        {
            _isSpawning = false;
            _cts?.Cancel();
        }

        public void SetPaused(bool isPaused) => _isPaused = isPaused;
        
        public void Dispose()
        {
            _pauseManager.Unregister(this);
            StopSpawning();
            _cts?.Dispose();
        }

        private async UniTaskVoid SpawnLoopAsync(CancellationToken token)
        {
            var canceled = await WaitWhilePlayableAsync(_config.WaitTime, token);

            if (canceled)
                return;

            while (!token.IsCancellationRequested && _isSpawning)
            {
                if (!_isPaused)
                    Spawn();

                canceled = await WaitWhilePlayableAsync(_config.SpawnInterval, token);

                if (canceled)
                    return;
            }
        }

        private async UniTask<bool> WaitWhilePlayableAsync(float duration, CancellationToken token)
        {
            var remainingTime = Mathf.Max(0f, duration);

            while (remainingTime > 0f)
            {
                var canceled = await UniTask.Yield(PlayerLoopTiming.Update, token)
                    .SuppressCancellationThrow();

                if (canceled)
                    return true;

                if (_isPaused)
                    continue;

                remainingTime -= Time.deltaTime;
            }

            return false;
        }
    }
}
