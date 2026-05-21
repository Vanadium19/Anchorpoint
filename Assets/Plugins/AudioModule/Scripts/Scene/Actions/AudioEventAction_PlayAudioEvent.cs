using System;
using UnityEngine;

namespace AudioModule
{
    [Serializable]
    public sealed class AudioEventAction_PlayAudioEvent : IAudioEventAction
    {
        [SerializeField]
        private AudioEventKey eventId;

        [SerializeField]
        private float _maxFrequency;

        public void Invoke(in AudioSourceEvent evt) => 
            evt.System.PlayEvent(this.eventId, evt.Position, evt.Rotation, this._maxFrequency);
    }
}