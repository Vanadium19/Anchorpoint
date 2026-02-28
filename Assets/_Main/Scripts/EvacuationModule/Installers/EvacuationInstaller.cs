using UnityEngine;
using Zenject;

namespace EvacuationModule
{
    public class EvacuationInstaller : MonoInstaller
    {
        [SerializeField] private EvacuationConfig config;
        [SerializeField] private EvacuationController controller;
        [SerializeField] private EvacuationView view;

        public override void InstallBindings()
        {
            Container.BindInstance(config).AsSingle();
            Container.BindInstance(controller).AsSingle();
            Container.BindInstance(view).AsSingle();

            Container.Bind<EndEvacuationCommand>().AsSingle().NonLazy();

            Container.BindInterfacesTo<EvacuationService>().AsSingle();
            Container.BindInterfacesTo<EvacuationPresenter>().AsSingle().NonLazy();
        }
    }
}