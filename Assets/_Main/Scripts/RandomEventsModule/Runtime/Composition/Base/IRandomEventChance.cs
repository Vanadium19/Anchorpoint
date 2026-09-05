namespace RandomEventsModule
{
    /// <summary>A probability roll a trigger makes before it asks for an event.</summary>
    /// <remarks>Rolling happens before any event is picked, so a failed roll leaves cooldowns and the minimum interval untouched.</remarks>
    public interface IRandomEventChance
    {
        /// <summary>Rolls and returns whether the trigger may proceed.</summary>
        bool Roll();
    }
}
