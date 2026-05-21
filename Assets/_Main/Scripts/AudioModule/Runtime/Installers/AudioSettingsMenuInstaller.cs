using UnityEngine;
using Zenject;

namespace AudioModule
{
    public class AudioSettingsMenuInstaller : MonoInstaller
    {
        [SerializeField] private SettingsMenuView settingsMenuView;

        public override void InstallBindings()
        {
            Container.Bind<SettingsMenuView>()
                .FromInstance(settingsMenuView)
                .AsSingle();

            Container.BindInterfacesTo<SettingsMenuPresenter>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<SettingsMenuEscapeCloseController>()
                .AsSingle()
                .NonLazy();
        }
    }
}