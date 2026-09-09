namespace RandomEventsModule
{
    /// <summary>Runs a scope's triggers: polls the periodic ones and reacts to reported signals.</summary>
    /// <remarks>
    /// The core has no knowledge of what signals exist — they are opaque string keys. Game modules
    /// must not depend on this interface directly; signals are reported by relays that live in the
    /// orchestration layer, so a module stays unaware that random events exist at all.
    /// </remarks>
    public interface IRandomEventTriggerRunner
    {
        /// <summary>Starts polling the scope's periodic triggers.</summary>
        void StartRunning();

        /// <summary>Stops polling the scope's periodic triggers.</summary>
        void StopRunning();

        /// <summary>Records an external fact under the given key and polls every trigger that listens for it.</summary>
        RandomEventStartResult ReportSignal(string signal, int amount = 1);

        /// <summary>Polls every trigger of the scope right now, ignoring their periodic timers.</summary>
        RandomEventStartResult Evaluate();
    }
}
