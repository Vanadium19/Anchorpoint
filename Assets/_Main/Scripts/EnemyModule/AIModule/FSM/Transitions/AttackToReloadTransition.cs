using FSMModule;

namespace EnemyModule
{
    public sealed class AttackToReloadTransition : AbstractStateTransition<StateName>
    {
        private readonly AttackState _attackState;

        public AttackToReloadTransition(AttackState attackState) : base(StateName.Attack, StateName.Reload)
        {
            _attackState = attackState;
        }

        public override bool CanPerform() => _attackState.NeedsReload;
    }
}