using UnityEngine;
using Zenject;

namespace MenuModule
{
    public class GameplayMenuInstaller : MonoInstaller
    {
        [SerializeField] private GameNavigationConfig navigationConfig;
        [SerializeField] private PauseMenuView pauseMenuView;
        [SerializeField] private VictoryView victoryView;

        public override void InstallBindings()
        {
            InstallSharedServices();

            if (pauseMenuView != null)
            {
                Container.Bind<PauseMenuView>()
                    .FromInstance(pauseMenuView)
                    .AsSingle();

                Container.BindInterfacesTo<PauseMenuPresenter>()
                    .AsSingle()
                    .NonLazy();

                Container.BindInterfacesTo<GameplayPauseController>()
                    .AsSingle()
                    .NonLazy();
            }

            if (victoryView != null)
            {
                Container.Bind<VictoryView>()
                    .FromInstance(victoryView)
                    .AsSingle();

                Container.BindInterfacesTo<VictoryPresenter>()
                    .AsSingle()
                    .NonLazy();
            }
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

            Container.BindInterfacesAndSelfTo<GamePauseService>()
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
