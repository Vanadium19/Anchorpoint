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

            Container.BindInterfacesTo<InventoryPresenter>()
                .AsSingle()
                .NonLazy();
        }
    }
}