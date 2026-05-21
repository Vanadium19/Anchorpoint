using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AudioModule
{
    [Serializable]
    public sealed class ClipRandomWeight : IClipProvider, ISerializationCallbackReceiver
    {
        [Serializable]
        private struct ClipInfo
        {
            public int weight;
            public AudioClip value;
        }

        [SerializeField]
        private ClipInfo[] clips;

        private int _sum;

        public AudioClip Value
        {
            get
            {
                int acc = 0;
                int randomValue = Random.Range(0, _sum);

                int lastIndex = this.clips.Length - 1;
                for (int i = 0; i < lastIndex; i++)
                {
                    ClipInfo clipInfo = this.clips[i];
                    acc += clipInfo.weight;

                    if (acc > randomValue)
                        return clipInfo.value;
                }

                return this.clips[lastIndex].value;
            }
        }

        public float MaxLength
        {
            get
            {
                float maxLength = 0;
                foreach (ClipInfo clip in this.clips)
                    if (clip.value != null && clip.value.length > maxLength)
                        maxLength = clip.value.length;

                return maxLength;
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _sum = 0;

            foreach (ClipInfo clip in this.clips) 
                _sum += clip.weight;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }
    }
}