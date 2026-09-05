namespace RandomEventsModule
{
    /// <summary>Saveable surface of the module's state store.</summary>
    public interface IRandomEventsState
    {
        /// <summary>Captures the current state for saving.</summary>
        RandomEventsMemento CreateSnapshot();

        /// <summary>Replaces the current state with a loaded snapshot.</summary>
        void RestoreSnapshot(RandomEventsMemento memento);
    }
}
