using Zenject;
using InputModule.Core;

namespace InputModule.Installers
{
    public class InputInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IGameInput>().To<GameInputService>().AsSingle();
        }
    }
}