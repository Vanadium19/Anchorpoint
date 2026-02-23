using UnityEngine;
using Zenject;

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
        }
    }
}
