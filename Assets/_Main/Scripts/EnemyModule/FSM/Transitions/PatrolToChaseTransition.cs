using FSMModule;

namespace EnemyModule
{
    public sealed class PatrolToChaseTransition : AbstractStateTransition<AIState>
    {
        private readonly PatrolState _patrolState;

        public PatrolToChaseTransition(PatrolState patrolState)
            : base(AIState.Patrol, AIState.Chase)
        {
            _patrolState = patrolState;
        }

        public override bool CanPerform()
        {
            return _patrolState.TrySpotPlayer();
        }
    }
}
