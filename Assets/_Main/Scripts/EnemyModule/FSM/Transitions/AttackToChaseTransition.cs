using FSMModule;

namespace EnemyModule
{
    public sealed class AttackToChaseTransition : AbstractStateTransition<AIState>
    {
        private readonly AttackState _attackState;

        public AttackToChaseTransition(AttackState attackState)
            : base(AIState.Attack, AIState.Chase)
        {
            _attackState = attackState;
        }

        public override bool CanPerform()
        {
            return _attackState.ShouldExitAttack();
        }
    }
}
