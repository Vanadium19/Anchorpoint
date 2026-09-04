namespace RandomEventsModule
{
    /// <summary>
    /// An immutable snapshot of a running event, taken at start time so that later inspector
    /// edits to the definition cannot change how the running instance behaves.
    /// </summary>
    public class RandomEventInfo
    {
        /// <summary>Creates a snapshot for one started event.</summary>
        public RandomEventInfo(RandomEventDefinition definition, string displayNameKey, bool isExclusive)
        {
            Definition = definition;
            DisplayNameKey = displayNameKey;
            IsExclusive = isExclusive;
        }

        /// <summary>The event's definition asset.</summary>
        public RandomEventDefinition Definition { get; }

        /// <summary>Localization key for the display name, captured at start time.</summary>
        public string DisplayNameKey { get; }

        /// <summary>Whether this event blocks others from starting while active, captured at start time.</summary>
        public bool IsExclusive { get; }
    }
}
