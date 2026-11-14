using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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

        private CancellationTokenSource _localeTokenSource;

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

            _localeTokenSource = new CancellationTokenSource();
            InitializeAsync(_localeTokenSource.Token).Forget();
        }

        public void Dispose()
        {
            _view.StartGameClicked -= OnStartGameClicked;
            _view.ExitClicked -= OnExitClicked;
            _view.EnglishClicked -= OnEnglishClicked;
            _view.RussianClicked -= OnRussianClicked;

            _localeTokenSource?.Cancel();
            _localeTokenSource?.Dispose();
            _localeTokenSource = null;
        }

        private async UniTaskVoid InitializeAsync(CancellationToken token)
        {
            await LocaleSelector.RestoreSavedAsync(token);

            if (token.IsCancellationRequested)
                return;

            _view.StartGameClicked += OnStartGameClicked;
            _view.ExitClicked += OnExitClicked;
            _view.EnglishClicked += OnEnglishClicked;
            _view.RussianClicked += OnRussianClicked;
            _view.Show();
        }

        private void OnStartGameClicked()
        {
            _navigationService.LoadGameFromMenu();
        }

        private void OnExitClicked() => _navigationService.QuitGame();

        private void OnEnglishClicked() => LocaleSelector.ApplyAsync(EnglishLocaleCode, _localeTokenSource.Token).Forget();

        private void OnRussianClicked() => LocaleSelector.ApplyAsync(RussianLocaleCode, _localeTokenSource.Token).Forget();
    }
}