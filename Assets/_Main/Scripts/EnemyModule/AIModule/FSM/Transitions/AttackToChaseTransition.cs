using FSMModule;

namespace EnemyModule
{
    public sealed class AttackToChaseTransition : AbstractStateTransition<StateName>
    {
        private readonly AttackState _attackState;

        public AttackToChaseTransition(AttackState attackState) : base(StateName.Attack, StateName.Chase)
        {
            _attackState = attackState;
        }

        public override bool CanPerform() => _attackState.ShouldExitAttack();
    }
}