using FSMModule;

namespace EnemyModule
{
    public sealed class ThrowToChaseTransition : AbstractStateTransition<StateName>
    {
        private readonly ThrowState _throwState;

        public ThrowToChaseTransition(ThrowState throwState) : base(StateName.Throw, StateName.Chase)
        {
            _throwState = throwState;
        }

        public override bool CanPerform() => _throwState.ShouldExitThrow();
    }
}
