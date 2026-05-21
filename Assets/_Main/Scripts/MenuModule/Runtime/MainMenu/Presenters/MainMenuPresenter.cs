using System;
using UnityEngine;
using Zenject;

namespace MenuModule
{
    public class MainMenuPresenter : IInitializable, IDisposable
    {
        private readonly MainMenuView _view;
        private readonly IGameNavigationService _navigationService;

        public MainMenuPresenter(MainMenuView view,
            IGameNavigationService navigationService)
        {
            _view = view;
            _navigationService = navigationService;
        }

        public void Initialize()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            _view.Show();

            _view.StartGameClicked += OnStartGameClicked;
            _view.ExitClicked += OnExitClicked;
        }

        public void Dispose()
        {
            _view.StartGameClicked -= OnStartGameClicked;
            _view.ExitClicked -= OnExitClicked;
        }

        private void OnStartGameClicked()
        {
            _navigationService.LoadHeadquarters();
        }

        private void OnExitClicked() => _navigationService.QuitGame();
    }
}