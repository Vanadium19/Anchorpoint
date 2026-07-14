using PlayerModule;
using UnityEngine;
using Zenject;
using InventoryModule;

namespace CampSaveModule
{
    public class CampSceneInstaller : MonoInstaller
    {
        [SerializeField] private PlayerProvider playerProvider;

        public override void InstallBindings()
        {
            Container.Bind<PlayerProvider>()
                .FromInstance(playerProvider)
                .AsSingle();

            Container.Bind<InventorySaveable>()
                .AsSingle();

            Container.Bind<CampSaveable>()
                .AsSingle();

            Container.Bind<WorkbenchSaveable>()
                .AsSingle();

            Container.BindInterfacesTo<CampSaveHandler>()
                .AsSingle();
        }
    }
}