using Zenject;
using InventoryModule.ContextMenu;

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

            Container.BindInterfacesTo<InventoryStaticDataResetHandler>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<UIInputHandler>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<ContextActionService>()
                .AsSingle()
                .NonLazy();
        }
    }
}
