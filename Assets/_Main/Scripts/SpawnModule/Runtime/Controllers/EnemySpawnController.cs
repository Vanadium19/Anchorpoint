using System;
using System.Linq;
using System.Threading;
using BaseModule;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace SpawnModule
{
    public sealed class EnemySpawnController : IInitializable, IDisposable, IPausable
    {
        private readonly LevelSpawnPointsView _view;
        private readonly IEnemyFactory _factory;
        private readonly SpawnConfig _config;
        private readonly IPauseManager _pauseManager;

        private CancellationTokenSource _cancellationTokenSource;
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
            Spawn(_config.InitialEnemyCount);
            StartSpawning();
        }

        public void StartSpawning()
        {
            if (_isSpawning)
                return;

            _cancellationTokenSource = new CancellationTokenSource();
            _isSpawning = true;

            SpawnLoopAsync(_cancellationTokenSource.Token).Forget();
        }

        public void Spawn() => Spawn(_config.EnemiesPerWave);

        public void StopSpawning()
        {
            _isSpawning = false;

            if (_cancellationTokenSource == null)
                return;

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        public void SetPaused(bool isPaused) => _isPaused = isPaused;

        public void Dispose()
        {
            _pauseManager.Unregister(this);
            StopSpawning();
        }

        private void Spawn(int enemyCount)
        {
            if (enemyCount <= 0)
                return;

            var points = _view.SpawnPoints;

            if (enemyCount > points.Count)
                throw new InvalidOperationException(
                    $"Enemy count ({enemyCount}) is greater than spawn point count ({points.Count}).");

            var shuffledPoints = points
                .OrderBy(_ => UnityEngine.Random.value)
                .ToList();

            for (var i = 0; i < enemyCount; i++)
                _factory.Create(shuffledPoints[i].Position);
        }

        private async UniTaskVoid SpawnLoopAsync(CancellationToken token)
        {
            var canceled = await WaitWhilePlayableAsync(_config.FirstWaveDelay, token);

            if (canceled)
                return;

            while (!token.IsCancellationRequested && _isSpawning)
            {
                if (!_isPaused)
                    Spawn();

                canceled = await WaitWhilePlayableAsync(_config.WaveInterval, token);

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
