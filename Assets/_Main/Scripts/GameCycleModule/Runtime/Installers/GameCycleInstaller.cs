using PlayerModule;
using UnityEngine;
using Zenject;

namespace GameCycleModule
{
    public class GameCycleInstaller : MonoInstaller
    {
        [SerializeField] private GameOverView gameOverView;
        [SerializeField] private PlayerProvider playerProvider;

        public override void InstallBindings()
        {
            Container.Bind<PlayerProvider>()
                .FromInstance(playerProvider)
                .AsSingle();

            Container.BindInterfacesTo<GameSessionService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<GameOverPresenter>()
                .AsSingle()
                .NonLazy();

            Container.Bind<GameOverView>()
                .FromInstance(gameOverView)
                .AsSingle();
        }
    }
}