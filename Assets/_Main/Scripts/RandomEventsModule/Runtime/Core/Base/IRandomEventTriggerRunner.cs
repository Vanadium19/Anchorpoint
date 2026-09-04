namespace RandomEventsModule
{
    /// <summary>Periodically (and on demand) evaluates whether a random event should start for a given scope.</summary>
    /// <remarks>
    /// The core has no knowledge of what signals exist — they are opaque key assets. Game modules
    /// must not depend on this interface directly; signals are reported by relays that live in the
    /// orchestration layer, so a module stays unaware that random events exist at all.
    /// </remarks>
    public interface IRandomEventTriggerRunner
    {
        /// <summary>Starts the periodic evaluation loop, if <c>evaluationIntervalSeconds</c> is above zero.</summary>
        void StartRunning();

        /// <summary>Stops the periodic evaluation loop.</summary>
        void StopRunning();

        /// <summary>Records an external fact under the given key and immediately evaluates.</summary>
        void ReportSignal(RandomEventKey signal, int amount = 1);

        /// <summary>Runs one evaluation pass now, regardless of the periodic loop.</summary>
        RandomEventStartResult Evaluate();
    }
}
