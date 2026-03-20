using FSMModule;

namespace EnemyModule
{
    public sealed class ChaseToPatrolTransition : AbstractStateTransition<StateName>
    {
        private readonly ChaseState _chaseState;

        public ChaseToPatrolTransition(ChaseState chaseState) : base(StateName.Chase, StateName.Patrol)
        {
            _chaseState = chaseState;
        }

        public override bool CanPerform() => _chaseState.ShouldReturnToPatrol();
    }
}