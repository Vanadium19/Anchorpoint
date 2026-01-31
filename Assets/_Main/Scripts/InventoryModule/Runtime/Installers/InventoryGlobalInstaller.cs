using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public sealed class InventoryGlobalInstaller : MonoInstaller
    {
        [SerializeField] private InventoryConfig inventoryConfig;
        [SerializeField] private ItemDatabase itemDatabase;

        public override void InstallBindings()
        {
            Container.Bind<InventoryConfig>()
                .FromInstance(inventoryConfig)
                .AsSingle();

            Container.Bind<ItemDatabase>()
                .FromInstance(itemDatabase)
                .AsSingle();

            Container.Bind<InventoryModel>()
                .AsSingle()
                .WithArguments(inventoryConfig.Width, inventoryConfig.Height);

            Container.Bind<IItemProvider>()
                .To<ItemProvider>()
                .AsSingle();

            Container.Bind<IInventoryService>()
                .To<InventoryService>()
                .AsSingle();
        }
    }
}