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
    /// The group starts from a random spawn point and walks the list from there, so a group of several
    /// enemies arrives from several sides and two groups rarely start in the same place. Each action
    /// waits for its own enemies, so groups marked to run in parallel are independent.
    /// </remarks>
    public class SpawnEnemiesAction : RandomEventActionBase
    {
        private readonly IEnemyFactory _enemyFactory;
        private readonly LevelSpawnPointsView _spawnPoints;
        private readonly int _enemyCount;
        private readonly float _spawnScatterRadius;

        /// <summary>Creates the action with the size and spread of its group.</summary>
        public SpawnEnemiesAction(
            IEnemyFactory enemyFactory,
            LevelSpawnPointsView spawnPoints,
            int enemyCount,
            float spawnScatterRadius)
        {
            _enemyFactory = enemyFactory;
            _spawnPoints = spawnPoints;
            _enemyCount = enemyCount;
            _spawnScatterRadius = spawnScatterRadius;
        }

        /// <inheritdoc/>
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
            var points = _spawnPoints.SpawnPoints;

            if (points.Count == 0)
                return enemies;

            var firstPointIndex = Random.Range(0, points.Count);

            for (var index = 0; index < _enemyCount; index++)
            {
                var point = points[(firstPointIndex + index) % points.Count];
                var enemyObject = _enemyFactory.Create(point.Position + GetScatterOffset());

                if (enemyObject != null)
                    enemies.Add(new SpawnedEnemy(enemyObject));
            }

            return enemies;
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
