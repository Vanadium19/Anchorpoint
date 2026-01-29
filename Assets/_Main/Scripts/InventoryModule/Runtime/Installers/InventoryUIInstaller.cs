using InputModule;
using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public class InventoryUIInstaller : MonoInstaller
    {
        [SerializeField] private InventoryView view;

        public override void InstallBindings()
        {
            Container.Bind<InventoryView>()
                .FromInstance(view)
                .AsSingle();

            Container.BindInterfacesTo<InventoryPresenter>()
                .AsSingle()
                .NonLazy();
        }
    }
}