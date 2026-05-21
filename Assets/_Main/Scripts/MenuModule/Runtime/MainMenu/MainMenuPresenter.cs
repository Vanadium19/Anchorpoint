using System;
using AudioModule;
using UnityEngine;
using Zenject;

namespace MenuModule
{
    public class MainMenuPresenter : IInitializable, IDisposable
    {
        private readonly MainMenuView _view;
        private readonly IGameNavigationService _navigationService;
        private readonly IAudioSettingsService _audioSettingsService;
        private SettingsMenuPresenter _settingsPresenter;

        public MainMenuPresenter(MainMenuView view,
            IGameNavigationService navigationService,
            IAudioSettingsService audioSettingsService)
        {
            _view = view;
            _navigationService = navigationService;
            _audioSettingsService = audioSettingsService;
        }

        public void Initialize()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            _view.Show();
            _view.SettingsMenuView?.Hide();

            _settingsPresenter = new(_view.SettingsMenuView, _audioSettingsService);

            _settingsPresenter.Initialize();

            _view.StartGameClicked += OnStartGameClicked;
            _view.SettingsClicked += OnSettingsClicked;
            _view.ExitClicked += OnExitClicked;
        }

        public void Dispose()
        {
            _view.StartGameClicked -= OnStartGameClicked;
            _view.SettingsClicked -= OnSettingsClicked;
            _view.ExitClicked -= OnExitClicked;

            _settingsPresenter?.Dispose();
        }

        private void OnStartGameClicked()
        {
            _navigationService.LoadHeadquarters();
        }

        private void OnSettingsClicked() => _view.SettingsMenuView?.Toggle();

        private void OnExitClicked() => _navigationService.QuitGame();
    }
}
