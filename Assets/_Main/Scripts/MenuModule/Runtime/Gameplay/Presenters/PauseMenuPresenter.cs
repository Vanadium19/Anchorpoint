using BaseModule;
using System;
using Zenject;

namespace MenuModule
{
    public class PauseMenuPresenter : IInitializable, IDisposable
    {
        private readonly PauseMenuView _view;

        private readonly IPauseManager _pauseService;
        private readonly IMenuNavigationService _navigationService;

        public PauseMenuPresenter(PauseMenuView view,
            IPauseManager pauseService,
            IMenuNavigationService navigationService)
        {
            _view = view;
            _pauseService = pauseService;
            _navigationService = navigationService;
        }

        public void Initialize()
        {
            if (_view == null)
                return;

            _view.Hide();

            _view.MainMenuClicked += OnMainMenuClicked;

            _pauseService.PauseStateChanged += OnPauseStateChanged;
        }

        public void Dispose()
        {
            if (_view != null)
                _view.MainMenuClicked -= OnMainMenuClicked;

            _pauseService.PauseStateChanged -= OnPauseStateChanged;
        }

        private void OnMainMenuClicked() => _navigationService.LoadMainMenu();

        private void OnPauseStateChanged(bool isPaused) => ApplyUserPauseState();

        private void ApplyUserPauseState()
        {
            if (_view == null)
                return;

            if (_pauseService.HasReason(PauseReason.UserPause))
            {
                _view.Show();
                return;
            }

            _view.Hide();
        }
    }
}
