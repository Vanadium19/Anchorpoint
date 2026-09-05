using System;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Asset wrapper for <see cref="StateScaledChance"/>.</summary>
    [Serializable]
    public class StateScaledChanceAsset : ZenjectRandomEventChanceAsset<StateScaledChance>
    {
        [SerializeField] private RandomEventKey counterKey;
        [SerializeField] [Range(0f, 100f)] private float basePercent = 30f;
        [SerializeField] [Range(0f, 100f)] private float stepPercent = 10f;
        [SerializeField] [Range(0f, 100f)] private float maxPercent = 100f;

        /// <inheritdoc/>
        protected override object[] GetArguments(DiContainer container) =>
            new object[] { counterKey, basePercent, stepPercent, maxPercent };
    }
}
