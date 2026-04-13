using Zenject;
using InventoryModule.ContextMenu;
using InventoryModule.ContextMenu.UI;

namespace InventoryModule
{
    public sealed class InventoryGlobalInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InventoryManager>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<EquipmentSlotService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<GridService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IContainerWindowService>()
                .To<ContainerWindowService>()
                .AsSingle();

            Container.Bind<IContextMenuStateService>()
                .To<ContextMenuStateService>()
                .AsSingle();

            Container.BindInterfacesTo<InventoryStaticDataResetHandler>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<UIInputHandler>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IContextActionService>()
                .To<ContextActionService>()
                .AsSingle();

            Container.Bind<IDeathLootStorage>()
                .To<DeathLootStorage>()
                .AsSingle();
        }
    }
}