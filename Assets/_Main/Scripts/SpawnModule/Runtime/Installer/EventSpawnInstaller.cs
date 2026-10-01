using UnityEngine;
using Zenject;

namespace SpawnModule
{
    /// <summary>Binds what <see cref="SpawnEnemiesAction"/> needs on a scene that spawns enemies from events rather than from waves.</summary>
    /// <remarks>Only the factory and the spawn points, without the controllers of <see cref="SpawnInstaller"/>; the two installers bind the same types, so a scene carries one of them, never both.</remarks>
    public class EventSpawnInstaller : MonoInstaller
    {
        [SerializeField] private LevelSpawnPointsView spawnPointsView;
        [SerializeField] private GameObject enemyPrefab;

        /// <summary>Binds the enemy factory and the scene spawn points.</summary>
        public override void InstallBindings()
        {
            Container.BindInstance(spawnPointsView).AsSingle();

            Container.Bind<IEnemyFactory>()
                .To<EnemyFactory>()
                .AsSingle()
                .WithArguments(enemyPrefab);
        }
    }
}
