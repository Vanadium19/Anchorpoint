using ComponentsModule;
using FSMModule;
using UnityEngine;

namespace EnemyModule
{
    public sealed class EnemyAIStateMachine
    {
        private readonly EnemyConfig _config;
        private readonly Enemy _enemy;
        private readonly EnemyView _view;
        private readonly IHealthComponent _health;
        private readonly IAutoStateMachine<AIState> _stateMachine;

        public EnemyAIStateMachine(
            EnemyConfig config,
            Enemy enemy,
            EnemyView view,
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
            _config = config;
            _enemy = enemy;
            _view = view;
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

            _stateMachine.OnStateChanged += _enemy.SetState;
        }

        public void Initialize()
        {
            _enemy.Initialize(_config.MaxAmmo, _view.transform.position);
            _stateMachine.OnEnter();
        }

        public void Tick()
        {
            if (!_health.IsAlive)
                return;

            _stateMachine.OnUpdate(Time.deltaTime);
        }

        public void Dispose()
        {
            _stateMachine.OnExit();
            _stateMachine.OnStateChanged -= _enemy.SetState;
        }
    }
}
