using FSMModule;

namespace EnemyModule
{
    public sealed class ReloadToChaseTransition : AbstractStateTransition<StateName>
    {
        private readonly ReloadState _reloadState;

        public ReloadToChaseTransition(ReloadState reloadState) : base(StateName.Reload, StateName.Chase)
        {
            _reloadState = reloadState;
        }

        public override bool CanPerform() => _reloadState.IsReloadComplete;
    }
}