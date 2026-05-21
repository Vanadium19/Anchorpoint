using UnityEngine;
using Zenject;

namespace MenuModule
{
    public class MainMenuInstaller : MonoInstaller
    {
        [SerializeField] private MainMenuView mainMenuView;

        public override void InstallBindings()
        {
            Container.Bind<MainMenuView>()
                .FromInstance(mainMenuView)
                .AsSingle();

            Container.BindInterfacesTo<MainMenuPresenter>()
                .AsSingle()
                .NonLazy();
        }
    }
}