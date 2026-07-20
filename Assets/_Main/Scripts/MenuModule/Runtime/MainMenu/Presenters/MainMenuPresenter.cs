using System;
using UnityEngine;
using Zenject;

namespace MenuModule
{
    public class MainMenuPresenter : IInitializable, IDisposable
    {
        private readonly MainMenuView _view;
        private readonly IMenuNavigationService _navigationService;

        public MainMenuPresenter(MainMenuView view,
            IMenuNavigationService navigationService)
        {
            _view = view;
            _navigationService = navigationService;
        }

        public void Initialize()
        {
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
            _navigationService.LoadGameFromMenu();
        }

        private void OnExitClicked() => _navigationService.QuitGame();
    }
}