using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AudioModule
{
    [CreateAssetMenu(fileName = "UISoundCatalog", menuName = "Game/Audio/UI/SoundCatalog")]
    public sealed class UISoundCatalog : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        public bool isMain = true;
        
        [Serializable, InlineProperty]
        public struct SoundInfo
        {
            [HorizontalGroup]
            public string id;

            [HorizontalGroup]
            public AudioClip clip;
        }
        
        [SerializeField]
        public SoundInfo[] sounds;

        private Dictionary<string, AudioClip> soundMap;

        public int GetCount()
        {
            return this.sounds.Length;
        }

        public (string, AudioClip) GetSoundAt(int index)
        {
            SoundInfo sound = this.sounds[index];
            return (sound.id, sound.clip);
        }

        public int IndexOfSound(string soundId)
        {
            for (int i = 0, count = this.sounds.Length; i < count; i++)
            {
                SoundInfo soundInfo = this.sounds[i];
                if (soundInfo.id == soundId)
                {
                    return i;
                }
            }

            return -1;
        }

        public bool TryGetSound(string id, out AudioClip sound)
        {
            return this.soundMap.TryGetValue(id, out sound);
        }

        public string[] GetKeys()
        {
            return this.soundMap.Keys.ToArray();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.soundMap = new Dictionary<string, AudioClip>();
            
            foreach (var sound in this.sounds)
            {
                if (!string.IsNullOrEmpty(sound.id))
                {
                    this.soundMap[sound.id] = sound.clip;
                }
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }
    }
}