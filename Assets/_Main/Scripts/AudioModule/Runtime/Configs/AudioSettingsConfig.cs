using System;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioModule
{
    [CreateAssetMenu(fileName = "AudioSettingsConfig", menuName = "Game/Configs/Audio/AudioSettingsConfig")]
    public class AudioSettingsConfig : ScriptableObject
    {
        [SerializeField] private AudioMixer mixer;

        [Header("Volume Parameters")]
        [SerializeField] private string musicVolumeParameter = "MusicVolume";
        [SerializeField] private string sfxVolumeParameter = "SfxVolume";

        public AudioMixer Mixer => mixer;

        public string GetVolumeParameter(AudioMixerChannel channel)
        {
            return channel switch
            {
                AudioMixerChannel.Music => musicVolumeParameter,
                AudioMixerChannel.Sfx => sfxVolumeParameter,
                _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, null),
            };
        }
    }
}