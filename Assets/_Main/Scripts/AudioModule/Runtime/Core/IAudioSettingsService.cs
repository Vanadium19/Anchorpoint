using System;

namespace AudioModule
{
    public interface IAudioSettingsService
    {
        event Action<AudioMixerChannel, float> VolumeChanged;

        float GetVolume(AudioMixerChannel channel);
        void SetVolume(AudioMixerChannel channel, float volume);
    }
}
