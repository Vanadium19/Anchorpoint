using System;
using System.Collections.Generic;
using UnityEngine;

namespace AudioModule
{
    public class AudioMixerSettingsService : IAudioSettingsService
    {
        private const float MinDecibels = -80f;
        private const float MutedVolumeThreshold = 0.0001f;

        private readonly AudioSettingsConfig _config;

        private readonly Dictionary<AudioMixerChannel, float> _volumes = new();

        public event Action<AudioMixerChannel, float> VolumeChanged;

        public AudioMixerSettingsService(AudioSettingsConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            if (!_config.Mixer)
                throw new NullReferenceException(nameof(_config.Mixer));

            SetVolume(AudioMixerChannel.Music, 1f);
            SetVolume(AudioMixerChannel.Sfx, 1f);
        }

        public float GetVolume(AudioMixerChannel channel) => _volumes[channel];

        public void SetVolume(AudioMixerChannel channel, float volume)
        {
            volume = Mathf.Clamp01(volume);

            if (_volumes.TryGetValue(channel, out var currentVolume) && Mathf.Approximately(currentVolume, volume))
                return;

            _volumes[channel] = volume;
            ApplyVolume(channel, volume);
            VolumeChanged?.Invoke(channel, volume);
        }

        private void ApplyVolume(AudioMixerChannel channel, float volume)
        {
            var mixerParameter = _config.GetVolumeParameter(channel);

            if (string.IsNullOrWhiteSpace(mixerParameter))
                throw new ArgumentException($"AudioMixer parameter '{channel}' is invalid.");

            var decibels = ToDecibels(volume);
            _config.Mixer.SetFloat(mixerParameter, decibels);
        }

        private float ToDecibels(float volume) => volume <= MutedVolumeThreshold ? MinDecibels : Mathf.Log10(volume) * 20f;
    }
}