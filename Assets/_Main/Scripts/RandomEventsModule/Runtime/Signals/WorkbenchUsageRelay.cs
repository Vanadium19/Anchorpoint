using BaseModule;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Reports a signal each time the workbench opens — the base's craft station until a dedicated alchemy table exists.</summary>
    /// <remarks>The craft service only exists in scenes with a workbench, so it is injected optionally and the relay is a no-op without it.</remarks>
    public class WorkbenchUsageRelay : RandomEventSignalRelay
    {
        [RandomEventSignalKey] public const string SignalKey = "workbench_usage";

        private readonly ICraftService _craftService;
        private bool _wasOpen;

        /// <summary>Creates the relay bound to the craft service it reports opens from, if the scene has one.</summary>
        public WorkbenchUsageRelay(IRandomEventTriggerRunner triggerRunner, [InjectOptional] ICraftService craftService)
            : base(triggerRunner, SignalKey)
        {
            _craftService = craftService;
        }

        /// <inheritdoc/>
        protected override void Subscribe()
        {
            if (_craftService != null)
                _craftService.StateChanged += OnStateChanged;
        }

        /// <inheritdoc/>
        protected override void Unsubscribe()
        {
            if (_craftService != null)
                _craftService.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged()
        {
            if (_craftService.IsWorkbenchOpen == _wasOpen)
                return;

            _wasOpen = _craftService.IsWorkbenchOpen;

            if (_wasOpen)
                Report();
        }
    }
}
