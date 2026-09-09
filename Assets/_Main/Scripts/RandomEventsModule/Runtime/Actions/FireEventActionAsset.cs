using System;
using AudioModule;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Asset wrapper for <see cref="FireEventAction"/>.</summary>
    [Serializable]
    public class FireEventActionAsset : ZenjectRandomEventActionAsset<FireEventAction>
    {

        [Header("VFX")]
        [SerializeField] private string ignitionAudioEventId = FireBankAPI.IgnitionEvent;
        [SerializeField] private string burningAudioEventId = FireBankAPI.BurningEvent;
        [SerializeField] private string extinguishAudioEventId = FireBankAPI.ExtinguishEvent;
        [SerializeField] private string extinguishHintKey = "randomEvents.fire.extinguishHint";

        [Header("Settings")]
        [SerializeField] [Min(0f)] private float spreadRadius = 6f;
        [SerializeField] [Min(0f)] private float spreadIntervalSeconds = 10f;
        [SerializeField] [Min(1)] private int maxBurningBuildings = 3;
        [SerializeField] [Min(0f)] private float extinguishRadius = 3f;
        [SerializeField] [Min(0f)] private float extinguishSeconds = 3f;

        /// <inheritdoc/>
        protected override object[] GetArguments(DiContainer container) => new object[]
        {
            ignitionAudioEventId, burningAudioEventId, extinguishAudioEventId, extinguishHintKey,
            spreadRadius, spreadIntervalSeconds, maxBurningBuildings,
            extinguishRadius, extinguishSeconds
        };
    }
}
