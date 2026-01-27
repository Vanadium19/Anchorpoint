using UnityEngine;
using Zenject;

namespace ExtractionModule
{
    public class ExtractionInstaller : MonoInstaller
    {
        [SerializeField] private ExtractionConfig config;
        [SerializeField] private ExtractionZoneView zoneView;
        [SerializeField] private ExtractionHUDView hudView;

        public override void InstallBindings()
        {
            Container.BindInstance(config).AsSingle();
            Container.BindInstance(zoneView).AsSingle();
            Container.BindInstance(hudView).AsSingle();

            Container.BindInterfacesAndSelfTo<ExtractionController>().AsSingle();
        }
    }
}