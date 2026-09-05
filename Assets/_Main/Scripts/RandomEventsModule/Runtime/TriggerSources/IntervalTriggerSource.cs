namespace RandomEventsModule
{
    /// <summary>Fires on a fixed timer, independent of any signal — a trigger for "enough time has passed".</summary>
    public class IntervalTriggerSource : IRandomEventTriggerSource
    {
        /// <summary>Creates the source with its polling interval.</summary>
        public IntervalTriggerSource(float pollIntervalSeconds)
        {
            PollIntervalSeconds = pollIntervalSeconds;
        }

        /// <inheritdoc/>
        public float PollIntervalSeconds { get; }

        /// <inheritdoc/>
        public bool HandlesSignal(string signal) => false;

        /// <inheritdoc/>
        public bool TryFire() => true;
    }
}
