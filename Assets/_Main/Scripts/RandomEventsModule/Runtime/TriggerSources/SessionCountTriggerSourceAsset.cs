using System;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Asset wrapper for <see cref="SessionCountTriggerSource"/>.</summary>
    [Serializable]
    public class SessionCountTriggerSourceAsset : ZenjectRandomEventTriggerSourceAsset<SessionCountTriggerSource>
    {
        [SerializeField] [RandomEventSignalPicker] private string counterKey = SessionCountTriggerSource.SortieCountKey;
        [SerializeField] [Min(1)] private int requiredCount = 3;
        [SerializeField] [Min(0f)] private float pollIntervalSeconds = 5f;

        /// <inheritdoc/>
        protected override object[] GetArguments(DiContainer container) => new object[] { counterKey, requiredCount, pollIntervalSeconds };
    }
}
