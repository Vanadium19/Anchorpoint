using System;
using UnityEngine;
using UtilsModule;
using Zenject;

namespace MenuModule
{
    public class MainMenuPresenter : IInitializable, IDisposable
    {
        private const string EnglishLocaleCode = "en";
        private const string RussianLocaleCode = "ru";

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

            LocaleSelector.RestoreSaved();

            _view.Show();

            _view.StartGameClicked += OnStartGameClicked;
            _view.ExitClicked += OnExitClicked;
            _view.EnglishClicked += OnEnglishClicked;
            _view.RussianClicked += OnRussianClicked;
        }

        public void Dispose()
        {
            _view.StartGameClicked -= OnStartGameClicked;
            _view.ExitClicked -= OnExitClicked;
            _view.EnglishClicked -= OnEnglishClicked;
            _view.RussianClicked -= OnRussianClicked;
        }

        private void OnStartGameClicked()
        {
            _navigationService.LoadGameFromMenu();
        }

        private void OnExitClicked() => _navigationService.QuitGame();

        private void OnEnglishClicked() => LocaleSelector.Apply(EnglishLocaleCode);

        private void OnRussianClicked() => LocaleSelector.Apply(RussianLocaleCode);
    }
}