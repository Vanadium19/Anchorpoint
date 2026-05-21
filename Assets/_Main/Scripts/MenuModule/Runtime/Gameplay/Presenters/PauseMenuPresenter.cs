using System;
using AudioModule;
using Zenject;

namespace MenuModule
{
    public class PauseMenuPresenter : IInitializable, IDisposable
    {
        private readonly PauseMenuView _view;

        private readonly IGamePauseService _pauseService;
        private readonly IGameNavigationService _navigationService;

        private readonly IAudioSettingsService _audioSettingsService;

        private SettingsMenuPresenter _settingsPresenter;

        public PauseMenuPresenter(PauseMenuView view,
            IGamePauseService pauseService,
            IGameNavigationService navigationService,
            IAudioSettingsService audioSettingsService)
        {
            _view = view;
            _pauseService = pauseService;
            _navigationService = navigationService;
            _audioSettingsService = audioSettingsService;
        }

        public void Initialize()
        {
            _view.HideSettings();
            _view.Hide();

            _settingsPresenter = new(_view.SettingsMenuView, _audioSettingsService);
            _settingsPresenter.Initialize();

            _view.SettingsClicked += OnSettingsClicked;
            _view.MainMenuClicked += OnMainMenuClicked;

            _pauseService.PauseStateChanged += OnPauseStateChanged;
        }

        public void Dispose()
        {
            _view.SettingsClicked -= OnSettingsClicked;
            _view.MainMenuClicked -= OnMainMenuClicked;

            _pauseService.PauseStateChanged -= OnPauseStateChanged;

            _settingsPresenter?.Dispose();
        }

        private void OnSettingsClicked() => _view.SettingsMenuView?.Toggle();

        private void OnMainMenuClicked() => _navigationService.LoadMainMenu();

        private void OnPauseStateChanged(bool isPaused) => ApplyUserPauseState();

        private void ApplyUserPauseState()
        {
            if (_pauseService.HasReason(GamePauseReason.UserPause))
            {
                _view.Show();
                return;
            }

            _view.HideSettings();
            _view.Hide();
        }
    }
}