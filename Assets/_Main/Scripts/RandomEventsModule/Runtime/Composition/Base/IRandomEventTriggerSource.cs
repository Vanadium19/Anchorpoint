namespace RandomEventsModule
{
    /// <summary>Decides <em>when</em> a trigger is allowed to roll: on a timer, on an accumulated signal, or both.</summary>
    /// <remarks>
    /// The runner polls a source every <see cref="PollIntervalSeconds"/> and also whenever a reported
    /// signal is claimed by <see cref="HandlesSignal"/>. <see cref="TryFire"/> is what actually consumes
    /// the readiness, so a source that counts signals decrements its counter there.
    /// </remarks>
    public interface IRandomEventTriggerSource
    {
        /// <summary>Seconds between periodic polls; zero means the source only reacts to signals.</summary>
        float PollIntervalSeconds { get; }

        /// <summary>Whether a reported signal should poll this source immediately.</summary>
        bool HandlesSignal(string signal);

        /// <summary>Consumes readiness and returns whether the trigger may roll now.</summary>
        bool TryFire();
    }
}
