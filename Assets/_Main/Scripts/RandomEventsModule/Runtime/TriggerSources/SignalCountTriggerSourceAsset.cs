using System;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Asset wrapper for <see cref="SignalCountTriggerSource"/>.</summary>
    [Serializable]
    public class SignalCountTriggerSourceAsset : ZenjectRandomEventTriggerSourceAsset<SignalCountTriggerSource>
    {
        [SerializeField] [RandomEventSignalPicker] private string signal;
        [SerializeField] [Min(1)] private int requiredCount = 1;

        /// <inheritdoc/>
        protected override object[] GetArguments(DiContainer container) => new object[] { signal, requiredCount };
    }
}
