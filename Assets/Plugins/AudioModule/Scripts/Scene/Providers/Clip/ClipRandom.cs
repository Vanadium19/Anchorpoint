using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AudioModule
{
    [Serializable]
    public class ClipRandom : IClipProvider
    {
        [SerializeField]
        private AudioClip[] clips;

        public AudioClip Value
        {
            get
            {
                int randomIndex = Random.Range(0, this.clips.Length);
                return this.clips[randomIndex];
            }
        }

        public float MaxLength
        {
            get
            {
                float maxLength = 0;
                foreach (var clip in this.clips)
                {
                    if (clip != null && clip.length > maxLength)
                    {
                        maxLength = clip.length;
                    }
                }

                return maxLength;
            }
        }
    }
}