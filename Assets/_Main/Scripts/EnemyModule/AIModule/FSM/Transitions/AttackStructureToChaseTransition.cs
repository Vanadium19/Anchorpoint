using FSMModule;

namespace EnemyModule
{
    public sealed class AttackStructureToChaseTransition : AbstractStateTransition<StateName>
    {
        private readonly AttackStructureState _attackStructureState;

        public AttackStructureToChaseTransition(AttackStructureState attackStructureState) : base(StateName.AttackStructure, StateName.Chase)
        {
            _attackStructureState = attackStructureState;
        }

        public override bool CanPerform() => _attackStructureState.TrySpotPlayer();
    }
}
