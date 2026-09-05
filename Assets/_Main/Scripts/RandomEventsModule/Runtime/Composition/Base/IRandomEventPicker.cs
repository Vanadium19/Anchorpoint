using System.Collections.Generic;

namespace RandomEventsModule
{
    /// <summary>Selects one event to start from a pool's eligible candidates.</summary>
    public interface IRandomEventPicker
    {
        /// <summary>Returns the picked candidate's definition, or <c>null</c> if none should start.</summary>
        RandomEventDefinition Pick(IReadOnlyList<RandomEventCandidate> candidates);
    }
}
