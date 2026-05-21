using InventoryModule;
using UnityEngine;
using Zenject;

namespace SpawnModule
{
    public class LootSpawnInstaller : MonoInstaller
    {
        [Header("References")]
        [SerializeField] private LevelLootSpawnPointsView spawnPointsView;

        public override void InstallBindings()
        {
            Container.BindInstance(spawnPointsView).AsSingle();

            Container.Bind<ILootFactory>()
                .To<LootFactory>()
                .AsSingle();

            Container.BindInterfacesTo<LootSpawnController>()
                .AsSingle()
                .NonLazy();
        }
    }
}
