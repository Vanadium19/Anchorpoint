using UnityEngine;
using Zenject;

namespace SpawnModule
{
    public class EventSpawnInstaller : MonoInstaller
    {
        [SerializeField] private LevelSpawnPointsView spawnPointsView;
        [SerializeField] private GameObject enemyPrefab;

        public override void InstallBindings()
        {
            Container.BindInstance(spawnPointsView).AsSingle();

            Container.BindInterfacesAndSelfTo<EnemyTargetRegistry>()
                .AsSingle();

            Container.Bind<IEnemyFactory>()
                .To<EnemyFactory>()
                .AsSingle()
                .WithArguments(enemyPrefab);
        }
    }
}
