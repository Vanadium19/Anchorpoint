using UnityEngine;
using Zenject;
using PlayerModule.Configs;
using PlayerModule.Controllers;
using PlayerModule.View;
using EntityModule;
using PlayerModule.Presenters;

namespace PlayerModule.Installers
{
    public class PlayerInstaller : MonoInstaller
    {
        [SerializeField] private PlayerConfig playerConfig;
        [SerializeField] private PlayerView playerView;

        [SerializeField] private PlayerHUDView hudView;
        [SerializeField] private GameOverView gameOverView;
        public override void InstallBindings()
        {
            Container.Bind<PlayerConfig>().FromInstance(playerConfig).AsSingle();
            Container.Bind<PlayerView>().FromInstance(playerView).AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerController>().AsSingle();

            Container.Bind<HealthComponent>().FromComponentOnRoot().AsSingle();
            Container.Bind<PlayerHUDView>().FromInstance(hudView).AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerHealthPresenter>().AsSingle();

            Container.Bind<GameOverView>().FromInstance(gameOverView).AsSingle();
            Container.BindInterfacesAndSelfTo<GameOverPresenter>().AsSingle();
        }
    }
}