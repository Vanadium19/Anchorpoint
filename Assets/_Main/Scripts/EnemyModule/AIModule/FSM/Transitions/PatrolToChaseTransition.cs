using FSMModule;

namespace EnemyModule
{
    public sealed class PatrolToChaseTransition : AbstractStateTransition<StateName>
    {
        private readonly PatrolState _patrolState;

        public PatrolToChaseTransition(PatrolState patrolState) : base(StateName.Patrol, StateName.Chase)
        {
            _patrolState = patrolState;
        }

        public override bool CanPerform() => _patrolState.TrySpotPlayer();
    }
}