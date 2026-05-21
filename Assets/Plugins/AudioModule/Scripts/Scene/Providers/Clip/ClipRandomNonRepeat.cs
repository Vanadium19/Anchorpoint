using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AudioModule
{
    [Serializable]
    public sealed class ClipRandomNonRepeat : IClipProvider
    {
        [SerializeField]
        private AudioClip[] clips;
        
        private readonly List<AudioClip> _queue = new();

        public AudioClip Value
        {
            get
            {
                if (_queue.Count == 0) 
                    _queue.AddRange(this.clips);

                int randomIndex = Random.Range(0, _queue.Count);
                AudioClip randomClip = _queue[randomIndex];
                _queue.Remove(randomClip);
                
                return randomClip;
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