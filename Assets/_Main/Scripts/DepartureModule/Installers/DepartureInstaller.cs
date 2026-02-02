using UnityEngine;
using Zenject;

namespace DepartureModule
{
    public class DepartureInstaller : MonoInstaller
    {
        [SerializeField] private DepartureConfig config;
        [SerializeField] private DepartureZoneView zoneView;
        [SerializeField] private DepartureHUDView hudView;

        public override void InstallBindings()
        {
            Container.BindInstance(config).AsSingle();
            Container.BindInstance(zoneView).AsSingle();
            Container.BindInstance(hudView).AsSingle();

            Container.BindInterfacesTo<DepartureController>().AsSingle();
        }
    }
}