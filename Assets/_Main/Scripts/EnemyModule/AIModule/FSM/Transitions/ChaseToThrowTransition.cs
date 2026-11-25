using FSMModule;

namespace EnemyModule
{
    public sealed class ChaseToThrowTransition : AbstractStateTransition<StateName>
    {
        private readonly ThrowState _throwState;

        public ChaseToThrowTransition(ThrowState throwState) : base(StateName.Chase, StateName.Throw)
        {
            _throwState = throwState;
        }

        public override bool CanPerform() => _throwState.CanEnterThrow();
    }
}
