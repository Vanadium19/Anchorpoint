using UnityEngine;
using Zenject;
using ComponentsModule;
using UnityEngine.AI;

namespace EnemyModule
{
    public class EnemyInstaller : MonoInstaller
    {
        [SerializeField] private EnemyConfig config;
        [SerializeField] private EnemyView view;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private Transform eyes;

        private void OnValidate()
        {
            view ??= GetComponent<EnemyView>();
            navMeshAgent ??= GetComponent<NavMeshAgent>();
        }

        public override void InstallBindings()
        {
            Container.BindInstance(config).AsSingle();
            Container.BindInstance(view).AsSingle();

            Container.Bind<NavMeshAgent>()
                .FromInstance(navMeshAgent)
                .AsSingle();

            Container.Bind(typeof(IHealthComponent), typeof(IDamageable))
                .To<HealthComponent>()
                .AsSingle()
                .WithArguments(config.MaxHealth);

            Container.Bind<IPathMoveComponent>()
                .To<NavMeshMoveComponent>()
                .AsSingle();

            Container.Bind<ITargetRotationComponent>()
                .To<TargetRotationComponent>()
                .AsSingle()
                .WithArguments(view.transform);

            Container.Bind<ILineOfSightComponent>()
                .To<LineOfSightComponent>()
                .AsSingle()
                .WithArguments(eyes);

            Container.Bind<ICoverFinderComponent>()
                .To<NavMeshCoverFinderComponent>()
                .AsSingle()
                .WithArguments(view.transform);

            Container.Bind<Enemy>().AsSingle();
            Container.Bind<AIAgent>().AsSingle();

            Container.BindInterfacesTo<EnemyController>()
                .AsSingle()
                .NonLazy();
        }
    }
}