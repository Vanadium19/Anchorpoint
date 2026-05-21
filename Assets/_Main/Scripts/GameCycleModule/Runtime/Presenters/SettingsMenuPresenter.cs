using System;

namespace GameCycleModule
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

            _view.SetMusicVolume(_audioSettingsService.MusicVolume);
            _view.SetSfxVolume(_audioSettingsService.SfxVolume);

            _view.MusicVolumeChanged += OnMusicVolumeChanged;
            _view.SfxVolumeChanged += OnSfxVolumeChanged;

            _audioSettingsService.MusicVolumeChanged += OnMusicVolumeChangedFromService;
            _audioSettingsService.SfxVolumeChanged += OnSfxVolumeChangedFromService;
        }

        public void Dispose()
        {
            if (!_isInitialized || _view == null || _audioSettingsService == null)
                return;

            _view.MusicVolumeChanged -= OnMusicVolumeChanged;
            _view.SfxVolumeChanged -= OnSfxVolumeChanged;

            _audioSettingsService.MusicVolumeChanged -= OnMusicVolumeChangedFromService;
            _audioSettingsService.SfxVolumeChanged -= OnSfxVolumeChangedFromService;

            _isInitialized = false;
        }

        private void OnMusicVolumeChanged(float value) => _audioSettingsService.SetMusicVolume(value);

        private void OnSfxVolumeChanged(float value) => _audioSettingsService.SetSfxVolume(value);

        private void OnMusicVolumeChangedFromService(float value) => _view.SetMusicVolume(value);

        private void OnSfxVolumeChangedFromService(float value) => _view.SetSfxVolume(value);
    }
}