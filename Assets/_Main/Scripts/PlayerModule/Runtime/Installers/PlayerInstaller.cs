using ComponentsModule;
using UIModule;
using UnityEngine;
using Zenject;
using InventoryModule;

namespace PlayerModule
{
    public class PlayerInstaller : MonoInstaller
    {
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private Transform player;
        
        [Header("Health")]
        [SerializeField] private HealthView healthView;
        
        [Header("Settings")]
        [SerializeField] private PlayerConfig config;

        private void OnValidate()
        {
            player ??= transform;
            characterController ??= GetComponent<CharacterController>();
        }

        public override void InstallBindings()
        {
            Container.Bind<PlayerConfig>()
                .FromInstance(config)
                .AsSingle();

            Container.Bind<CharacterController>()
                .FromInstance(characterController)
                .AsSingle();

            Container.Bind<Transform>()
                .FromInstance(player)
                .AsSingle();

            Container.Bind<IMoveComponent>()
                .To<MoveComponent>()
                .AsSingle()
                .WithArguments(config.WalkSpeed, config.JumpHeight, config.Gravity);

            Container.Bind<IRotationComponent>()
                .To<RotationComponent>()
                .AsSingle()
                .WithArguments(config.MouseSensitivity, config.LookXLimit);

            Container.Bind<ICrouchComponent>()
                .To<CrouchComponent>()
                .AsSingle()
                .WithArguments(cameraRoot, config.CrouchData);

            Container.Bind<ILeanComponent>()
                .To<LeanComponent>()
                .AsSingle()
                .WithArguments(cameraRoot, config.LeanAngle, config.LeanOffset, config.LeanSpeed);

            Container.Bind(typeof(IHealthComponent), typeof(IDamageable))
                .To<HealthComponent>()
                .AsSingle()
                .WithArguments(config.MaxHealth);

            Container.Bind<HealthView>()
                .FromInstance(healthView)
                .AsSingle();
            
            Container.BindInterfacesTo<HealthPresenter>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<PlayerMovementController>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<PlayerInteractionController>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<InventoryDeathHandler>()
                .AsSingle()
                .NonLazy();
        }
    }
}