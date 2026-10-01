using FSMModule;

namespace EnemyModule
{
    public sealed class AttackStructureToPatrolTransition : AbstractStateTransition<StateName>
    {
        private readonly AttackStructureState _attackStructureState;

        public AttackStructureToPatrolTransition(AttackStructureState attackStructureState) : base(StateName.AttackStructure, StateName.Patrol)
        {
            _attackStructureState = attackStructureState;
        }

        public override bool CanPerform() => !_attackStructureState.CanAttackStructure();
    }
}
