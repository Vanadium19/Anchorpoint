using UnityEngine;
using Zenject;
using PlayerModule.Configs;
using PlayerModule.Controllers;
using PlayerModule.View;

namespace PlayerModule.Installers
{
    public class PlayerInstaller : MonoInstaller
    {
        [SerializeField] private PlayerConfig playerConfig;
        [SerializeField] private PlayerView playerView;

        public override void InstallBindings()
        {
            // 1. Биндим конфиг
            Container.Bind<PlayerConfig>().FromInstance(playerConfig).AsSingle();

            // 2. Биндим View
            Container.Bind<PlayerView>().FromInstance(playerView).AsSingle();

            // 3. Биндим Контроллер
            // InterfacesToAndSelfTo означает, что он биндится и как PlayerController, 
            // и как IInitializable (Start), и как ITickable (Update)
            Container.BindInterfacesAndSelfTo<PlayerController>().AsSingle();
        }
    }
}