using UnityEngine;
using Zenject;

namespace BaseModule
{
    public class BaseModuleInstaller : MonoInstaller
    {
        [SerializeField] private BaseLevelConfig baseLevelConfig;

        public override void InstallBindings()
        {
            Container.Bind<BaseLevelConfig>().FromInstance(baseLevelConfig).AsSingle();
            Container.Bind<IBaseLevelService>().To<BaseLevelService>().AsSingle();
        }
    }
}
