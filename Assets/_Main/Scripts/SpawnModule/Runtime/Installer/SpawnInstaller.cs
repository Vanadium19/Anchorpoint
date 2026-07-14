using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace SpawnModule
{
    public class SpawnInstaller : MonoInstaller
    {
        [Header("References")]
        [SerializeField] private LevelSpawnPointsView spawnPointsView;
        [SerializeField] private GameObject enemyPrefab;

        [Header("Initial Spawn")]
        [Min(0)]
        [SerializeField] private int initialEnemyCount;

        [Header("Subsequent Waves")]
        [FormerlySerializedAs("enemyCount")]
        [Min(0)]
        [SerializeField] private int enemiesPerWave;

        [FormerlySerializedAs("waitTime")]
        [Min(0)]
        [SerializeField] private int firstWaveDelay;

        [FormerlySerializedAs("spawnInterval")]
        [Min(0)]
        [SerializeField] private int waveInterval;

        public override void InstallBindings()
        {
            Container.BindInstance(spawnPointsView).AsSingle();

            var spawnConfig = new SpawnConfig(
                initialEnemyCount,
                enemiesPerWave,
                firstWaveDelay,
                waveInterval);

            Container.BindInstance(spawnConfig).AsSingle();

            Container.Bind<IEnemyFactory>()
                .To<EnemyFactory>()
                .AsSingle()
                .WithArguments(enemyPrefab);

            Container.BindInterfacesTo<EnemySpawnController>()
                .AsSingle()
                .NonLazy();
        }
    }
}
