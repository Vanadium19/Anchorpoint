using System;
using System.Collections.Generic;
using System.Threading;

namespace RandomEventsModule
{
    /// <summary>
    /// Drives the lifecycle of random events: starting, running and stopping them while
    /// enforcing cooldowns, exclusivity and the minimum interval between events.
    /// </summary>
    /// <remarks>
    /// Events are addressed by their definition asset, never by a string identifier. Deciding
    /// when an event should start is the trigger runner's job, not this service's.
    /// </remarks>
    public interface IRandomEventService
    {
        /// <summary>Raised right after an event starts.</summary>
        event Action<RandomEventInfo> EventStarted;

        /// <summary>Raised right after an event finishes, regardless of how it ended.</summary>
        event Action<RandomEventInfo> EventFinished;

        /// <summary>Whether any event is currently running.</summary>
        bool IsAnyEventActive { get; }

        /// <summary>Snapshot of the currently running events.</summary>
        IReadOnlyList<RandomEventInfo> ActiveEvents { get; }

        /// <summary>Whether the given definition currently has a running instance.</summary>
        bool IsEventActive(RandomEventDefinition definition);

        /// <summary>Starts a specific event, subject to its own gating (enabled, cooldown, exclusivity).</summary>
        RandomEventStartResult Start(RandomEventDefinition definition, CancellationToken token);

        /// <summary>Picks one eligible event out of the pool with the given picker and starts it.</summary>
        RandomEventStartResult StartFromPool(RandomEventPool pool, IRandomEventPicker picker, CancellationToken token);

        /// <summary>Cancels a running event, if any.</summary>
        void Stop(RandomEventDefinition definition);

        /// <summary>Cancels every running event.</summary>
        void StopAll();
    }
}
