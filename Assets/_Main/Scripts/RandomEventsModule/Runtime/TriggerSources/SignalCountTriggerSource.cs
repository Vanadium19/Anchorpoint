namespace RandomEventsModule
{
    /// <summary>Fires once a reported signal's stored counter reaches a required amount, then resets it.</summary>
    /// <remarks>The counter lives under the signal's own key, the same value <see cref="IRandomEventTriggerRunner.ReportSignal"/> accumulates, so there is no separate counter key to keep in sync.</remarks>
    public class SignalCountTriggerSource : IRandomEventTriggerSource
    {
        private readonly IRandomEventStateStore _store;
        private readonly string _signal;
        private readonly int _requiredCount;

        /// <summary>Creates the source over one signal key and the count it must reach.</summary>
        public SignalCountTriggerSource(IRandomEventStateStore store, string signal, int requiredCount)
        {
            _store = store;
            _signal = signal;
            _requiredCount = requiredCount;
        }

        /// <inheritdoc/>
        public float PollIntervalSeconds => 0f;

        /// <inheritdoc/>
        public bool HandlesSignal(string signal) => signal == _signal;

        /// <inheritdoc/>
        public bool TryFire()
        {
            if (_store.GetInt(_signal) < _requiredCount)
                return false;

            _store.SetInt(_signal, 0);

            return true;
        }
    }
}
