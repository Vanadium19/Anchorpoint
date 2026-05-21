using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AudioModule
{
    [Serializable, InlineProperty] 
    public class ClipConst : IClipProvider
    {
        [SerializeField]
        private AudioClip clip;

        public AudioClip Value => this.clip;

        public float MaxLength => this.clip != null ? clip.length : 0;
    }
}