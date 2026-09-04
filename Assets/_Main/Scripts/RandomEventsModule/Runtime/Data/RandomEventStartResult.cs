namespace RandomEventsModule
{
    /// <summary>Outcome of a call to <see cref="IRandomEventService.Start"/> or <see cref="IRandomEventService.StartRandom"/>.</summary>
    public enum RandomEventStartResult
    {
        /// <summary>The event started.</summary>
        Started = 0,

        /// <summary>The requested definition was null.</summary>
        NotFound = 1,

        /// <summary>The event (or its definition) is disabled.</summary>
        Disabled = 2,

        /// <summary>The event already has a running instance.</summary>
        AlreadyActive = 3,

        /// <summary>Blocked by cooldown, exclusivity or the minimum interval between events.</summary>
        Blocked = 4,

        /// <summary>No eligible candidate was found to pick from.</summary>
        NoCandidates = 5,
    }
}
