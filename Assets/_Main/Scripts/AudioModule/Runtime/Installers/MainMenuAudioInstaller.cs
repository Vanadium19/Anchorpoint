using Zenject;

namespace AudioModule
{
    public class MainMenuAudioInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var audioSystem = AudioSystem.Resolve(this);

            Container.Bind<IAudioSystem>().FromInstance(audioSystem);

            Container.BindInterfacesTo<MainMenuAudioPresenter>().AsSingle().NonLazy();
        }
    }
}