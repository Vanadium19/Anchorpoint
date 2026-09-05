namespace RandomEventsModule
{
    /// <summary>How multiple conditions in a set combine into one pass/fail result.</summary>
    public enum ConditionMatchType
    {
        /// <summary>Every condition must be met.</summary>
        All = 0,

        /// <summary>At least one condition must be met.</summary>
        Any = 1,
    }
}
