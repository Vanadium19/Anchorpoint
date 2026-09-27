using System;

namespace EvacuationModule
{
    /// <summary>
    /// Optional veto over starting or continuing an evacuation.
    /// </summary>
    /// <remarks>
    /// The implementer may also react to the refusal, for example by showing a message. The service treats a missing gate as always allowed.
    /// </remarks>
    public interface IEvacuationGate
    {
        /// <summary>
        /// Raised when evacuation becomes forbidden; an evacuation already in progress must stop.
        /// </summary>
        event Action Blocked;

        /// <summary>
        /// Returns false when something forbids evacuation.
        /// </summary>
        bool TryEnterEvacuation();
    }
}
