using System;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Asset wrapper for <see cref="IntervalTriggerSource"/>.</summary>
    [Serializable]
    public class IntervalTriggerSourceAsset : ZenjectRandomEventTriggerSourceAsset<IntervalTriggerSource>
    {
        [SerializeField] [Min(0f)] private float pollIntervalSeconds = 60f;

        /// <inheritdoc/>
        protected override object[] GetArguments(DiContainer container) => new object[] { pollIntervalSeconds };
    }
}
