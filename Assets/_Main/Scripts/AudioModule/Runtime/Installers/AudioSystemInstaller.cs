using Zenject;

namespace AudioModule
{
    public class AudioSystemInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var audioSystem = AudioSystem.Resolve(this);

            Container.Bind<IAudioSystem>().FromInstance(audioSystem);

            Container.BindInterfacesTo<AudioPauseBridge>()
                .AsSingle()
                .NonLazy();
        }
    }
}
