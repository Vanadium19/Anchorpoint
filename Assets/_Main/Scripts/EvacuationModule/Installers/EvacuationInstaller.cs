using UnityEngine;
using Zenject;

namespace EvacuationModule
{
    public class EvacuationInstaller : MonoInstaller
    {
        [SerializeField] private EvacuationConfig config;
        [SerializeField] private EvacuationZoneView zoneView;
        [SerializeField] private EvacuationHUDView hudView;

        public override void InstallBindings()
        {
            Container.BindInstance(config).AsSingle();
            Container.BindInstance(zoneView).AsSingle();
            Container.BindInstance(hudView).AsSingle();

            Container.BindInterfacesAndSelfTo<EvacuationController>().AsSingle();
        }
    }
}