using Zenject;
using InventoryModule;

namespace GameSceneSaveModule
{
    public class GameSceneSaveModuleInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<GameSceneSaveHandler>()
                .AsSingle();
        }
    }
}
