using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

namespace SpawnModule
{
    public sealed class EnemySpawnController : IInitializable, IDisposable
    {
        private readonly LevelSpawnPointsView _view;
        private readonly IEnemyFactory _factory;
        private readonly SpawnConfig _config;
        private CancellationTokenSource _cancellationTokenSource;
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

        public void Dispose() => StopSpawning();

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
            try
            {
                await UniTask.WaitForSeconds(
                    _config.FirstWaveDelay,
                    cancellationToken: token);

                while (!token.IsCancellationRequested && _isSpawning)
                {
                    Spawn();

                    await UniTask.WaitForSeconds(
                        _config.WaveInterval,
                        cancellationToken: token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
