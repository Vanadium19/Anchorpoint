namespace RandomEventsModule
{
    /// <summary>Fires once every X sorties into the scope's scene, then resets its own counter.</summary>
    /// <remarks>
    /// The counter lives under <paramref name="counterKey"/> and is saved state, so it survives across
    /// sorties. Since a scope and its trigger sources are constructed exactly once per scene visit, the
    /// constructor counts the current sortie by adding one to the counter on creation. Fires at most once
    /// per session, even if polled again after firing.
    /// </remarks>
    public class SessionCountTriggerSource : IRandomEventTriggerSource
    {
        [RandomEventSignalKey] public const string SortieCountKey = "sortie_count";

        private readonly IRandomEventStateStore _store;
        private readonly string _counterKey;
        private readonly int _requiredCount;
        private readonly float _pollIntervalSeconds;
        private bool _hasFired;

        /// <summary>Creates the source over the sortie counter key, its required count and poll delay.</summary>
        public SessionCountTriggerSource(IRandomEventStateStore store, string counterKey, int requiredCount, float pollIntervalSeconds)
        {
            _store = store;
            _counterKey = counterKey;
            _requiredCount = requiredCount;
            _pollIntervalSeconds = pollIntervalSeconds;

            _store.AddInt(_counterKey, 1);
        }

        /// <inheritdoc/>
        public float PollIntervalSeconds => _pollIntervalSeconds;

        /// <inheritdoc/>
        public bool HandlesSignal(string signal) => false;

        /// <inheritdoc/>
        public bool TryFire()
        {
            if (_hasFired)
                return false;

            if (_store.GetInt(_counterKey) < _requiredCount)
                return false;

            _store.SetInt(_counterKey, 0);
            _hasFired = true;

            return true;
        }
    }
}
