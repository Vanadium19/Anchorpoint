using UnityEngine;
using Zenject;

namespace AudioModule
{
    [CreateAssetMenu(fileName = "AudioSettingsInstaller", menuName = "Game/Configs/Audio/AudioSettingsInstaller")]
    public class AudioSettingsInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private AudioSettingsConfig audioSettingsConfig;

        public override void InstallBindings()
        {
            Container.Bind<AudioSettingsConfig>()
                .FromInstance(audioSettingsConfig)
                .AsSingle()
                .IfNotBound();

            Container.BindInterfacesTo<AudioMixerSettingsService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<AudioSettingsSaveService>()
                .AsSingle()
                .NonLazy();
        }
    }
}