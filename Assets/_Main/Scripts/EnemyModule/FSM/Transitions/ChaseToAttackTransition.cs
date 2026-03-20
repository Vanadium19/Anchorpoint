using FSMModule;

namespace EnemyModule
{
    public sealed class ChaseToAttackTransition : AbstractStateTransition<StateName>
    {
        private readonly ChaseState _chaseState;

        public ChaseToAttackTransition(ChaseState chaseState) : base(StateName.Chase, StateName.Attack)
        {
            _chaseState = chaseState;
        }

        public override bool CanPerform() => _chaseState.CanEnterAttack();
    }
}