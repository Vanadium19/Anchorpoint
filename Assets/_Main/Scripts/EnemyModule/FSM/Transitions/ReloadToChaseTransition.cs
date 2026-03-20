using FSMModule;

namespace EnemyModule
{
    public sealed class ReloadToChaseTransition : AbstractStateTransition<AIState>
    {
        private readonly ReloadState _reloadState;

        public ReloadToChaseTransition(ReloadState reloadState)
            : base(AIState.Reload, AIState.Chase)
        {
            _reloadState = reloadState;
        }

        public override bool CanPerform()
        {
            return _reloadState.IsReloadComplete;
        }
    }
}
