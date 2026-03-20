using FSMModule;

namespace EnemyModule
{
    public sealed class AttackToReloadTransition : AbstractStateTransition<AIState>
    {
        private readonly AttackState _attackState;

        public AttackToReloadTransition(AttackState attackState)
            : base(AIState.Attack, AIState.Reload)
        {
            _attackState = attackState;
        }

        public override bool CanPerform()
        {
            return _attackState.NeedsReload;
        }
    }
}
