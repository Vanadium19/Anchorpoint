using EvacuationModule;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Raises the raid chance counter on every base visit that ended without a raid.</summary>
    /// <remarks>
    /// The counter grows when the player leaves the base, so the higher chance applies from the next
    /// return onward, as the raid rules require. The raid event itself marks the visit with
    /// <see cref="RaidOccurredKey"/> and resets <see cref="MissedReturnsKey"/> through
    /// <see cref="SetStateAction"/> steps — this relay only reads that mark.
    /// Both keys are saved state: renaming either orphans the accumulated value in the save.
    /// </remarks>
    public class RaidChanceRelay : RandomEventSignalRelay
    {
        [RandomEventSignalKey] public const string MissedReturnsKey = "raid_missed_returns";

        [RandomEventSignalKey] public const string RaidOccurredKey = "raid_occurred";

        private readonly IEvacuationService _evacuationService;
        private readonly IRandomEventStateStore _store;

        /// <summary>Creates the relay over the scene evacuation service and state store.</summary>
        public RaidChanceRelay(
            IRandomEventTriggerRunner triggerRunner,
            [InjectOptional] IEvacuationService evacuationService,
            IRandomEventStateStore store)
            : base(triggerRunner, MissedReturnsKey)
        {
            _evacuationService = evacuationService;
            _store = store;
        }

        /// <inheritdoc/>
        protected override void Subscribe()
        {
            if (_evacuationService != null)
                _evacuationService.Completed += OnEvacuationCompleted;
        }

        /// <inheritdoc/>
        protected override void Unsubscribe()
        {
            if (_evacuationService != null)
                _evacuationService.Completed -= OnEvacuationCompleted;
        }

        private void OnEvacuationCompleted()
        {
            if (_store.GetInt(RaidOccurredKey) != 0)
            {
                _store.SetInt(RaidOccurredKey, 0);

                return;
            }

            Report();
        }
    }
}
