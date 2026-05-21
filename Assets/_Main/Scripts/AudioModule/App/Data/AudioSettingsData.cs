using System;
using Unity.Plastic.Newtonsoft.Json;

namespace AudioModule
{
    [Serializable]
    public class AudioSettingsData
    {
        [JsonProperty] public float MusicVolume;

        [JsonProperty] public float SfxVolume;

        public AudioSettingsData() : this(1f, 1f) { }

        public AudioSettingsData(float musicVolume, float sfxVolume)
        {
            MusicVolume = musicVolume;
            SfxVolume = sfxVolume;
        }
    }
}