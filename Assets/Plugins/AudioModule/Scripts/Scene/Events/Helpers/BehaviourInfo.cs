using System;
using UnityEngine;

namespace AudioModule
{
    [Serializable]
    internal sealed class BehaviourInfo
    {
        [SerializeField]
        public bool loop = true;

        [SerializeReference]
        public IAudioEventBehaviour value;

        [SerializeField, Space]
        public bool mute;
    }
}