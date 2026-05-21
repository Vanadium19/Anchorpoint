using UnityEngine;
using Zenject;

namespace MenuModule
{
    public class GameplayMenuInstaller : MonoInstaller
    {
        [SerializeField] private PauseMenuView pauseMenuView;
        [SerializeField] private VictoryView victoryView;

        public override void InstallBindings()
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

            if (victoryView == null)
                return;

            Container.Bind<VictoryView>()
                .FromInstance(victoryView)
                .AsSingle();

            Container.BindInterfacesTo<VictoryPresenter>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<VictorySaveDeleteController>()
                .AsSingle()
                .NonLazy();
        }
    }
}