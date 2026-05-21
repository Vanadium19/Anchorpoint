using UnityEngine;
using Zenject;

namespace MenuModule
{
    [CreateAssetMenu(fileName = "MenuNavigationInstaller", menuName = "Game/Configs/Menu/MenuNavigationInstaller")]
    public class MenuNavigationInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private MenuNavigationConfig navigationConfig;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<GamePauseService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<MenuNavigationConfig>()
                .FromInstance(navigationConfig)
                .AsSingle()
                .IfNotBound();

            Container.Bind<IMenuNavigationService>()
                .To<MenuNavigationService>()
                .AsSingle()
                .IfNotBound();
        }
    }
}