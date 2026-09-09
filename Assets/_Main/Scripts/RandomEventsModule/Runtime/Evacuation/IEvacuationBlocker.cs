namespace RandomEventsModule
{
    /// <summary>Holds on the player's ability to evacuate the scene, taken and lifted by event actions.</summary>
    /// <remarks>
    /// A hold is identified by its message key, so several events can block evacuation at once and
    /// lifting one does not lift another. Blocking twice under the same key is one hold, and every
    /// hold is dropped once no random event is running — an event interrupted between
    /// <see cref="Block"/> and <see cref="Unblock"/> cannot leave the scene locked.
    /// </remarks>
    public interface IEvacuationBlocker
    {
        /// <summary>Whether evacuation is forbidden right now.</summary>
        bool IsBlocked { get; }

        /// <summary>Takes a hold and remembers the message shown while it lasts.</summary>
        void Block(string blockedMessageKey, float messageDurationSeconds);

        /// <summary>Lifts the hold taken under the given key; an empty key lifts every hold.</summary>
        void Unblock(string blockedMessageKey);
    }
}
