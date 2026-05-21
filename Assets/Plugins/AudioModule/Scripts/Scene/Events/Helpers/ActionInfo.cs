using System;
using UnityEngine;

namespace AudioModule
{
    [Serializable]
    internal sealed class ActionInfo
    {
        [SerializeField]
        public float time;

        [SerializeReference]
        public IAudioEventAction value;

        [SerializeField, Space]
        public bool mute;

        internal bool passed;
    }
}