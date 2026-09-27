namespace EvacuationModule
{
    /// <summary>
    /// Optional veto over starting an evacuation. Asks whether the player may start evacuating right now.
    /// </summary>
    /// <remarks>
    /// The implementer may also react to the refusal, for example by showing a message. The service treats a missing gate as always allowed.
    /// </remarks>
    public interface IEvacuationGate
    {
        /// <summary>
        /// Returns false when something forbids evacuation.
        /// </summary>
        bool TryEnterEvacuation();
    }
}
