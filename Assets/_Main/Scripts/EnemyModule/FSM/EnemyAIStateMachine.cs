using ComponentsModule;
using FSMModule;
using UnityEngine;

namespace EnemyModule
{
    public sealed class EnemyAIStateMachine
    {
        private readonly IHealthComponent _health;
        private readonly IAutoStateMachine<AIState> _stateMachine;

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
            _stateMachine = new AutoStateMachine<AIState>(
                AIState.Patrol,
                new[]
                {
                    (AIState.Patrol, (IState)patrolState),
                    (AIState.Chase, (IState)chaseState),
                    (AIState.Attack, (IState)attackState),
                    (AIState.Reload, (IState)reloadState),
                },
                new IStateTransition<AIState>[]
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
