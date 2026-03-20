using UnityEngine;
using Zenject;
using ComponentsModule;
using UnityEngine.AI;
using System;

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

            Container.Bind<Transform[]>()
                .FromInstance(patrolPoints)
                .WhenInjectedInto<PatrolState>();

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

            Container.Bind<PatrolState>().AsSingle();
            Container.Bind<ChaseState>().AsSingle();
            Container.Bind<AttackState>().AsSingle();
            Container.Bind<ReloadState>().AsSingle();

            Container.Bind<PatrolToChaseTransition>().AsSingle();
            Container.Bind<ChaseToAttackTransition>().AsSingle();
            Container.Bind<ChaseToPatrolTransition>().AsSingle();
            Container.Bind<AttackToReloadTransition>().AsSingle();
            Container.Bind<AttackToChaseTransition>().AsSingle();
            Container.Bind<ReloadToChaseTransition>().AsSingle();

            Container.Bind<EnemyAIStateMachine>().AsSingle();

            Container.BindInterfacesTo<EnemyController>()
                .AsSingle()
                .NonLazy();
        }
    }
}
