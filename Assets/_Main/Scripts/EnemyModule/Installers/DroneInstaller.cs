using ComponentsModule;
using UnityEngine;
using Zenject;

namespace EnemyModule
{
    /// <summary>
    /// Assembles the kamikaze drone inside its <see cref="GameObjectContext"/>.
    /// </summary>
    public class DroneInstaller : MonoInstaller
    {
        [SerializeField] private EnemyConfig config;
        [SerializeField] private DroneConfig droneConfig;
        [SerializeField] private DroneView view;
        [SerializeField] private DroneFlightMover flight;
        [SerializeField] private Transform eyes;

        public override void InstallBindings()
        {
            Container.BindInstance(config).AsSingle();
            Container.BindInstance(droneConfig).AsSingle();
            Container.BindInstance(view).AsSingle();

            Container.Bind<Transform>().FromInstance(transform);
            Container.Bind<Transform[]>().FromInstance(new Transform[0]);

            Container.Bind(typeof(IHealthComponent), typeof(IDamageable))
                .To<HealthComponent>()
                .AsSingle()
                .WithArguments(config.MaxHealth);

            Container.Bind(typeof(IPathMoveComponent), typeof(IDroneFlightComponent))
                .FromInstance(flight)
                .AsSingle();

            Container.Bind<ITargetRotationComponent>()
                .To<TargetRotationComponent>()
                .AsSingle()
                .WithArguments(transform);

            Container.Bind<ILineOfSightComponent>()
                .To<LineOfSightComponent>()
                .AsSingle()
                .WithArguments(eyes);

            Container.Bind<IExplosionDamageService>()
                .To<ExplosionDamageService>()
                .AsSingle();

            Container.BindInterfacesTo<DroneExplosive>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<LootDumper>()
                .AsSingle()
                .NonLazy();

            Container.Bind<Blackboard>().AsSingle();
        }
    }
}
