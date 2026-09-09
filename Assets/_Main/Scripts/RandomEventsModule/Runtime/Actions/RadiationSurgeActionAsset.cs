using System;
using AudioModule;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Asset wrapper for <see cref="RadiationSurgeAction"/>.</summary>
    [Serializable]
    public class RadiationSurgeActionAsset : ZenjectRandomEventActionAsset<RadiationSurgeAction>
    {
        [SerializeField] [Min(0.1f)] private float durationSeconds = 15f;
        [SerializeField] [Min(0f)] private float damagePerTick = 5f;
        [SerializeField] [Min(0.1f)] private float tickIntervalSeconds = 1f;
        [SerializeField] private string geigerAudioEventId = RadiationBankAPI.GeigerCounterEvent;

        /// <inheritdoc/>
        protected override object[] GetArguments(DiContainer container) => new object[]
        {
            durationSeconds, damagePerTick, tickIntervalSeconds, geigerAudioEventId
        };
    }
}
