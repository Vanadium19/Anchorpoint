using UnityEngine;
using Zenject;
using ExtractionModule.Configs;
using ExtractionModule.Controllers;
using ExtractionModule.View;

namespace ExtractionModule.Installers
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