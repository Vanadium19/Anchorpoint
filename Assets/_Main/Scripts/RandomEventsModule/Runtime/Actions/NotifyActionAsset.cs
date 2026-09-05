using System;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Asset wrapper for <see cref="NotifyAction"/>.</summary>
    [Serializable]
    public class NotifyActionAsset : ZenjectRandomEventActionAsset<NotifyAction>
    {
        [SerializeField] private string localizationKey;
        [SerializeField] [Min(0f)] private float durationSeconds = 4f;

        /// <inheritdoc/>
        protected override object[] GetArguments(DiContainer container) => new object[] { localizationKey, durationSeconds };
    }
}
