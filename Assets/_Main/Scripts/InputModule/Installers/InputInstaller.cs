using Zenject;
using InputModule.Core;

namespace InputModule.Installers
{
    //TODO: Переделать в SO контекст
    public class InputInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<GameInputService>()
                .AsSingle();
        }
    }
}