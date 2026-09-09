using System;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Asset wrapper for <see cref="FixedChance"/>.</summary>
    [Serializable]
    public class FixedChanceAsset : ZenjectRandomEventChanceAsset<FixedChance>
    {
        [SerializeField] [Range(0f, 100f)] private float percent = 100f;

        /// <inheritdoc/>
        protected override object[] GetArguments(DiContainer container) => new object[] { percent };
    }
}
