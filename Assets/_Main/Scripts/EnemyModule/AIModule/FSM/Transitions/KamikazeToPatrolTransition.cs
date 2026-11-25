using FSMModule;

namespace EnemyModule
{
    public sealed class KamikazeToPatrolTransition : AbstractStateTransition<StateName>
    {
        private readonly KamikazeState _kamikazeState;

        public KamikazeToPatrolTransition(KamikazeState kamikazeState) : base(StateName.Kamikaze, StateName.Patrol)
        {
            _kamikazeState = kamikazeState;
        }

        public override bool CanPerform() => _kamikazeState.HasLostTarget();
    }
}
