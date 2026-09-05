using System;

namespace RandomEventsModule
{
    /// <summary>Marks a <c>public const string</c> as a signal/state key discoverable by <see cref="RandomEventSignalPickerAttribute"/>.</summary>
    /// <remarks>Put it on the same constant a relay reports and a trigger source is configured to listen for, so both stay in sync.</remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class RandomEventSignalKeyAttribute : Attribute
    {
    }
}
