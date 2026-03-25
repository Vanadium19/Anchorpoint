using UnityEngine;
using Zenject;

namespace SpawnModule
{
    public class SpawnInstaller : MonoInstaller
    {
        [SerializeField] private LevelSpawnPointsView spawnPointsView;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private int enemyCount;

        public override void InstallBindings()
        {
            Container.BindInstance(spawnPointsView).AsSingle();

            var spawnConfig = new SpawnConfig
            {
                EnemyCount = enemyCount
            };
            Container.BindInstance(spawnConfig).AsSingle();

            Container.Bind<IEnemyFactory>()
                .To<EnemyFactory>()
                .AsSingle()
                .WithArguments(enemyPrefab);

            // Добавим интерфейсы потом
            Container.BindInterfacesTo<EnemySpawnController>()
                .AsSingle()
                .NonLazy();
        }
    }
}