using ComponentsModule;
using FSMModule;
using UnityEngine;

namespace EnemyModule
{
    public sealed class EnemyAIStateMachine
    {
        private readonly IHealthComponent _health;
        private readonly IAutoStateMachine<StateName> _stateMachine;

        public EnemyAIStateMachine(
            IHealthComponent health,
            PatrolState patrolState,
            ChaseState chaseState,
            AttackState attackState,
            ReloadState reloadState,
            PatrolToChaseTransition patrolToChaseTransition,
            ChaseToAttackTransition chaseToAttackTransition,
            ChaseToPatrolTransition chaseToPatrolTransition,
            AttackToReloadTransition attackToReloadTransition,
            AttackToChaseTransition attackToChaseTransition,
            ReloadToChaseTransition reloadToChaseTransition)
        {
            _health = health;
            _stateMachine = new AutoStateMachine<StateName>(
                StateName.Patrol,
                new[]
                {
                    (StateName.Patrol, (IState)patrolState),
                    (StateName.Chase, (IState)chaseState),
                    (StateName.Attack, (IState)attackState),
                    (StateName.Reload, (IState)reloadState),
                },
                new IStateTransition<StateName>[]
                {
                    patrolToChaseTransition,
                    chaseToAttackTransition,
                    chaseToPatrolTransition,
                    attackToReloadTransition,
                    attackToChaseTransition,
                    reloadToChaseTransition,
                });
        }

        public void Initialize() => _stateMachine.OnEnter();

        public void Tick()
        {
            if (!_health.IsAlive)
                return;

            _stateMachine.OnUpdate(Time.deltaTime);
        }

        public void Dispose() => _stateMachine.OnExit();
    }
}
