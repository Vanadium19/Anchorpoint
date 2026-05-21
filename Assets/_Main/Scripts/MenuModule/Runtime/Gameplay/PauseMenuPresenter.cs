using System;
using AudioModule;
using InputModule;
using Zenject;

namespace MenuModule
{
    public class PauseMenuPresenter : IInitializable, ITickable, IDisposable
    {
        private readonly PauseMenuView _view;

        private readonly IInputMap _inputMap;
        private readonly IInputService _inputService;

        private readonly IGamePauseService _pauseService;
        private readonly IGameNavigationService _navigationService;

        private readonly IAudioSettingsService _audioSettingsService;

        private SettingsMenuPresenter _settingsPresenter;

        public PauseMenuPresenter(PauseMenuView view,
            IInputMap inputMap,
            IInputService inputService,
            IGamePauseService pauseService,
            IGameNavigationService navigationService,
            IAudioSettingsService audioSettingsService)
        {
            _view = view;
            _inputMap = inputMap;
            _inputService = inputService;
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

        public void Tick()
        {
            if (_inputMap is not { IsPausePressed: true })
                return;

            if (_pauseService.HasReason(GamePauseReason.Victory))
                return;

            _pauseService.ToggleUserPause();
            ApplyUserPauseState();
        }

        public void Dispose()
        {
            _view.SettingsClicked -= OnSettingsClicked;
            _view.MainMenuClicked -= OnMainMenuClicked;

            _pauseService.PauseStateChanged -= OnPauseStateChanged;

            _settingsPresenter?.Dispose();

            if (_pauseService.HasReason(GamePauseReason.UserPause))
                _pauseService.Resume(GamePauseReason.UserPause);
        }

        private void OnSettingsClicked() => _view.SettingsMenuView?.Toggle();

        private void OnMainMenuClicked() => _navigationService.LoadMainMenu();

        private void OnPauseStateChanged(bool isPaused) => ApplyUserPauseState();

        private void ApplyUserPauseState()
        {
            if (_pauseService.HasReason(GamePauseReason.UserPause))
            {
                _view.Show();
                _inputService.SetUIMode(true);
                return;
            }

            _view.HideSettings();
            _view.Hide();

            if (!_pauseService.IsPaused)
                _inputService.SetUIMode(false);
        }
    }
}
