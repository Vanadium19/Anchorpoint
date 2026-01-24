using UnityEngine;
using Zenject;

namespace CameraModule
{
    public class CameraInstaller : MonoInstaller
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 2f;
        [SerializeField] private float minHeight = 1f;
        [SerializeField] private float maxHeight = 10f;

        public override void InstallBindings()
        {
            Container.Bind<IBuildingModeCameraService>()
                .To<BuildingModeCameraService>()
                .AsSingle()
                .WithArguments(moveSpeed, rotationSpeed, minHeight, maxHeight);

            Container.BindInterfacesTo<BuildingModeCameraController>()
                .AsSingle()
                .NonLazy();
        }
    }
}