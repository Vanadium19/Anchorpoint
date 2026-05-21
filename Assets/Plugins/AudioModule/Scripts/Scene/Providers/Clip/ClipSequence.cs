using System;
using UnityEngine;

namespace AudioModule
{
    [Serializable]
    public class ClipSequence : IClipProvider
    {
        [SerializeField]
        private AudioClip[] clips;

        private int _pointer;

        public AudioClip Value
        {
            get
            {
                AudioClip clip = this.clips[_pointer];
                _pointer = (_pointer + 1) % this.clips.Length;
                return clip;
            }
        }

        public float MaxLength
        {
            get
            {
                float maxLength = 0;
                foreach (var clip in this.clips)
                    if (clip != null && clip.length > maxLength)
                        maxLength = clip.length;

                return maxLength;
            }
        }
    }
}