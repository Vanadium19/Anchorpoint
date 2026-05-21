using System;
using Zenject;

namespace MenuModule
{
    public class PauseMenuPresenter : IInitializable, IDisposable
    {
        private readonly PauseMenuView _view;

        private readonly IGamePauseService _pauseService;
        private readonly IGameNavigationService _navigationService;

        public PauseMenuPresenter(PauseMenuView view,
            IGamePauseService pauseService,
            IGameNavigationService navigationService)
        {
            _view = view;
            _pauseService = pauseService;
            _navigationService = navigationService;
        }

        public void Initialize()
        {
            _view.Hide();

            _view.MainMenuClicked += OnMainMenuClicked;

            _pauseService.PauseStateChanged += OnPauseStateChanged;
        }

        public void Dispose()
        {
            _view.MainMenuClicked -= OnMainMenuClicked;

            _pauseService.PauseStateChanged -= OnPauseStateChanged;
        }

        private void OnMainMenuClicked() => _navigationService.LoadMainMenu();

        private void OnPauseStateChanged(bool isPaused) => ApplyUserPauseState();

        private void ApplyUserPauseState()
        {
            if (_pauseService.HasReason(GamePauseReason.UserPause))
            {
                _view.Show();
                return;
            }

            _view.Hide();
        }
    }
}