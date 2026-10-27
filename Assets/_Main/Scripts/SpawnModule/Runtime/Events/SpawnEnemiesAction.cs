using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using RandomEventsModule;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SpawnModule
{
    /// <summary>Spawns a group of enemies across the scene spawn points and holds the sequence until the last one is dead.</summary>
    /// <remarks>
    /// Spawn positions are captured when the event plan is created, so delayed event steps do not
    /// keep Unity object references that may be destroyed before the wave actually starts.
    /// </remarks>
    public class SpawnEnemiesAction : RandomEventActionBase
    {
        private readonly IEnemyFactory _enemyFactory;
        private readonly List<Vector3> _spawnPositions;
        private readonly int _enemyCount;
        private readonly float _spawnScatterRadius;

        public SpawnEnemiesAction(
            IEnemyFactory enemyFactory,
            LevelSpawnPointsView spawnPoints,
            int enemyCount,
            float spawnScatterRadius)
        {
            _enemyFactory = enemyFactory;
            _spawnPositions = CaptureSpawnPositions(spawnPoints);
            _enemyCount = enemyCount;
            _spawnScatterRadius = spawnScatterRadius;
        }

        public override async UniTask<bool> ExecuteAsync(CancellationToken token)
        {
            var enemies = Spawn();

            while (HasAliveEnemies(enemies))
            {
                var canceled = await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow();

                if (canceled)
                    break;
            }

            return true;
        }

        private List<SpawnedEnemy> Spawn()
        {
            var enemies = new List<SpawnedEnemy>();

            if (_spawnPositions.Count == 0)
                return enemies;

            var firstPointIndex = Random.Range(0, _spawnPositions.Count);

            for (var index = 0; index < _enemyCount; index++)
            {
                var position = _spawnPositions[(firstPointIndex + index) % _spawnPositions.Count];
                var enemyObject = _enemyFactory.Create(position + GetScatterOffset());

                if (enemyObject != null)
                    enemies.Add(new SpawnedEnemy(enemyObject));
            }

            return enemies;
        }

        private static List<Vector3> CaptureSpawnPositions(LevelSpawnPointsView spawnPoints)
        {
            var positions = new List<Vector3>();

            if (spawnPoints == null || spawnPoints.SpawnPoints == null)
                return positions;

            foreach (var spawnPoint in spawnPoints.SpawnPoints)
            {
                if (spawnPoint != null)
                    positions.Add(spawnPoint.Position);
            }

            return positions;
        }

        private static bool HasAliveEnemies(List<SpawnedEnemy> enemies)
        {
            enemies.RemoveAll(enemy => !enemy.IsAlive);

            return enemies.Count > 0;
        }

        private Vector3 GetScatterOffset()
        {
            var offset = Random.insideUnitCircle * _spawnScatterRadius;

            return new Vector3(offset.x, 0f, offset.y);
        }
    }
}
