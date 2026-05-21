using System;
using UnityEngine;

namespace AudioModule
{
    [Serializable]
    internal sealed class AudioEventBehaviour_PitchParameter : AudioEventBehaviour
    {
        [SerializeField]
        private AudioParameterKey pitchKey;

        public override void OnUpdate(in AudioSourceEvent evt)
        {
            if (evt.System.TryGetFloat(this.pitchKey, out float pitch))
                evt.Source.pitch = pitch;
        }

        public override void OnReset(in AudioSourceEvent evt)
        {
            if (evt.System.TryGetFloat(this.pitchKey, out float pitch))
                evt.Source.pitch = pitch;
        }
    }
}