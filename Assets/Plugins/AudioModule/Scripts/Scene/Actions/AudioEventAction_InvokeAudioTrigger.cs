using System;
using UnityEngine;

namespace AudioModule
{
    [Serializable]
    public sealed class AudioEventAction_InvokeAudioTrigger : IAudioEventAction
    {
        [SerializeField]
        private string triggerId;
        
        public void Invoke(in AudioSourceEvent evt) => evt.System.InvokeTrigger(this.triggerId);
    }
}