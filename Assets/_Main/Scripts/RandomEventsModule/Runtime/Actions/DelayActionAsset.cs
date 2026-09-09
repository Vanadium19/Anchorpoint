using System;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Asset wrapper for <see cref="DelayAction"/>.</summary>
    [Serializable]
    public class DelayActionAsset : ZenjectRandomEventActionAsset<DelayAction>
    {
        [SerializeField] [Min(0f)] private float durationSeconds = 15f;

        /// <inheritdoc/>
        protected override object[] GetArguments(DiContainer container) => new object[] { durationSeconds };
    }
}
