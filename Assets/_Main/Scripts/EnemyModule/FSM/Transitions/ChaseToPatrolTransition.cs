using FSMModule;

namespace EnemyModule
{
    public sealed class ChaseToPatrolTransition : AbstractStateTransition<AIState>
    {
        private readonly ChaseState _chaseState;

        public ChaseToPatrolTransition(ChaseState chaseState)
            : base(AIState.Chase, AIState.Patrol)
        {
            _chaseState = chaseState;
        }

        public override bool CanPerform()
        {
            return _chaseState.ShouldReturnToPatrol();
        }
    }
}
