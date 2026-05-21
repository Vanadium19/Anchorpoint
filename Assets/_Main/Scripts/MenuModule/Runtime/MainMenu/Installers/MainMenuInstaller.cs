using UnityEngine;
using Zenject;

namespace MenuModule
{
    public class MainMenuInstaller : MonoInstaller
    {
        [SerializeField] private GameNavigationConfig navigationConfig;
        [SerializeField] private MainMenuView mainMenuView;

        public override void InstallBindings()
        {
            InstallSharedServices();

            if (mainMenuView == null)
                return;

            Container.Bind<MainMenuView>()
                .FromInstance(mainMenuView)
                .AsSingle();

            Container.BindInterfacesTo<MainMenuPresenter>()
                .AsSingle()
                .NonLazy();
        }

        private void InstallSharedServices()
        {
            Container.Bind<GameNavigationConfig>()
                .FromInstance(GetNavigationConfig())
                .AsSingle()
                .IfNotBound();

            Container.Bind<IGameNavigationService>()
                .To<GameNavigationService>()
                .AsSingle()
                .IfNotBound();
        }

        private GameNavigationConfig GetNavigationConfig()
        {
            return navigationConfig != null
                ? navigationConfig
                : ScriptableObject.CreateInstance<GameNavigationConfig>();
        }
    }
}
