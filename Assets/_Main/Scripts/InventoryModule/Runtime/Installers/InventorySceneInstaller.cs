using ComponentsModule;
using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public sealed class InventorySceneInstaller : MonoInstaller
    {
        [SerializeField] private InventoryView inventoryView;

        public override void InstallBindings()
        {
            Container.Bind<InventoryView>()
                .FromInstance(inventoryView)
                .AsSingle();

            Container.Bind<IInventoryItemPool>()
                .To<InventoryItemPool>()
                .AsSingle()
                .WithArguments(inventoryView.ItemPrefab, inventoryView.ItemsContainer);

            Container.BindInterfacesTo<InventoryPresenter>()
                .AsSingle()
                .NonLazy();
            Container.Bind<IPlayerPositionProvider>()
                .FromComponentInHierarchy()
                .AsSingle();
        }
    }
}