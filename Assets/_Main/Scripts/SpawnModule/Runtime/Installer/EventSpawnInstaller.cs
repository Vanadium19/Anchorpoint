using UnityEngine;
using Zenject;

namespace SpawnModule
{
    public class EventSpawnInstaller : MonoInstaller
    {
        [SerializeField] private LevelSpawnPointsView spawnPointsView;
        [SerializeField] private GameObject enemyPrefab;
        [Tooltip("Enemy types picked by weight for every spawn. When empty, only Enemy Prefab spawns.")]
        [SerializeField] private UnitEntry[] enemyVariants;

        public override void InstallBindings()
        {
            Container.BindInstance(spawnPointsView).AsSingle();
            Container.BindInstance(new EnemyVariants(enemyVariants)).AsSingle();

            Container.BindInterfacesAndSelfTo<EnemyTargetRegistry>()
                .AsSingle();

            Container.Bind<IEnemyFactory>()
                .To<EnemyFactory>()
                .AsSingle()
                .WithArguments(enemyPrefab);
        }
    }
}
