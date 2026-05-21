using System;
using AudioModule;

namespace MenuModule
{
    public class SettingsMenuPresenter : IDisposable
    {
        private readonly SettingsMenuView _view;
        private readonly IAudioSettingsService _audioSettingsService;

        private bool _isInitialized;

        public SettingsMenuPresenter(SettingsMenuView view, IAudioSettingsService audioSettingsService)
        {
            _view = view;
            _audioSettingsService = audioSettingsService;
        }

        public void Initialize()
        {
            if (_isInitialized || _view == null || _audioSettingsService == null)
                return;

            _isInitialized = true;

            _view.SetMusicVolume(_audioSettingsService.GetVolume(AudioMixerChannel.Music));
            _view.SetSfxVolume(_audioSettingsService.GetVolume(AudioMixerChannel.Sfx));

            _view.MusicVolumeChanged += OnMusicVolumeChanged;
            _view.SfxVolumeChanged += OnSfxVolumeChanged;

            _audioSettingsService.VolumeChanged += OnVolumeChangedFromService;
        }

        public void Dispose()
        {
            if (!_isInitialized || _view == null || _audioSettingsService == null)
                return;

            _view.MusicVolumeChanged -= OnMusicVolumeChanged;
            _view.SfxVolumeChanged -= OnSfxVolumeChanged;

            _audioSettingsService.VolumeChanged -= OnVolumeChangedFromService;

            _isInitialized = false;
        }

        private void OnMusicVolumeChanged(float value) => _audioSettingsService.SetVolume(AudioMixerChannel.Music, value);

        private void OnSfxVolumeChanged(float value) => _audioSettingsService.SetVolume(AudioMixerChannel.Sfx, value);

        private void OnVolumeChangedFromService(AudioMixerChannel channel, float volume)
        {
            switch (channel)
            {
                case AudioMixerChannel.Music:
                    _view.SetMusicVolume(volume);
                    break;

                case AudioMixerChannel.Sfx:
                    _view.SetSfxVolume(volume);
                    break;
            }
        }
    }
}
