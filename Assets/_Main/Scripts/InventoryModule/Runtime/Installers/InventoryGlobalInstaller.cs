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

            Container.Bind<IItemDatabase>()
                .FromInstance(itemDatabase)
                .AsSingle();

            Container.Bind<InventoryModel>()
                .AsSingle()
                .WithArguments(inventoryConfig.Width, inventoryConfig.Height);

            Container.Bind<IGridService>()
                .To<GridService>()
                .AsSingle()
                .WithArguments(inventoryConfig.Width, inventoryConfig.Height);

            Container.Bind<IInventoryService>()
                .To<InventoryService>()
                .AsSingle();

            Container.Bind<IItemDropService>()
                .To<ItemDropService>()
                .AsSingle();

            Container.Bind<ILootItemFactory>()
                .To<LootItemFactory>()
                .AsSingle();
        }
    }
}