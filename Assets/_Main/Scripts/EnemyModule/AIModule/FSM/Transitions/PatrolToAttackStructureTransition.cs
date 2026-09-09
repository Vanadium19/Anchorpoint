using FSMModule;

namespace EnemyModule
{
    public sealed class PatrolToAttackStructureTransition : AbstractStateTransition<StateName>
    {
        private readonly AttackStructureState _attackStructureState;

        public PatrolToAttackStructureTransition(AttackStructureState attackStructureState) : base(StateName.Patrol, StateName.AttackStructure)
        {
            _attackStructureState = attackStructureState;
        }

        public override bool CanPerform() => _attackStructureState.CanAttackStructure();
    }
}
