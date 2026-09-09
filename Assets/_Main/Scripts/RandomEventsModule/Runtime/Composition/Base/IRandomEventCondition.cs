namespace RandomEventsModule
{
    /// <summary>A single check used by a gate action to decide whether to proceed.</summary>
    public interface IRandomEventCondition
    {
        /// <summary>Returns whether the condition currently holds.</summary>
        bool IsMet();
    }
}
