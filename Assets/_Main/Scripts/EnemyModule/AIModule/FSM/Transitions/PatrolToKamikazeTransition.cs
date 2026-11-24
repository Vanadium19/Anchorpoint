using FSMModule;

namespace EnemyModule
{
    public sealed class PatrolToKamikazeTransition : AbstractStateTransition<StateName>
    {
        private readonly PatrolState _patrolState;

        public PatrolToKamikazeTransition(PatrolState patrolState) : base(StateName.Patrol, StateName.Kamikaze)
        {
            _patrolState = patrolState;
        }

        public override bool CanPerform() => _patrolState.TrySpotPlayer();
    }
}
