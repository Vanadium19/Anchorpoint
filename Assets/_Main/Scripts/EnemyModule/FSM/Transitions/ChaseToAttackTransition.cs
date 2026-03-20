using FSMModule;

namespace EnemyModule
{
    public sealed class ChaseToAttackTransition : AbstractStateTransition<AIState>
    {
        private readonly ChaseState _chaseState;

        public ChaseToAttackTransition(ChaseState chaseState)
            : base(AIState.Chase, AIState.Attack)
        {
            _chaseState = chaseState;
        }

        public override bool CanPerform()
        {
            return _chaseState.CanEnterAttack();
        }
    }
}
