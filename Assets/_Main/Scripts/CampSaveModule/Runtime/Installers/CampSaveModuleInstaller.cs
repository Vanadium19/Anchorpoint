using Zenject;
using UnityEngine;
using SaveModule;

namespace CampSaveModule
{
    public class CampSaveModuleInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<CampSaveable>()
                .AsSingle();

            Container.BindInterfacesTo<CampSaveHandler>()
                .AsSingle();
        }
    }
}
